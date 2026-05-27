using Acme.HRM.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Acme.HRM.InterfaceAppService
{
    public interface IPayrollAppService :
    ICrudAppService<
        PayrollDto,
        long,
        GetAllPayrollsInput,
        CreatePayrollDto,
        UpdatePayrollDto>
    {
        // Use Case: Chạy tính lương tự động cho toàn bộ nhân viên/phòng ban
        Task GeneratePayrollAsync(GeneratePayrollInput input);

        // Use Case: Đóng kỳ lương (Khóa không cho sửa nữa)
        Task ClosePayrollPeriodAsync(int year, int month);

        // Use Case: Gửi payslip hàng loạt qua Email hệ thống
        Task SendBulkPayslipAsync(int year, int month);
    }
}
