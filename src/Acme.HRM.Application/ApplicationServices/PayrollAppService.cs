using Acme.HRM.DomainServices;
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
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Acme.HRM.ApplicationServices
{
    [Authorize(HRMPermissions.Payrolls.Default)]
    public class PayrollAppService : ApplicationService, IPayrollAppService
    {
        private readonly IRepository<Payroll, long> _payrollRepository;
        private readonly IRepository<PayrollItem, long> _payrollItemRepository;
        private readonly IRepository<Employee, long> _employeeRepository;
        private readonly PayrollManager _payrollManager; // Gọi Domain Service
        private readonly IRepository<Attendance, long> _attendanceRepository; // Gọi Domain Service
        private readonly IRepository<LeaveRequest, long> _leaveRequestRepository; // Gọi Domain Service

        public PayrollAppService(
            IRepository<Payroll, long> payrollRepository,
            IRepository<PayrollItem, long> payrollItemRepository,
            IRepository<Employee, long> employeeRepository,
            PayrollManager payrollManager,
            IRepository<Attendance, long> attendanceRepository,
            IRepository<LeaveRequest, long> leaveRepository
        )
        {
            _payrollRepository = payrollRepository;
            _payrollItemRepository = payrollItemRepository;
            _employeeRepository = employeeRepository;
            _payrollManager = payrollManager;
            _attendanceRepository = attendanceRepository;

        }

        // ── GET SINGLE (Bảo mật: Nhân viên chỉ được xem lương của mình) ──
        public async Task<PayrollDto> GetAsync(long id)
        {
            var payroll = await GetPayrollWithNavigationAsync(id);

            // Chốt chặn bảo mật thông tin thu nhập
            var isHR = await AuthorizationService.IsGrantedAsync(HRMPermissions.Payrolls.Management);
            if (!isHR)
            {
                var currentEmployee = await _employeeRepository.FirstOrDefaultAsync(x => x.UserId == CurrentUser.Id);
                if (currentEmployee == null || payroll.EmployeeId != currentEmployee.Id)
                {
                    throw new UserFriendlyException("Bạn không có đặc quyền truy cập phiếu lương của nhân sự khác.");
                }
            }

            var dto = ObjectMapper.Map<Payroll, PayrollDto>(payroll);
            var items = await _payrollItemRepository.GetListAsync(x => x.PayrollId == id);
            dto.Items = ObjectMapper.Map<List<PayrollItem>, List<PayrollItemDto>>(items);

            return dto;
        }

        // ── GET LIST (Phân Scope dữ liệu thông minh) ─────────────────
        public async Task<PagedResultDto<PayrollDto>> GetListAsync(GetAllPayrollsInput input)
        {
            var query = await _payrollRepository.WithDetailsAsync(
                x => x.Employee, 
                x => x.Employee.Department, 
                x => x.Employee.Position
            );

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Employee.FullName.Contains(input.Keyword) || x.Employee.EmployeeCode.Contains(input.Keyword))
                .WhereIf(input.EmployeeId.HasValue, x => x.EmployeeId == input.EmployeeId)
                .WhereIf(input.DepartmentId.HasValue, x => x.Employee.DepartmentId == input.DepartmentId)
                .WhereIf(input.Year.HasValue, x => x.Year == input.Year)
                .WhereIf(input.Month.HasValue, x => x.Month == input.Month)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
                .WhereIf(input.NetSalaryFrom.HasValue, x => x.NetSalary >= input.NetSalaryFrom)
                .WhereIf(input.NetSalaryTo.HasValue, x => x.NetSalary <= input.NetSalaryTo);

            // Nếu không phải là kế toán/HR, tự động ép điều kiện chỉ thấy bảng lương của chính mình
            var isHR = CurrentUser.UserName == "admin" || await AuthorizationService.IsGrantedAsync(HRMPermissions.Payrolls.Management); 
            //if (!isHR)
            //{
            //    var currentEmployee = await _employeeRepository.FirstOrDefaultAsync(x => x.UserId == CurrentUser.Id);
            //    query = query.Where(x => x.EmployeeId == (currentEmployee != null ? currentEmployee.Id : -1));
            //}

            var totalCount = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query
                .OrderBy(string.IsNullOrWhiteSpace(input.Sorting)
                    ? "Year desc, Month desc, Employee.FullName"
                    : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

            return new PagedResultDto<PayrollDto>(
                totalCount,
                ObjectMapper.Map<List<Payroll>, List<PayrollDto>>(items)
            );
        }

        // ── CREATE MANUAL (Tạo đơn lẻ) ────────────────────────────────
        [Authorize(HRMPermissions.Payrolls.Management)]
        public async Task<PayrollDto> CreateAsync(CreatePayrollDto input)
        {
            await _payrollManager.EnsureNotDuplicateAsync(input.EmployeeId, input.Year, input.Month);

            var payroll = ObjectMapper.Map<CreatePayrollDto, Payroll>(input);
            payroll.Status = PayrollStatus.Draft;
            payroll.Items = ObjectMapper.Map<List<CreateUpdatePayrollItemDto>, List<PayrollItem>>(input.Items);

            // Thực thi tính toán tại Domain
            _payrollManager.ExecuteSalaryCalculation(payroll);

            await _payrollRepository.InsertAsync(payroll, autoSave: true);
            return await GetAsync(payroll.Id);
        }

        // ── UPDATE (Cập nhật bảng lương nháp) ──────────────────────────
        [Authorize(HRMPermissions.Payrolls.Management)]
        public async Task<PayrollDto> UpdateAsync(long id, UpdatePayrollDto input)
        {
            // Lấy danh sách với include navigation properties (Items)
            var query = await _payrollRepository.WithDetailsAsync(x => x.Items);

            // Lọc theo id
            var payroll = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);
            if (payroll == null) throw new EntityNotFoundException(typeof(Payroll), id);

            if (payroll.Status != PayrollStatus.Draft || payroll.LockedAt.HasValue)
            {
                throw new UserFriendlyException("Bảng lương đã bị khóa hoặc không còn ở trạng thái Bản nháp (Draft).");
            }

            payroll.BaseSalary = input.BaseSalary;

            // Xóa danh sách Item cũ để tránh xung đột dữ liệu liên kết
            payroll.Items.Clear();
            var newItems = ObjectMapper.Map<List<CreateUpdatePayrollItemDto>, List<PayrollItem>>(input.Items);
            foreach (var item in newItems)
            {
                payroll.Items.Add(item);
            }

            // Gọi Domain Service tính toán lại dòng tiền
            _payrollManager.ExecuteSalaryCalculation(payroll);

            await _payrollRepository.UpdateAsync(payroll, autoSave: true);
            return await GetAsync(id);
        }

        private async Task<decimal> CalculateWorkRateAsync(long employeeId, int year, int month)
        {
            // 1. Tính số ngày công chuẩn của tháng (Ví dụ: 22 ngày)
            int totalStandardWorkingDays = 22;

            // 2. Lấy tất cả đơn nghỉ phép đã duyệt của nhân viên trong tháng này
            // Cần Include thêm LeaveType để check Code
            var approvedLeaves = await _leaveRequestRepository.WithDetailsAsync(x => x.LeaveType);
            var monthlyLeaves = await AsyncExecuter.ToListAsync(
                approvedLeaves.Where(x =>
                    x.EmployeeId == employeeId &&
                    x.Status == LeaveRequestStatus.Approved &&
                    x.StartDate.Month == month && x.StartDate.Year == year)
            );

            // 3. Phân loại và tính tổng ngày nghỉ có lương bằng Code
            decimal paidLeaveDays = monthlyLeaves
                .Where(x => x.LeaveType.Code == "AL" || x.LeaveType.Code == "PH") // AL: Phép năm, PH: Nghỉ lễ
                .Sum(x => x.TotalDays);

            // 4. Lấy số ngày đi làm thực tế từ Attendance
            var presentDays = await _attendanceRepository.CountAsync(x =>
                x.EmployeeId == employeeId &&
                x.WorkDate.Month == month &&
                x.WorkDate.Year == year &&
                x.Status == AttendanceStatus.Present);

            // Tổng ngày được trả lương = Đi làm + Nghỉ có lương
            decimal actualWorkingDays = (decimal)presentDays + paidLeaveDays;

            return actualWorkingDays / totalStandardWorkingDays;
        }

        // ── GENERATE BATCH (Tạo bảng lương tự động hàng loạt) ───────────
        // ── GENERATE BATCH (Tạo bảng lương tự động hàng loạt) ───────────
        [Authorize(HRMPermissions.Payrolls.Management)]
        public async Task GenerateAsync(GeneratePayrollInput input)
        {
            var query = await _employeeRepository.WithDetailsAsync(x => x.Contracts);
            query = query.Where(x => x.Status == EmployeeStatus.Active);

            // FIX: Chỉ lọc nếu DepartmentId có giá trị, nếu null thì bỏ qua (lấy tất cả)
            if (input.DepartmentId.HasValue && input.DepartmentId.Value > 0)
            {
                query = query.Where(x => x.DepartmentId == input.DepartmentId.Value);
            }

            var employees = await AsyncExecuter.ToListAsync(query);
            var targetStartDate = new DateOnly(input.Year, input.Month, 1);
            var targetEndDate = new DateOnly(input.Year, input.Month, DateTime.DaysInMonth(input.Year, input.Month));

            foreach (var employee in employees)
            {
                var exists = await _payrollRepository.AnyAsync(x =>
                    x.EmployeeId == employee.Id && x.Year == input.Year && x.Month == input.Month);
                if (exists) continue;

                var activeContract = employee.Contracts
                    .Where(c => c.Status == ContractStatus.Active)
                    .Where(c => c.StartDate <= targetEndDate && (c.EndDate == null || c.EndDate >= targetStartDate))
                    .OrderByDescending(c => c.StartDate)
                    .FirstOrDefault();

                if (activeContract == null) continue; // Hoặc Throw lỗi tùy bạn

                // 🔥 BỔ SUNG: Tính tỷ lệ hưởng lương dựa trên ngày công
                decimal workRate = await CalculateWorkRateAsync(employee.Id, input.Year, input.Month);

                var payroll = new Payroll
                {
                    EmployeeId = employee.Id,
                    Year = input.Year,
                    Month = input.Month,
                    // 🔥 Lương cơ bản tháng này = Lương hợp đồng * Tỷ lệ công
                    BaseSalary = Math.Round(activeContract.BasicSalary * workRate, 0),
                    Status = PayrollStatus.Draft
                };

                _payrollManager.ExecuteSalaryCalculation(payroll);
                await _payrollRepository.InsertAsync(payroll, autoSave: true);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        // ── WORKFLOW & CHUYỂN TRẠNG THÁI (Đồng bộ Enum hoàn toàn) ────────
        [Authorize(HRMPermissions.Payrolls.Management)]
        public async Task<PayrollDto> SubmitAsync(long id) => await UpdateStatusAsync(id, PayrollStatus.Draft, PayrollStatus.Processing);

        [Authorize(HRMPermissions.Payrolls.Management)]
        public async Task<PayrollDto> ApproveAsync(long id) => await UpdateStatusAsync(id, PayrollStatus.Processing, PayrollStatus.Approved);

        [Authorize(HRMPermissions.Payrolls.Management)]
        public async Task<PayrollDto> LockAsync(long id)
        {
            var payroll = await _payrollRepository.GetAsync(id);
            if (payroll.LockedAt.HasValue) throw new UserFriendlyException("Bảng lương này đã được khóa từ trước.");

            payroll.LockedAt = Clock.Now;
            await _payrollRepository.UpdateAsync(payroll, autoSave: true);
            return await GetAsync(id);
        }

        [Authorize(HRMPermissions.Payrolls.Management)]
        public async Task<PayrollDto> MarkAsPaidAsync(long id)
        {
            var payroll = await _payrollRepository.GetAsync(id);
            if (payroll.Status != PayrollStatus.Approved) throw new UserFriendlyException("Chỉ quyết toán chi trả cho bảng lương đã duyệt.");

            payroll.Status = PayrollStatus.Paid;
            payroll.PaidAt = Clock.Now;
            await _payrollRepository.UpdateAsync(payroll, autoSave: true);
            return await GetAsync(id);
        }

        [Authorize(HRMPermissions.Payrolls.Management)]
        public async Task DeleteAsync(long id)
        {
            var payroll = await _payrollRepository.GetAsync(id);
            if (payroll.Status != PayrollStatus.Draft) throw new UserFriendlyException("Chỉ được xóa bảng lương ở trạng thái nháp.");
            await _payrollRepository.DeleteAsync(id, autoSave: true);
        }

        // ── PRIVATE HELPERS ──────────────────────────────────────────
        private async Task<Payroll> GetPayrollWithNavigationAsync(long id)
        {
            var query = await _payrollRepository.WithDetailsAsync(
                x => x.Employee,
                x => x.Employee.Department,
                x => x.Employee.Position
            );
            var payroll = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);

            if (payroll == null) throw new EntityNotFoundException(typeof(Payroll), id);
            return payroll;
        }

        private async Task<PayrollDto> UpdateStatusAsync(long id, PayrollStatus currentStatus, PayrollStatus nextStatus)
        {
            var payroll = await _payrollRepository.GetAsync(id);
            if (payroll.Status != currentStatus) throw new UserFriendlyException($"Hành động không hợp lệ. Trạng thái hiện tại không phải là {currentStatus}.");

            payroll.Status = nextStatus;
            await _payrollRepository.UpdateAsync(payroll, autoSave: true);
            return await GetAsync(id);  
        }
    }
}