using Acme.HRM.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Acme.HRM.InterfaceAppService
{
    public interface IAttendanceAppService :
    ICrudAppService<
        AttendanceDto,
        long,
        GetAllAttendancesInput,
        CreateAttendanceDto,
        UpdateAttendanceDto>
    {
        // Use Case: Check-in / check-out (Nhân viên tự bấm trên mobile/web)
        Task<AttendanceDto> CheckInAsync();
        Task<AttendanceDto> CheckOutAsync();

        // Use Case: Giải trình bất thường (Nhân viên gửi yêu cầu)
        Task RequestExplainAsync(long id, string explainNote);

        // Use Case: Duyệt giải trình (HR Manager duyệt)
        Task ApproveExplainAsync(long id, string note); // Dùng chung Dto Note nếu cần

        // Use Case: Khóa bảng công (HR Manager chốt dữ liệu tháng để tính lương)
        Task LockAttendanceAsync(int year, int month);
        Task UnlockAttendanceAsync(int year, int month);
    }
}
