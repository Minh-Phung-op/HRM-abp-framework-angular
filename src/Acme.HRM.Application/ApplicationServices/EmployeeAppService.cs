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
using Volo.Abp.Identity; // Thêm thư viện quản lý Identity của ABP

namespace Acme.HRM.ApplicationServices
{
    [Authorize(HRMPermissions.Employees.Default)]
    public class EmployeeAppService : CrudAppService<
        Employee,
        EmployeeDto,
        long,
        GetAllEmployeesInput,
        CreateUpdateEmployeeDto>,
        IEmployeeAppService // 🔥 BỔ SUNG: Thực thi Interface
    {
        private readonly IRepository<Department, long> _departmentRepository;
        private readonly IRepository<Position, long> _positionRepository;
        private readonly IdentityUserManager _userManager; // 🔥 INJECT: Quản lý User của ABP
        private readonly IIdentityRoleRepository _roleRepository;

        public EmployeeAppService(
            IRepository<Employee, long> repository,
            IRepository<Department, long> departmentRepository,
            IRepository<Position, long> positionRepository,
            IdentityUserManager userManager,
            IIdentityRoleRepository roleRepository)
            : base(repository)
        {
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;
            _userManager = userManager;

            // Cấu hình phân quyền mặc định của ABP cho các hàm CRUD
            GetPolicyName = HRMPermissions.Employees.Default;
            GetListPolicyName = HRMPermissions.Employees.Default;
            CreatePolicyName = HRMPermissions.Employees.Create;
            UpdatePolicyName = HRMPermissions.Employees.Update;
            DeletePolicyName = HRMPermissions.Employees.Delete;
            _roleRepository = roleRepository;
        }

        // ── GET SINGLE ──────────────────────────────────────────────
        public override async Task<EmployeeDto> GetAsync(long id)
        {
            var query = await Repository.WithDetailsAsync(
                x => x.Department,
                x => x.Position,
                x => x.Manager
            );

            var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);

            if (entity == null)
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Employee), id);

            return ObjectMapper.Map<Employee, EmployeeDto>(entity);
        }

        // ── GET LIST ─────────────────────────────────────────────────
        public override async Task<PagedResultDto<EmployeeDto>> GetListAsync(GetAllEmployeesInput input)
        {
            var query = await Repository.WithDetailsAsync(
                x => x.Department,
                x => x.Position,
                x => x.Manager,
                x => x.User
            );

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.FullName.Contains(input.Keyword)
                      || x.Email.Contains(input.Keyword)
                      || x.EmployeeCode.Contains(input.Keyword))
                .WhereIf(input.DepartmentId.HasValue, x => x.DepartmentId == input.DepartmentId)
                .WhereIf(input.PositionId.HasValue, x => x.PositionId == input.PositionId)
                .WhereIf(input.ManagerId.HasValue, x => x.ManagerId == input.ManagerId)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                .WhereIf(input.Gender.HasValue, x => x.Gender == input.Gender)
                .WhereIf(input.StartDateFrom.HasValue, x => x.StartDate >= input.StartDateFrom)
                .WhereIf(input.StartDateTo.HasValue, x => x.StartDate <= input.StartDateTo);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(Employee.FullName) : input.Sorting)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );
            var dtos = ObjectMapper.Map<List<Employee>, List<EmployeeDto>>(items);
            for (int i = 0; i < items.Count; i++)
            {
                var user = items[i].User;
                if (user != null)
                {
                    dtos[i].Roles = (await _userManager.GetRolesAsync(user)).ToList();
                }
            }

            return new PagedResultDto<EmployeeDto>(
                totalCount,
                dtos
            );
        }

        // ── CREATE ───────────────────────────────────────────────────
        public override async Task<EmployeeDto> CreateAsync(CreateUpdateEmployeeDto input)
        {
            await ValidateInputAsync(input, existingEmployeeId: null);

            var employee = ObjectMapper.Map<CreateUpdateEmployeeDto, Employee>(input);
            employee.Status = EmployeeStatus.Active; // Mặc định trạng thái ban đầu
            employee.UserId = null;     // Chưa cấp tài khoản đăng nhập

            await Repository.InsertAsync(employee, autoSave: true);
            return await GetAsync(employee.Id);
        }

        // ── UPDATE ───────────────────────────────────────────────────
        public override async Task<EmployeeDto> UpdateAsync(long id, CreateUpdateEmployeeDto input)
        {
            await ValidateInputAsync(input, existingEmployeeId: id);

            var employee = await Repository.GetAsync(id);
            ObjectMapper.Map(input, employee);

            await Repository.UpdateAsync(employee, autoSave: true);
            return await GetAsync(id);
        }

        // ── DELETE ───────────────────────────────────────────────────
        public override async Task DeleteAsync(long id)
        {
            var isDeptManager = await _departmentRepository.AnyAsync(x => x.ManagerId == id);
            if (isDeptManager)
                throw new UserFriendlyException("Không thể xóa nhân viên đang là trưởng phòng. Vui lòng cập nhật phòng ban trước.");

            var hasSubordinates = await Repository.AnyAsync(x => x.ManagerId == id);
            if (hasSubordinates)
                throw new UserFriendlyException("Không thể xóa nhân viên đang quản lý nhân viên khác. Vui lòng gán lại quản lý trước.");

            // 💡 Nghiệp vụ mở rộng: Nếu xóa hồ sơ nhân viên, xóa luôn tài khoản hệ thống của họ nếu có
            var employee = await Repository.GetAsync(id);
            if (employee.UserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(employee.UserId.Value.ToString());
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }

            await Repository.DeleteAsync(id, autoSave: true);
        }

        public async Task<List<string>> GetAssignableRolesAsync()
        {
            var allRoles = (await _roleRepository.GetListAsync()).Select(r => r.Name).ToList();

            // 1. Nếu là Admin: Chỉ được thấy và cấp quyền HR
            if (CurrentUser.IsInRole("admin"))
            {
                return allRoles.Where(r => r.Contains("HR")).ToList();
            }

            // 2. Nếu là HR: Được cấp các quyền còn lại (trừ admin và chính HR)
            if (CurrentUser.IsInRole("HR"))
            {
                return allRoles.Where(r => r != "admin" && !r.Contains("HR")).ToList();
            }

            return new List<string>();
        }

        // ── 🔥 BỔ SUNG: CẤP TÀI KHOẢN CHO NHÂN VIÊN (HRM LUỒNG 1) ─────
        [Authorize(HRMPermissions.Employees.Create)] // Quyền HR mới được gọi
        public async Task CreateAccountForEmployeeAsync(long employeeId, string email, string password, string roleName)
        {
            // 1. Kiểm tra xem Employee có tồn tại không
            var employee = await Repository.GetAsync(employeeId);
            // chặn tạo nếu có tài khoản rồi
            if (employee.UserId.HasValue)
            {
                throw new UserFriendlyException("Nhân viên này đã có tài khoản hệ thống!");
            }

            // 2. Tạo đối tượng AbpUser bằng Identity của ABP
            var identityUser = new IdentityUser(
                GuidGenerator.Create(),
                userName: email, // Sử dụng email làm UserName luôn cho tiện
                email: email
            );

            // 3. Sử dụng _userManager để tạo User kèm Password (ABP tự băm mật khẩu)
            var result = await _userManager.CreateAsync(identityUser, password);

            if (!result.Succeeded)
            {
                throw new UserFriendlyException("Không thể tạo tài khoản: " + string.Join(", ", result.Errors));
            }

            // Gán Role được truyền từ Frontend
            if (!string.IsNullOrEmpty(roleName))
            {
                await _userManager.AddToRoleAsync(identityUser, roleName);
            }

            // 5. Cập nhật UserId ngược lại cho bảng Employee để liên kết hai bảng
            employee.UserId = identityUser.Id;
            await Repository.UpdateAsync(employee);
        }

        // ── 🔥 BỔ SUNG: CHO THÔI VIỆC (OFFBOARD) ─────────────────────
        [Authorize(HRMPermissions.Employees.Update)]
        public async Task OffboardAsync(long id, DateOnly terminationDate)
        {
            var query = await Repository.WithDetailsAsync(
                x => x.Contracts
            );
            var employee = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);
            if (employee == null)
            {
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Employee), id);
            }
            // ── 1. CẬP NHẬT TRẠNG THÁI HỢP ĐỒNG ──────────────────────────────
            // Tìm hợp đồng đang Active hoặc hợp đồng có thời gian gối lên ngày nghỉ việc
            var activeContract = employee.Contracts
                .Where(c => c.Status == ContractStatus.Active)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefault();

            if (activeContract != null)
            {
                activeContract.EndDate = terminationDate; // Chốt ngày kết thúc hợp đồng thực tế
                activeContract.Status = ContractStatus.Expired; // Hoặc chuyển sang Expired tùy bạn định nghĩa Enum

                // Cập nhật thông qua Repository của Contract (Nếu bạn đã inject)
                // Hoặc nhờ tính chất Aggregate của EF Core, khi Update Employee thì Contract con cũng tự động lưu
            }

            employee.Status = EmployeeStatus.Terminated; // Chuyển trạng thái nghỉ việc

            // Kèm logic bảo mật: Khóa ngay tài khoản đăng nhập để tránh rò rỉ dữ liệu công ty
            if (employee.UserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(employee.UserId.Value.ToString());
                if (user != null)
                {
                    // Bước A: Khóa tài khoản vĩnh viễn (Không cho đăng nhập mới nữa)
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

                    // Bước B: 🔥 VIẾT Ở ĐÂY ── Đổi dấu cấu hình bảo mật (Ép văng ra khỏi hệ thống ngay lập tức)
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }

            await Repository.UpdateAsync(employee, autoSave: true);
        }

        // ── 🔥 BỔ SUNG: NHÂN VIÊN TỰ XEM HỒ SƠ (MY PROFILE) ──────────
        [AllowAnonymous] // Cho phép gọi không cần check quyền hạt nhân, tự bảo mật dựa vào token đăng nhập
        public async Task<EmployeeDto> GetMyProfileAsync()
        {
            if (!CurrentUser.Id.HasValue)
                throw new UnauthorizedAccessException("Bạn chưa đăng nhập vào hệ thống.");

            var query = await Repository.WithDetailsAsync(
                x => x.Department,
                x => x.Position,
                x => x.Manager
            );

            var employee = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.UserId == CurrentUser.Id);
            if (employee == null)
                throw new UserFriendlyException("Tài khoản của bạn chưa được liên kết với hồ sơ nhân sự nào.");

            return ObjectMapper.Map<Employee, EmployeeDto>(employee);
        }

        // ── PRIVATE VALIDATION ───────────────────────────────────────
        private async Task ValidateInputAsync(CreateUpdateEmployeeDto input, long? existingEmployeeId)
        {
            var duplicateCode = await Repository.AnyAsync(x =>
                x.EmployeeCode == input.EmployeeCode &&
                (!existingEmployeeId.HasValue || x.Id != existingEmployeeId));

            if (duplicateCode)
                throw new UserFriendlyException($"Mã nhân viên '{input.EmployeeCode}' đã tồn tại.");

            var duplicateEmail = await Repository.AnyAsync(x =>
                x.Email == input.Email &&
                (!existingEmployeeId.HasValue || x.Id != existingEmployeeId));

            if (duplicateEmail)
                throw new UserFriendlyException($"Email '{input.Email}' đã được sử dụng.");

            var dept = await _departmentRepository.FirstOrDefaultAsync(x => x.Id == input.DepartmentId);
            if (dept == null)
                throw new UserFriendlyException("Phòng ban không tồn tại.");
            if (!dept.IsActive)
                throw new UserFriendlyException("Phòng ban đã bị vô hiệu hóa.");

            var position = await _positionRepository.FirstOrDefaultAsync(x => x.Id == input.PositionId);
            if (position == null)
                throw new UserFriendlyException("Chức vụ không tồn tại.");
            if (position.DepartmentId != input.DepartmentId)
                throw new UserFriendlyException("Chức vụ không thuộc phòng ban đã chọn.");

            if (input.ManagerId.HasValue)
            {
                if (existingEmployeeId.HasValue && input.ManagerId == existingEmployeeId)
                    throw new UserFriendlyException("Nhân viên không thể là quản lý của chính mình.");

                var managerExists = await Repository.AnyAsync(x => x.Id == input.ManagerId);
                if (!managerExists)
                    throw new UserFriendlyException("Quản lý trực tiếp không tồn tại.");
            }
        }
    }
}