using Acme.HRM.Dtos;
using Acme.HRM.Entities;
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
    [Authorize(HRMPermissions.Departments.Default)]
    public class DepartmentAppService : CrudAppService<
        Department,
        DepartmentDto,
        long,
        GetAllDepartmentsInput,
        CreateUpdateDepartmentDto>,
        IDepartmentAppService // Thực thi Interface quản lý
    {
        private readonly IRepository<Employee, long> _employeeRepository;
        private readonly IRepository<Position, long> _positionRepository;

        public DepartmentAppService(
            IRepository<Department, long> repository,
            IRepository<Employee, long> employeeRepository,
            IRepository<Position, long> positionRepository)
            : base(repository)
        {
            _employeeRepository = employeeRepository;
            _positionRepository = positionRepository;

            // 🔥 BỔ SUNG: Cấu hình phân quyền chuẩn cho các hành động CRUD
            GetPolicyName = HRMPermissions.Departments.Default;
            GetListPolicyName = HRMPermissions.Departments.Default;
            CreatePolicyName = HRMPermissions.Departments.Manage;
            UpdatePolicyName = HRMPermissions.Departments.Manage;
            DeletePolicyName = HRMPermissions.Departments.Manage;
        }

        // ── GET SINGLE ──────────────────────────────────────────────
        public override async Task<DepartmentDto> GetAsync(long id)
        {
            var query = await Repository.WithDetailsAsync(
                x => x.Parent,
                x => x.Manager
            );

            var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);

            if (entity == null)
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Department), id);

            return ObjectMapper.Map<Department, DepartmentDto>(entity);
        }

        // ── GET LIST ─────────────────────────────────────────────────
        public override async Task<PagedResultDto<DepartmentDto>> GetListAsync(GetAllDepartmentsInput input)
        {
            var query = await Repository.WithDetailsAsync(
                x => x.Parent,
                x => x.Manager
            );

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword))
                .WhereIf(input.ParentId.HasValue, x => x.ParentId == input.ParentId)
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(Department.Name) : input.Sorting)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<DepartmentDto>(
                totalCount,
                ObjectMapper.Map<List<Department>, List<DepartmentDto>>(items)
            );
        }

        // ── 🔥 OVERRIDE: CREATE (Thêm validate chặt chẽ) ─────────────
        public override async Task<DepartmentDto> CreateAsync(CreateUpdateDepartmentDto input)
        {
            await ValidateDepartmentInputAsync(input, currentId: null);

            var department = ObjectMapper.Map<CreateUpdateDepartmentDto, Department>(input);

            await Repository.InsertAsync(department, autoSave: true);
            return await GetAsync(department.Id);
        }

        // ── 🔥 OVERRIDE: UPDATE (Chặn đệ quy vòng lặp cây) ───────────
        public override async Task<DepartmentDto> UpdateAsync(long id, CreateUpdateDepartmentDto input)
        {
            await ValidateDepartmentInputAsync(input, currentId: id);

            var department = await Repository.GetAsync(id);
            ObjectMapper.Map(input, department);

            await Repository.UpdateAsync(department, autoSave: true);
            return await GetAsync(id);
        }

        // ── 🔥 OVERRIDE: DELETE (Chặn mồ côi dữ liệu) ────────────────
        public override async Task DeleteAsync(long id)
        {
            // 1. Chặn xóa nếu có phòng ban con đang trực thuộc
            var hasChildren = await Repository.AnyAsync(x => x.ParentId == id);
            if (hasChildren)
            {
                throw new UserFriendlyException("Không thể xóa phòng ban này vì đang có các phòng ban con trực thuộc. Vui lòng chuyển hoặc xóa phòng ban con trước!");
            }

            // 2. Chặn xóa nếu đang có nhân sự thuộc phòng ban này
            var hasEmployees = await _employeeRepository.AnyAsync(x => x.DepartmentId == id);
            if (hasEmployees)
            {
                throw new UserFriendlyException("Không thể xóa phòng ban đang có nhân viên làm việc. Vui lòng điều chuyển nhân sự sang phòng ban khác trước!");
            }

            // 3. Chặn xóa nếu đang có các Chức vụ (Position) bám vào phòng ban này
            var hasPositions = await _positionRepository.AnyAsync(x => x.DepartmentId == id);
            if (hasPositions)
            {
                throw new UserFriendlyException("Không thể xóa phòng ban đang chứa các chức danh/vị trí công việc. Vui lòng xóa các vị trí liên quan trước!");
            }

            await Repository.DeleteAsync(id, autoSave: true);
        }

        // ── 🔒 PRIVATE VALIDATION LOGIC ──────────────────────────────
        private async Task ValidateDepartmentInputAsync(CreateUpdateDepartmentDto input, long? currentId)
        {
            // 1. Kiểm tra trùng Mã phòng ban (Code)
            var isCodeDuplicate = await Repository.AnyAsync(x =>
                x.Code == input.Code && (!currentId.HasValue || x.Id != currentId.Value));

            if (isCodeDuplicate)
            {
                throw new UserFriendlyException($"Mã phòng ban '{input.Code}' đã tồn tại trên hệ thống!");
            }

            // 2. Kiểm tra tính hợp lệ của Phòng ban cha (ParentId)
            if (input.ParentId.HasValue)
            {
                if (currentId.HasValue && input.ParentId.Value == currentId.Value)
                {
                    throw new UserFriendlyException("Một phòng ban không thể chọn chính nó làm phòng ban cha!");
                }

                var parentExist = await Repository.AnyAsync(x => x.Id == input.ParentId.Value);
                if (!parentExist)
                {
                    throw new UserFriendlyException("Phòng ban cha được chọn không tồn tại!");
                }

                // 🔥 THUẬT TOÁN: Chặn vòng lặp đệ quy cây (Loop/Cyclic Dependency)
                if (currentId.HasValue)
                {
                    var checkParentId = input.ParentId;
                    while (checkParentId.HasValue)
                    {
                        // Nếu lội ngược dòng sơ đồ tổ chức mà gặp lại chính Id hiện tại -> Có vòng lặp vô hạn
                        if (checkParentId.Value == currentId.Value)
                        {
                            throw new UserFriendlyException("Lỗi cấu trúc: Không thể chọn phòng ban cấp dưới làm phòng ban cha của cấp trên!");
                        }

                        // Lấy bản ghi của ông cha để bốc tiếp ParentId của ông ấy lên check tiếp
                        var grandParent = await Repository.FirstOrDefaultAsync(x => x.Id == checkParentId.Value);
                        checkParentId = grandParent?.ParentId;
                    }
                }
            }

            // 3. Kiểm tra tính hợp lệ của Trưởng phòng (ManagerId) nếu có truyền vào
            if (input.ManagerId.HasValue)
            {
                var managerExist = await _employeeRepository.AnyAsync(x => x.Id == input.ManagerId.Value);
                if (!managerExist)
                {
                    throw new UserFriendlyException("Nhân sự được bổ nhiệm làm Trưởng phòng không tồn tại!");
                }
            }
        }
    }
}