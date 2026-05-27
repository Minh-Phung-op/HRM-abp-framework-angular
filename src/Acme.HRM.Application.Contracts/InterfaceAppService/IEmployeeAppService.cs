using Acme.HRM.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Acme.HRM.InterfaceAppService
{
    public interface IEmployeeAppService :
    ICrudAppService<
        EmployeeDto,
        long,
        GetAllEmployeesInput,
        CreateUpdateEmployeeDto,
        CreateUpdateEmployeeDto>
    {
        // Thêm phương thức cho Use Case "Offboard nhân viên" của HR Manager
        Task OffboardAsync(long id, DateOnly offboardDate);

        // Thêm phương thức cho Use Case "Xem & cập nhật hồ sơ" của chính Nhân viên
        Task<EmployeeDto> GetProfileAsync();
        Task<EmployeeDto> UpdateProfileAsync(CreateUpdateEmployeeDto input);
    }
}
