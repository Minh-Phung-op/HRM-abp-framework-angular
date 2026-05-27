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
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Acme.HRM.ApplicationServices
{
    [Authorize(HRMPermissions.Positions.Default)]
    public class PositionAppService : CrudAppService<
        Position,
        PositionDto,
        long,
        GetAllPositionsInput,
        CreateUpdatePositionDto>
    {
        private readonly IRepository<Employee, long> _employeeRepository;
        private readonly IRepository<Department, long> _departmentRepository;

        public PositionAppService(
            IRepository<Position, long> repository,
            IRepository<Employee, long> employeeRepository,
            IRepository<Department, long> departmentRepository)
            : base(repository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;

            // 🔥 Tích hợp hệ thống phân quyền tự động của ABP CrudAppService
            GetPolicyName = HRMPermissions.Positions.Default;
            GetListPolicyName = HRMPermissions.Positions.Default;
            CreatePolicyName = HRMPermissions.Positions.Manage;
            UpdatePolicyName = HRMPermissions.Positions.Manage;
            DeletePolicyName = HRMPermissions.Positions.Manage;
        }

        // ── 🔥 BỔ SUNG OVERRIDE: CREATE (Chặn trùng tên chức vụ trong cùng một phòng)
        public override async Task<PositionDto> CreateAsync(CreateUpdatePositionDto input)
        {
            // 1. Kiểm tra phòng ban chỉ định có tồn tại thật không
            await EnsureDepartmentExistsAsync(input.DepartmentId);

            // 2. Chặn trùng tên chức vụ (Title) trong cùng một phòng ban (Ví dụ: Một phòng IT không nên tạo 2 vị trí "Trưởng phòng" độc lập)
            var cleanTitle = input.Title.Trim();
            var isDuplicate = await Repository.AnyAsync(x => x.DepartmentId == input.DepartmentId && x.Title.ToLower() == cleanTitle.ToLower());
            if (isDuplicate)
            {
                throw new UserFriendlyException($"Chức vụ '{cleanTitle}' đã tồn tại trong phòng ban này rồi.");
            }

            var entity = ObjectMapper.Map<CreateUpdatePositionDto, Position>(input);
            entity.Title = cleanTitle;

            await Repository.InsertAsync(entity, autoSave: true);
            return await GetAsync(entity.Id); // Trả về Dto đầy đủ kèm theo thông tin Department được include
        }

        // ── 🔥 BỔ SUNG OVERRIDE: UPDATE (Chặn sửa trùng tên với chức vụ khác)
        public override async Task<PositionDto> UpdateAsync(long id, CreateUpdatePositionDto input)
        {
            var entity = await Repository.GetAsync(id);

            // 1. Kiểm tra phòng ban mới có hợp lệ không nếu HR có hành vi chuyển đổi phòng cho chức vụ
            if (entity.DepartmentId != input.DepartmentId)
            {
                await EnsureDepartmentExistsAsync(input.DepartmentId);
            }

            // 2. Kiểm tra trùng tên với các bản ghi khác cùng phòng
            var cleanTitle = input.Title.Trim();
            var isDuplicate = await Repository.AnyAsync(x =>
                x.DepartmentId == input.DepartmentId &&
                x.Title.ToLower() == cleanTitle.ToLower() &&
                x.Id != id);

            if (isDuplicate)
            {
                throw new UserFriendlyException($"Không thể cập nhật! Tên chức vụ '{cleanTitle}' đã được sử dụng cho một vị trí khác trong phòng ban này.");
            }

            ObjectMapper.Map(input, entity);
            entity.Title = cleanTitle;

            await Repository.UpdateAsync(entity, autoSave: true);
            return await GetAsync(id);
        }

        // ── GET SINGLE (Giữ nguyên logic Eager Loading tối ưu của bạn) ──
        public override async Task<PositionDto> GetAsync(long id)
        {
            var query = await Repository.WithDetailsAsync(x => x.Department);
            var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);

            if (entity == null)
                throw new EntityNotFoundException(typeof(Position), id);

            return ObjectMapper.Map<Position, PositionDto>(entity);
        }

        // ── GET LIST (Giữ nguyên logic của bạn) ─────────────────────────
        public override async Task<PagedResultDto<PositionDto>> GetListAsync(GetAllPositionsInput input)
        {
            var query = await Repository.WithDetailsAsync(x => x.Department);

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Title.Contains(input.Keyword) || x.Level.Contains(input.Keyword))
                .WhereIf(input.DepartmentId.HasValue, x => x.DepartmentId == input.DepartmentId)
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(Position.Title) : input.Sorting)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<PositionDto>(
                totalCount,
                ObjectMapper.Map<List<Position>, List<PositionDto>>(items)
            );
        }

        // ── 🔥 BỔ SUNG OVERRIDE: DELETE (Chốt chặn bảo vệ toàn vẹn dữ liệu) ──
        public override async Task DeleteAsync(long id)
        {
            var entity = await Repository.FindAsync(id);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(Position), id);
            }

            // 🛑 CHỐT CHẶN CHÍ MẠNG: Kiểm tra xem có Nhân viên (Employee) nào đang giữ chức vụ này không
            var hasEmployees = await _employeeRepository.AnyAsync(x => x.PositionId == id);
            if (hasEmployees)
            {
                throw new UserFriendlyException(
                    $"Không thể xóa chức vụ '{entity.Title}'. Đang có nhân viên trong công ty giữ chức vụ này. Vui lòng chuyển đổi chức vụ của nhân viên trước khi thực hiện xóa."
                );
            }

            // An toàn tuyệt đối -> Tiến hành xóa mềm (Soft Delete)
            await Repository.DeleteAsync(entity, autoSave: true);
        }

        // ── PRIVATE HELPERS ──────────────────────────────────────────
        private async Task EnsureDepartmentExistsAsync(long departmentId)
        {
            var departmentExists = await _departmentRepository.AnyAsync(x => x.Id == departmentId);
            if (!departmentExists)
            {
                throw new UserFriendlyException($"Phòng ban được chỉ định (Id: {departmentId}) không tồn tại trên hệ thống.");
            }
        }
    }
}