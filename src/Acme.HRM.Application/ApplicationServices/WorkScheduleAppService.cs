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
    [Authorize(HRMPermissions.WorkSchedules.Default)]
    public class WorkScheduleAppService : CrudAppService<
        WorkSchedule,
        WorkScheduleDto,
        long,
        GetAllWorkSchedulesInput,
        CreateUpdateWorkScheduleDto>,
        IWorkScheduleAppService
    {
        private readonly IRepository<Employee, long> _employeeRepository;
        // Giả định bạn có bảng cấu hình ca chi tiết liên kết (nếu có) hoặc bảng phân lịch
        // private readonly IRepository<EmployeeScheduleAssignment, long> _assignmentRepository; 

        public WorkScheduleAppService(
            IRepository<WorkSchedule, long> repository,
            IRepository<Employee, long> employeeRepository)
            : base(repository)
        {
            _employeeRepository = employeeRepository;

            // 🔥 Thiết lập phân quyền tự động cho toàn bộ các trục API
            GetPolicyName = HRMPermissions.WorkSchedules.Default;
            GetListPolicyName = HRMPermissions.WorkSchedules.Default;
            CreatePolicyName = HRMPermissions.WorkSchedules.Manage;
            UpdatePolicyName = HRMPermissions.WorkSchedules.Manage;
            DeletePolicyName = HRMPermissions.WorkSchedules.Manage;
        }

        // ── 🔥 BỔ SUNG OVERRIDE: GET SINGLE (Nạp kèm cấu hình chi tiết) ─────
        public override async Task<WorkScheduleDto> GetAsync(long id)
        {
            // Đảm bảo Include các bảng con (ví dụ: WorkScheduleDays hoặc Shifts) để Front-end có data render
            var query = await Repository.WithDetailsAsync(); // Thêm biểu thức lambda nếu cần nạp quan hệ sâu hơn

            var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(WorkSchedule), id);
            }

            return ObjectMapper.Map<WorkSchedule, WorkScheduleDto>(entity);
        }

        // ── 🔥 BỔ SUNG OVERRIDE: CREATE (Chặn trùng tên & Check Logic thời gian) ──
        public override async Task<WorkScheduleDto> CreateAsync(CreateUpdateWorkScheduleDto input)
        {
            // 1. Chặn trùng tên lịch làm việc công ty
            var cleanName = input.Name.Trim();
            var isNameExist = await Repository.AnyAsync(x => x.Name.ToLower() == cleanName.ToLower());
            if (isNameExist)
            {
                throw new UserFriendlyException($"Lịch làm việc có tên '{cleanName}' đã tồn tại.");
            }

            // 2. Kiểm tra logic thời gian (Nếu DTO của bạn có chứa giờ CheckIn / CheckOut trực tiếp)
            ValidateScheduleTime(input);

            var entity = ObjectMapper.Map<CreateUpdateWorkScheduleDto, WorkSchedule>(input);
            entity.Name = cleanName;

            await Repository.InsertAsync(entity, autoSave: true);
            return await GetAsync(entity.Id);
        }

        // ── 🔥 BỔ SUNG OVERRIDE: UPDATE (Kiểm soát dữ liệu khi sửa) ─────────
        public override async Task<WorkScheduleDto> UpdateAsync(long id, CreateUpdateWorkScheduleDto input)
        {
            var entity = await Repository.GetAsync(id);
            var cleanName = input.Name.Trim();

            // 1. Nếu đổi tên, kiểm tra xem có trùng với lịch khác không
            if (entity.Name.ToLower() != cleanName.ToLower())
            {
                var isNameExist = await Repository.AnyAsync(x => x.Name.ToLower() == cleanName.ToLower() && x.Id != id);
                if (isNameExist)
                {
                    throw new UserFriendlyException($"Không thể cập nhật! Tên lịch làm việc '{cleanName}' đã được sử dụng.");
                }
            }

            // 2. Kiểm tra logic thời gian
            ValidateScheduleTime(input);

            ObjectMapper.Map(input, entity);
            entity.Name = cleanName;

            await Repository.UpdateAsync(entity, autoSave: true);
            return await GetAsync(id);
        }

        // ── GET LIST (Giữ nguyên cấu trúc của bạn nhưng tối ưu luồng) ───────
        public override async Task<PagedResultDto<WorkScheduleDto>> GetListAsync(GetAllWorkSchedulesInput input)
        {
            var query = await Repository.WithDetailsAsync();

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));

            var totalCount = await AsyncExecuter.CountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(WorkSchedule.Name) : input.Sorting)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<WorkScheduleDto>(
                totalCount,
                ObjectMapper.Map<List<WorkSchedule>, List<WorkScheduleDto>>(items)
            );
        }

        // ── 🔥 BỔ SUNG OVERRIDE: DELETE (Chốt chặn an toàn dữ liệu chấm công) ──
        public override async Task DeleteAsync(long id)
        {
            var entity = await Repository.FindAsync(id);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(WorkSchedule), id);
            }

            // 🛑 CHỐT CHẶN CHÍ MẠNG: Kiểm tra xem lịch này có đang được gán mặc định cho nhân viên nào không
            // Giả định Entity Employee của bạn có trường WorkScheduleId để gắn ca cố định
            //var isBeingUsed = await _employeeRepository.AnyAsync(x => x.WorkScheduleId == id);
            //if (isBeingUsed)
            //{
            //    throw new UserFriendlyException(
            //        $"Không thể xóa lịch '{entity.Name}'. Đang có nhân sự trong công ty áp dụng lịch làm việc này để chấm công. Hãy chuyển lịch của họ trước."
            //    );
            //}

            await Repository.DeleteAsync(entity, autoSave: true);
        }

        // ── PRIVATE HELPERS ──────────────────────────────────────────
        private void ValidateScheduleTime(CreateUpdateWorkScheduleDto input)
        {
            // Đoạn này tùy thuộc vào các thuộc tính thời gian trong Model của bạn. Ví dụ:
             if (input.CheckInTime >= input.CheckOutTime)
            {
                throw new UserFriendlyException("Thời gian bắt đầu ca (Check-in) không được lớn hơn hoặc bằng thời gian kết thúc ca (Check-out)");
            }
        }
    }
}