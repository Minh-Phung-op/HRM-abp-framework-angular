using Acme.HRM.Dtos;
using Acme.HRM.Entities;
using Acme.HRM.Enums;
using Acme.HRM.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Acme.HRM.ApplicationServices
{
    [Authorize(HRMPermissions.LeaveBalances.Default)]
    public class LeaveBalanceAppService : CrudAppService<
        LeaveBalance,
        LeaveBalanceDto,
        long,
        GetAllLeaveBalancesInput,
        CreateUpdateLeaveBalanceDto>,
        ILeaveBalanceAppService // 🔥 BỔ SUNG: Thực thi Interface chính thức
    {
        private readonly IRepository<Employee, long> _employeeRepository;
        private readonly IRepository<LeaveType, long> _leaveTypeRepository;
        private readonly IRepository<LeaveRequest, long> _leaveRequestRepository;

        public LeaveBalanceAppService(
            IRepository<LeaveBalance, long> repository,
            IRepository<Employee, long> employeeRepository,
            IRepository<LeaveType, long> leaveTypeRepository,
            IRepository<LeaveRequest, long> leaveRequestRepository
            
            )
            : base(repository)
        {
            _employeeRepository = employeeRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _leaveRequestRepository = leaveRequestRepository;

            // Cấu hình phân quyền dựa trên Policy của ABP
            GetPolicyName = HRMPermissions.LeaveBalances.Default;
            GetListPolicyName = HRMPermissions.LeaveBalances.Default;
            CreatePolicyName = HRMPermissions.LeaveBalances.Manage;
            UpdatePolicyName = HRMPermissions.LeaveBalances.Manage;
            DeletePolicyName = HRMPermissions.LeaveBalances.Manage;
        }

        // ── GET SINGLE ──────────────────────────────────────────────
        public override async Task<LeaveBalanceDto> GetAsync(long id)
        {
            var query = await Repository.WithDetailsAsync(
                x => x.Employee,
                x => x.LeaveType
            );

            var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);

            if (entity == null)
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(LeaveBalance), id);

            return ObjectMapper.Map<LeaveBalance, LeaveBalanceDto>(entity);
        }

        // ── GET LIST (Tích hợp bảo mật Data Scope) ───────────────────
        public override async Task<PagedResultDto<LeaveBalanceDto>> GetListAsync(GetAllLeaveBalancesInput input)
        {
            var query = await Repository.WithDetailsAsync(
                x => x.Employee,
                x => x.LeaveType
            );

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Employee.FullName.Contains(input.Keyword) || x.Employee.EmployeeCode.Contains(input.Keyword))
                .WhereIf(input.EmployeeId.HasValue, x => x.EmployeeId == input.EmployeeId)
                .WhereIf(input.DepartmentId.HasValue, x => x.Employee.DepartmentId == input.DepartmentId)
                .WhereIf(input.LeaveTypeId.HasValue, x => x.LeaveTypeId == input.LeaveTypeId)
                .WhereIf(input.Year.HasValue, x => x.Year == input.Year);

            // 🔥 BỔ SUNG: Nhân viên thường chỉ được xem quỹ phép của chính mình
            query = await ApplyDataScopeFilterAsync(query);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Employee.FullName, LeaveType.Name" : input.Sorting)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<LeaveBalanceDto>(
                totalCount,
                ObjectMapper.Map<List<LeaveBalance>, List<LeaveBalanceDto>>(items)
            );
        }

        // API phục vụ riêng cho dropdown/info khi làm đơn nghỉ phép
        public async Task<List<LeaveBalanceDto>> GetMyCurrentBalancesAsync()
        {
            var employeeId = await GetCurrentEmployeeIdAsync(); // Hàm helper bạn đã viết ở service trước
            var currentYear = DateTime.Now.Year;

            var query = await Repository.WithDetailsAsync(x => x.LeaveType);
            var balances = await AsyncExecuter.ToListAsync(
                query.Where(x => x.EmployeeId == employeeId && x.Year == currentYear)
            );

            return ObjectMapper.Map<List<LeaveBalance>, List<LeaveBalanceDto>>(balances);
        }

        // ── CREATE ───────────────────────────────────────────────────
        public override async Task<LeaveBalanceDto> CreateAsync(CreateUpdateLeaveBalanceDto input)
        {
            // Kiểm tra trùng lặp khóa logic
            var exists = await Repository.AnyAsync(x =>
                x.EmployeeId == input.EmployeeId &&
                x.LeaveTypeId == input.LeaveTypeId &&
                x.Year == input.Year);

            if (exists)
                throw new UserFriendlyException("Đã tồn tại số dư nghỉ phép cho nhân viên này trong năm đã chọn.");

            var entity = ObjectMapper.Map<CreateUpdateLeaveBalanceDto, LeaveBalance>(input);
            entity.UsedDays = 0;
            entity.PendingDays = 0;

            await Repository.InsertAsync(entity, autoSave: true);
            return await GetAsync(entity.Id);
        }

        // ── 🔥 BỔ SUNG OVERRIDE: UPDATE (Bảo vệ dữ liệu gốc) ─────────
        public override async Task<LeaveBalanceDto> UpdateAsync(long id, CreateUpdateLeaveBalanceDto input)
        {
            var entity = await Repository.GetAsync(id);

            // Chặn đứng hành vi đổi Employee/LeaveType/Year trên bản ghi cũ
            if (entity.EmployeeId != input.EmployeeId || entity.LeaveTypeId != input.LeaveTypeId || entity.Year != input.Year)
            {
                throw new UserFriendlyException("Không được phép thay đổi Nhân viên, Loại nghỉ hoặc Năm của quỹ phép đã thiết lập.");
            }

            // Giữ nguyên các trường UsedDays và PendingDays do hệ thống tự tính qua Đơn xin nghỉ
            var currentUsed = entity.UsedDays;
            var currentPending = entity.PendingDays;

            ObjectMapper.Map(input, entity);

            entity.UsedDays = currentUsed;
            entity.PendingDays = currentPending;

            await Repository.UpdateAsync(entity, autoSave: true);
            return await GetAsync(id);
        }

        // ── 🔥 BỔ SUNG: KHỞI TẠO PHÉP NĂM LOẠT LOẠT (BULK INITIALIZE) ──
        [Authorize(HRMPermissions.LeaveBalances.Manage)]
        public async Task BulkInitializeYearlyAsync(BulkInitializeLeaveBalanceDto input)
        {
            // 1. Lấy thông tin loại phép (để lấy DefaultDays cấu hình sẵn trong LeaveType)
            var leaveType = await _leaveTypeRepository.GetAsync(input.LeaveTypeId);
            // 2. Xác định số ngày sẽ cấp: 
            // Logic mới: Nếu không nhập (null) HOẶC nhập bằng 0 -> Lấy từ LeaveType
            decimal daysToAssign = (input.DefaultDays.HasValue && input.DefaultDays.Value > 0)
                                   ? input.DefaultDays.Value
                                   : leaveType.DefaultDaysPerYear;
            // Lấy danh sách ID nhân viên đã có balance cho năm này rồi (chỉ 1 câu lệnh SQL)
            var existingEmployeeIds = (await Repository.GetListAsync(x =>
                x.LeaveTypeId == input.LeaveTypeId && x.Year == input.Year))
                .Select(x => x.EmployeeId).ToList();
            var activeEmployees = await _employeeRepository.GetListAsync(x => x.Status == EmployeeStatus.Active);
            var newBalances = new List<LeaveBalance>();

            foreach (var emp in activeEmployees)
            {
                var isExist = await Repository.AnyAsync(x =>
                    x.EmployeeId == emp.Id && x.LeaveTypeId == input.LeaveTypeId && x.Year == input.Year);

                if (!existingEmployeeIds.Contains(emp.Id))
                {
                    newBalances.Add(new LeaveBalance
                    {
                        EmployeeId = emp.Id,
                        LeaveTypeId = input.LeaveTypeId,
                        Year = input.Year,
                        AllocatedDays = daysToAssign,
                        CarriedOverDays = 0,
                        UsedDays = 0,
                        PendingDays = 0
                        // Không cần gán TotalDays vì nó tự tính
                    });
                }
            }

            if (newBalances.Any())
            {
                await Repository.InsertManyAsync(newBalances, autoSave: true);
            }
        }

        // ── 🔥 BỔ SUNG: ĐIỀU CHỈNH PHÉP THỦ CÔNG (ADJUSTMENT) ─────────
        [Authorize(HRMPermissions.LeaveBalances.Manage)]
        public async Task AdjustBalanceAsync(long id, AdjustLeaveBalanceDto input)
        {
            var balance = await Repository.GetAsync(id);

            // Cộng hoặc trừ trực tiếp vào số ngày được cấu hình
            balance.AllocatedDays += input.AdjustmentDays;

            if (balance.AllocatedDays < 0)
            {
                throw new UserFriendlyException("Thao tác thất bại! Số ngày phép được cấp không thể nhỏ hơn 0.");
            }

            await Repository.UpdateAsync(balance, autoSave: true);
        }

        // ── 🔒 PRIVATE HELPER METHODS ────────────────────────────────
        private async Task<IQueryable<LeaveBalance>> ApplyDataScopeFilterAsync(IQueryable<LeaveBalance> query)
        {
            var isHR = await AuthorizationService.IsGrantedAsync(HRMPermissions.LeaveBalances.Manage);

            // Nếu không phải quản lý/HR, ép điều kiện chỉ xem số dư của mình
            if (!isHR && CurrentUser.Id.HasValue)
            {
                var employee = await _employeeRepository.FirstOrDefaultAsync(x => x.UserId == CurrentUser.Id);
                if (employee != null)
                {
                    query = query.Where(x => x.EmployeeId == employee.Id);
                }
                else
                {
                    // Tài khoản rác chưa liên kết hồ sơ nhân sự -> Trả về query rỗng
                    query = query.Where(x => false);
                }
            }
            return query;
        }

        private async Task<long> GetCurrentEmployeeIdAsync()
        {
            if (!CurrentUser.Id.HasValue)
                throw new UnauthorizedAccessException("Bạn chưa đăng nhập vào hệ thống.");

            // Sửa logic mapping từ CreatorId thành UserId chính xác
            var employee = await _employeeRepository.FirstOrDefaultAsync(x => x.UserId == CurrentUser.Id);

            if (employee == null)
                throw new UserFriendlyException("Tài khoản hệ thống của bạn chưa được liên kết với bất kỳ mã nhân sự nào.");

            return employee.Id;
        }

        [Authorize(HRMPermissions.LeaveBalances.Manage)]
        public async Task RecalculateBalanceAsync(long id)
        {
            var balance = await Repository.GetAsync(id);

            // Giả sử bạn inject IRepository<LeaveRequest> vào đây
            var requests = await _leaveRequestRepository.GetListAsync(x =>
                x.EmployeeId == balance.EmployeeId &&
                x.LeaveTypeId == balance.LeaveTypeId &&
                x.StartDate.Year == balance.Year);

            balance.UsedDays = requests.Where(x => x.Status == LeaveRequestStatus.Approved).Sum(x => x.TotalDays);
            balance.PendingDays = requests.Where(x => x.Status == LeaveRequestStatus.Pending).Sum(x => x.TotalDays);

            await Repository.UpdateAsync(balance);
        }
    }
}