using Acme.HRM.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Acme.HRM
{
    public interface IDepartmentAppService
    : ICrudAppService<DepartmentDto, long, GetAllDepartmentsInput, CreateUpdateDepartmentDto>
    { }

    public interface IPositionAppService
        : ICrudAppService<PositionDto, long, GetAllPositionsInput, CreateUpdatePositionDto>
    { }

    public interface IEmployeeAppService
        : ICrudAppService<EmployeeDto, long, GetAllEmployeesInput, CreateUpdateEmployeeDto>
    { }

    public interface IWorkScheduleAppService
        : ICrudAppService<WorkScheduleDto, long, GetAllWorkSchedulesInput, CreateUpdateWorkScheduleDto>
    { }

    public interface IAttendanceAppService
        : ICrudAppService<AttendanceDto, long, GetAllAttendancesInput, CreateAttendanceDto, UpdateAttendanceDto>
    {
        Task<AttendanceDto> LockAsync(long id);
        Task<AttendanceDto> UnlockAsync(long id);
        Task BulkLockAsync(GetAllAttendancesInput filter);
    }

    public interface ILeaveTypeAppService
        : ICrudAppService<LeaveTypeDto, long, GetAllLeaveTypesInput, CreateUpdateLeaveTypeDto>
    { }

    public interface ILeaveBalanceAppService
        : ICrudAppService<LeaveBalanceDto, long, GetAllLeaveBalancesInput, CreateUpdateLeaveBalanceDto>
    { }

    public interface ILeaveRequestAppService
        : IReadOnlyAppService<LeaveRequestDto, LeaveRequestDto, long, GetAllLeaveRequestsInput>
    {
        Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestDto input);
        Task<LeaveRequestDto> ApproveAsync(long id, CreateLeaveRequestApprovalLogDto input);
        Task<LeaveRequestDto> RejectAsync(long id, CreateLeaveRequestApprovalLogDto input);
        Task<LeaveRequestDto> CancelAsync(long id);
    }

    public interface IPayrollAppService
        : ICrudAppService<PayrollDto, long, GetAllPayrollsInput, CreatePayrollDto, UpdatePayrollDto>
    {
        Task GenerateAsync(GeneratePayrollInput input);
        Task<PayrollDto> ApproveAsync(long id);
        Task<PayrollDto> MarkAsPaidAsync(long id);
        Task<PayrollDto> LockAsync(long id);
    }
}
