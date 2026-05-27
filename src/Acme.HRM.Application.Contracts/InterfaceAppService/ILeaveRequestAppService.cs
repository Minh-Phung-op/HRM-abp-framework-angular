using Acme.HRM.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Acme.HRM.InterfaceAppService
{
    public interface ILeaveRequestAppService :
    ICrudAppService<
        LeaveRequestDto,
        long,
        GetAllLeaveRequestsInput,
        CreateLeaveRequestDto,
        UpdateLeaveRequestDto>
    {
        // Use Case: Trưởng nhóm duyệt/từ chối phép của Team
        Task<LeaveRequestDto> ApproveAsync(long id, CreateLeaveRequestApprovalLogDto input);

        // Use Case: Từ chối đơn (Kèm lý do trong trường Comment của DTO)
        Task<LeaveRequestDto> RejectAsync(long id, CreateLeaveRequestApprovalLogDto input);

        // Use Case: Nhân viên tự hủy đơn đăng ký phép khi còn ở trạng thái Chờ duyệt
        Task CancelAsync(long id);
    }
}
