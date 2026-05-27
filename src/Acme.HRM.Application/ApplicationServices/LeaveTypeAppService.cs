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
    [Authorize(HRMPermissions.LeaveTypes.Default)]
    public class LeaveTypeAppService : CrudAppService<
        LeaveType,
        LeaveTypeDto,
        long,
        GetAllLeaveTypesInput,
        CreateUpdateLeaveTypeDto>,
        ILeaveTypeAppService // Thực thi Interface chính thức
    {
        private readonly IRepository<LeaveBalance, long> _leaveBalanceRepository;
        private readonly IRepository<LeaveRequest, long> _leaveRequestRepository;

        public LeaveTypeAppService(
            IRepository<LeaveType, long> repository,
            IRepository<LeaveBalance, long> leaveBalanceRepository,
            IRepository<LeaveRequest, long> leaveRequestRepository)
            : base(repository)
        {
            _leaveBalanceRepository = leaveBalanceRepository;
            _leaveRequestRepository = leaveRequestRepository;

            // Tích hợp hệ thống phân quyền ABP Policies
            GetPolicyName = HRMPermissions.LeaveTypes.Default;
            GetListPolicyName = HRMPermissions.LeaveTypes.Default;
            CreatePolicyName = HRMPermissions.LeaveTypes.Manage;
            UpdatePolicyName = HRMPermissions.LeaveTypes.Manage;
            DeletePolicyName = HRMPermissions.LeaveTypes.Manage;
        }

        // ── 🔥 BỔ SUNG OVERRIDE: CREATE (Chặn trùng mã Code) ─────────
        public override async Task<LeaveTypeDto> CreateAsync(CreateUpdateLeaveTypeDto input)
        {
            // Viết hoa mã code để chuẩn hóa dữ liệu (Ví dụ: al -> AL)
            var formattedCode = input.Code.Trim().ToUpper();

            var isCodeExist = await Repository.AnyAsync(x => x.Code == formattedCode);
            if (isCodeExist)
            {
                throw new UserFriendlyException($"Mã loại nghỉ phép '{formattedCode}' đã tồn tại trong hệ thống.");
            }

            // Nếu không cho phép chuyển phép, ép số ngày về 0 hoặc null
            if (!input.CarryOver)
            {
                input.MaxCarryDays = 0;
            }

            var entity = ObjectMapper.Map<CreateUpdateLeaveTypeDto, LeaveType>(input);
            entity.Code = formattedCode;

            await Repository.InsertAsync(entity, autoSave: true);
            return ObjectMapper.Map<LeaveType, LeaveTypeDto>(entity);
        }

        // ── 🔥 BỔ SUNG OVERRIDE: UPDATE (Chặn sửa trùng Code với thằng khác)
        public override async Task<LeaveTypeDto> UpdateAsync(long id, CreateUpdateLeaveTypeDto input)
        {
            var entity = await Repository.GetAsync(id);
            var formattedCode = input.Code.Trim().ToUpper();

            // Nếu người dùng thay đổi mã Code, phải check xem có trùng với bản ghi KHÁC không
            if (entity.Code != formattedCode)
            {
                var isCodeExist = await Repository.AnyAsync(x => x.Code == formattedCode && x.Id != id);
                if (isCodeExist)
                {
                    throw new UserFriendlyException($"Không thể cập nhật! Mã loại nghỉ phép '{formattedCode}' đã được sử dụng bởi một loại nghỉ khác.");
                }
            }

            ObjectMapper.Map(input, entity);
            entity.Code = formattedCode;

            await Repository.UpdateAsync(entity, autoSave: true);
            return ObjectMapper.Map<LeaveType, LeaveTypeDto>(entity);
        }

        // ── GET LIST (Giữ nguyên cấu trúc tối ưu của bạn) ───────────
        public override async Task<PagedResultDto<LeaveTypeDto>> GetListAsync(GetAllLeaveTypesInput input)
        {
            var query = await Repository.GetQueryableAsync();

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword))
                .WhereIf(input.Paid.HasValue, x => x.Paid == input.Paid)
                .WhereIf(input.CarryOver.HasValue, x => x.CarryOver == input.CarryOver);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var items = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(LeaveType.Name) : input.Sorting)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );

            return new PagedResultDto<LeaveTypeDto>(
                totalCount,
                ObjectMapper.Map<List<LeaveType>, List<LeaveTypeDto>>(items)
            );
        }

        // ── 🔥 SỬA ĐỔI HOÀN THIỆN: DELETE (Chốt chặn toàn vẹn dữ liệu) ──
        public override async Task DeleteAsync(long id)
        {
            var entity = await Repository.FindAsync(id);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(LeaveType), id);
            }

            // 1. Kiểm tra xem loại nghỉ này đã được cấu hình số dư quỹ phép (LeaveBalance) cho ai chưa
            var hasBalance = await _leaveBalanceRepository.AnyAsync(x => x.LeaveTypeId == id);
            if (hasBalance)
            {
                throw new UserFriendlyException(
                    $"Không thể xóa loại nghỉ '{entity.Name}'. Dữ liệu về Quỹ số dư phép của nhân viên đang liên kết với loại nghỉ này."
                );
            }

            // 2. Kiểm tra xem có Đơn xin nghỉ phép (LeaveRequest) nào đang dùng loại nghỉ này không
            var hasRequest = await _leaveRequestRepository.AnyAsync(x => x.LeaveTypeId == id);
            if (hasRequest)
            {
                throw new UserFriendlyException(
                    $"Không thể xóa loại nghỉ '{entity.Name}'. Đang có đơn xin nghỉ phép trong hệ thống đăng ký theo loại nghỉ này."
                );
            }

            // An toàn tuyệt đối -> Tiến hành xóa mềm (Soft Delete của ABP tự xử lý nếu cấu hình)
            await Repository.DeleteAsync(entity, autoSave: true);
        }
    }
}