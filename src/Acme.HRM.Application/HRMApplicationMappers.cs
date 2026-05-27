using Acme.HRM.Dtos;
using Acme.HRM.Entities;
using Acme.HRM.Enums;
using Acme.HRM.Permissions;
using AutoMapper;
using System.Linq;
using Volo.Abp.AutoMapper;
using Volo.Abp.Users;

namespace Acme.HRM;
public class HrmApplicationAutoMapperProfile : Profile
{
    public HrmApplicationAutoMapperProfile()
    {
        ConfigureOrganization();
        ConfigureAttendance();
        ConfigureLeave();
        ConfigurePayroll();
    }

    // ─────────────────────────────────────────────────────────────
    // ORGANIZATION (TỔ CHỨC & NHÂN VIÊN)
    // ─────────────────────────────────────────────────────────────
    private void ConfigureOrganization()
    {
        // Department
        CreateMap<Department, DepartmentDto>()
            .ForMember(d => d.ParentName,
                o => o.MapFrom(s => s.Parent != null ? s.Parent.Name : null))
            .ForMember(d => d.ManagerName,
                o => o.MapFrom(s => s.Manager != null ? s.Manager.FullName : null));

        CreateMap<CreateUpdateDepartmentDto, Department>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(d => d.Parent, o => o.Ignore())
            .ForMember(d => d.Manager, o => o.Ignore());

        // Position
        CreateMap<Position, PositionDto>()
            .ForMember(d => d.DepartmentName,
                o => o.MapFrom(s => s.Department != null ? s.Department.Name : null));

        CreateMap<CreateUpdatePositionDto, Position>()
            .IgnoreFullAuditedObjectProperties()
             .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(d => d.Department, o => o.Ignore());

        // Employee
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.DepartmentName,
                o => o.MapFrom(s => s.Department != null ? s.Department.Name : null))
            .ForMember(d => d.PositionTitle,
                o => o.MapFrom(s => s.Position != null ? s.Position.Title : null))
            .ForMember(d => d.ManagerName,
                o => o.MapFrom(s => s.Manager != null ? s.Manager.FullName : null))
            .ForMember(d => d.ContractType,
                o => o.MapFrom(s =>
                    s.Contracts
                        .Where(c => c.Status == ContractStatus.Active)
                        .OrderByDescending(c => c.StartDate)
                        .Select(c => (ContractType?)c.ContractType)
                        .FirstOrDefault()
                ))
            .ForMember(d => d.Roles, o => o.Ignore())
            .ForMember(d => d.ContractEndDate,
                o => o.MapFrom(s =>
                    s.Contracts
                        .Where(c => c.Status == ContractStatus.Active)
                        .OrderByDescending(c => c.StartDate)
                        .Select(c => c.EndDate)
                        .FirstOrDefault()
                ))
            // 🔥 THÊM MỚI: Tự động map danh sách hợp đồng đi kèm của nhân viên
            .ForMember(d => d.Contracts, o => o.MapFrom(s => s.Contracts));

        // CreateUpdateEmployeeDto -> Employee (Các trường Enum & DateOnly tự động khớp cấu trúc)
        CreateMap<CreateUpdateEmployeeDto, Employee>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(d => d.Department, o => o.Ignore())
            .ForMember(d => d.Position, o => o.Ignore())
            .ForMember(d => d.Manager, o => o.Ignore())
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.UserId, o => o.Ignore())
            .ForMember(x => x.User, o => o.Ignore())
            // 🔥 THÊM MỚI: Khi tạo mới nhân viên thông thường danh sách hợp đồng sẽ được xử lý riêng hoặc tạo sau
            .ForMember(d => d.Contracts, o => o.Ignore());

        // ── 🔥 THÊM MỚI HOÀN TOÀN: CONTRACT MAPPING ──
        CreateMap<Contract, ContractDto>()
            .ForMember(d => d.EmployeeName,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
            .ForMember(d => d.EmployeeCode,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.EmployeeCode : null));

        CreateMap<CreateContractDto, Contract>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Employee, o => o.Ignore());

        CreateMap<UpdateContractDto, Contract>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.EmployeeId, o => o.Ignore())
            .ForMember(d => d.Employee, o => o.Ignore());
    }

    // ─────────────────────────────────────────────────────────────
    // ATTENDANCE (CA LÀM VIỆC & CHẤM CÔNG)
    // ─────────────────────────────────────────────────────────────
    private void ConfigureAttendance()
    {
        // WorkSchedule
        CreateMap<WorkSchedule, WorkScheduleDto>();
        CreateMap<CreateUpdateWorkScheduleDto, WorkSchedule>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.IsDefault, o => o.Ignore());

        // Attendance -> Cập nhật thêm các trường liên quan đến giải trình và đi muộn về sớm
        CreateMap<Attendance, AttendanceDto>()
            .ForMember(d => d.EmployeeName,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
            .ForMember(d => d.EmployeeCode,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.EmployeeCode : null))
            .ForMember(d => d.ScheduleName,
                o => o.MapFrom(s => s.Schedule != null ? s.Schedule.Name : null));

        CreateMap<CreateAttendanceDto, Attendance>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(d => d.Employee, o => o.Ignore())
            .ForMember(d => d.Schedule, o => o.Ignore())
            .ForMember(d => d.EarlyLeaveMinutes, o => o.Ignore()) // Tính toán tự động ở AppService
            .ForMember(d => d.LateMinutes, o => o.Ignore())       // Tính toán tự động ở AppService
            .ForMember(d => d.ExplainNote, o => o.Ignore())
            .ForMember(d => d.ExplainStatus, o => o.Ignore())
            .ForMember(d => d.ExplainApprovedBy, o => o.Ignore())
            .ForMember(d => d.IsLocked, o => o.Ignore());

        // UpdateAttendanceDto 
        CreateMap<UpdateAttendanceDto, Attendance>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(d => d.EmployeeId, o => o.Ignore())
            .ForMember(d => d.WorkDate, o => o.Ignore())
            .ForMember(d => d.ScheduleId, o => o.Ignore())
            .ForMember(d => d.Source, o => o.Ignore())
            .ForMember(d => d.IsLocked, o => o.Ignore())
            .ForMember(d => d.ExplainStatus, o => o.Ignore())
            .ForMember(d => d.ExplainApprovedBy, o => o.Ignore())
            .ForMember(d => d.Employee, o => o.Ignore())
            .ForMember(d => d.Schedule, o => o.Ignore());
    }

    // ─────────────────────────────────────────────────────────────
    // LEAVE (QUẢN LÝ NGHỈ PHÉP) - ĐÃ CẬP NHẬT THEO MODEL MỚI
    // ─────────────────────────────────────────────────────────────
    private void ConfigureLeave()
    {
        // 1. LeaveType
        CreateMap<LeaveType, LeaveTypeDto>();
        CreateMap<CreateUpdateLeaveTypeDto, LeaveType>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Id, o => o.Ignore());

        // 2. LeaveBalance
        CreateMap<LeaveBalance, LeaveBalanceDto>()
            .ForMember(d => d.EmployeeName,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
            .ForMember(d => d.EmployeeCode,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.EmployeeCode : null))
            .ForMember(d => d.LeaveTypeName,
                o => o.MapFrom(s => s.LeaveType != null ? s.LeaveType.Name : null))
            .ForMember(d => d.RemainingDays,
                o => o.MapFrom(s => s.TotalDays - s.UsedDays - s.PendingDays));

        CreateMap<CreateUpdateLeaveBalanceDto, LeaveBalance>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.TotalDays, o => o.Ignore())
            .ForMember(d => d.UsedDays, o => o.Ignore())
            .ForMember(d => d.PendingDays, o => o.Ignore())
            .ForMember(d => d.Employee, o => o.Ignore())
            .ForMember(d => d.LeaveType, o => o.Ignore());

        // 3. LeaveRequest (Đã xóa bỏ Approver, TlApproved, HrApproved, RejectedReason cũ)
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(d => d.EmployeeName,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
            .ForMember(d => d.EmployeeCode,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.EmployeeCode : null))
            .ForMember(d => d.LeaveTypeName,
                o => o.MapFrom(s => s.LeaveType != null ? s.LeaveType.Name : null))
            // Tự động map danh sách ApprovalLogs từ Entity sang Dto để Frontend vẽ Timeline lịch sử duyệt đơn
            .ForMember(d => d.ApprovalLogs, o => o.MapFrom(s => s.ApprovalLogs));

        CreateMap<CreateLeaveRequestDto, LeaveRequest>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(d => d.EmployeeId, o => o.Ignore()) // Thường gán bằng EmployeeId của CurrentUser trong AppService
            .ForMember(d => d.TotalDays, o => o.Ignore())   // Tính toán logic (EndDate - StartDate trừ ngày nghỉ lễ/cuối tuần) tại AppService
            .ForMember(d => d.Status, o => o.Ignore())      // Mặc định set bằng LeaveRequestStatus.PendingTeamLead tại AppService
            .ForMember(d => d.Employee, o => o.Ignore())
            .ForMember(d => d.LeaveType, o => o.Ignore())
            .ForMember(d => d.ApprovalLogs, o => o.Ignore()); // Đơn mới tạo chưa thể có Log duyệt

        // 4. 🔥 MỚI BỔ SUNG: Mapping cho bảng Log lịch sử duyệt đơn nghỉ phép
        CreateMap<LeaveRequestApprovalLog, LeaveRequestApprovalLogDto>()
            .ForMember(d => d.UserName,
                o => o.MapFrom(s => s.User != null ? s.User.UserName : null)) // Lấy Username tài khoản hệ thống đã duyệt log này
            .ForMember(d => d.UserFullName,
                o => o.MapFrom(s => s.User != null ? s.User.Name + " " + s.User.Surname : null)); // Họ tên người duyệt (nếu có lưu)

        // Dùng khi người duyệt bấm nút Duyệt/Từ chối, đẩy dữ liệu xử lý lên
        CreateMap<CreateLeaveRequestApprovalLogDto, LeaveRequestApprovalLog>()
            .IgnoreCreationAuditedObjectProperties()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.LeaveRequestId, o => o.Ignore()) // Thường truyền qua URL hoặc gán thủ công trong AppService
            .ForMember(d => d.UserId, o => o.Ignore())         // Luôn lấy từ CurrentUser.Id trong AppService bảo mật hơn
            .ForMember(d => d.LeaveRequest, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
    }

    // ─────────────────────────────────────────────────────────────
    // PAYROLL (LƯƠNG THƯỞNG)
    // ─────────────────────────────────────────────────────────────
    private void ConfigurePayroll()
    {
        // PayrollItem
        CreateMap<PayrollItem, PayrollItemDto>();

        CreateMap<CreateUpdatePayrollItemDto, PayrollItem>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Payroll, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PayrollId, o => o.Ignore());

        // Payroll
        CreateMap<Payroll, PayrollDto>()
            .ForMember(d => d.EmployeeName,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
            .ForMember(d => d.EmployeeCode,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.EmployeeCode : null))
            .ForMember(d => d.DepartmentName,
                o => o.MapFrom(s => s.Employee != null && s.Employee.Department != null
                    ? s.Employee.Department.Name : null))
            .ForMember(d => d.PositionTitle,
                o => o.MapFrom(s => s.Employee != null && s.Employee.Position != null
                    ? s.Employee.Position.Title : null))
            // 🔥 THAY ĐỔI: Bỏ .Ignore(), cho phép AutoMapper tự động map list con từ Entity sang DTO
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        CreateMap<CreatePayrollDto, Payroll>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(d => d.GrossSalary, o => o.Ignore())
            .ForMember(d => d.NetSalary, o => o.Ignore())
            .ForMember(d => d.TotalDeduction, o => o.Ignore())
            .ForMember(d => d.BhxhEmployee, o => o.Ignore())
            .ForMember(d => d.BhytEmployee, o => o.Ignore())
            .ForMember(d => d.BhtnEmployee, o => o.Ignore())
            .ForMember(d => d.Pit, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.LockedAt, o => o.Ignore())
            .ForMember(d => d.PaidAt, o => o.Ignore())
            .ForMember(d => d.Employee, o => o.Ignore())
            // 🔥 THAY ĐỔI: Cho phép map list Items truyền lên từ Create DTO vào thẳng Entity
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        CreateMap<UpdatePayrollDto, Payroll>()
            .IgnoreFullAuditedObjectProperties()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(d => d.EmployeeId, o => o.Ignore())
            .ForMember(d => d.Year, o => o.Ignore())
            .ForMember(d => d.Month, o => o.Ignore())
            .ForMember(d => d.GrossSalary, o => o.Ignore())
            .ForMember(d => d.NetSalary, o => o.Ignore())
            .ForMember(d => d.TotalDeduction, o => o.Ignore())
            .ForMember(d => d.BhxhEmployee, o => o.Ignore())
            .ForMember(d => d.BhytEmployee, o => o.Ignore())
            .ForMember(d => d.BhtnEmployee, o => o.Ignore())
            .ForMember(d => d.Pit, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.LockedAt, o => o.Ignore())
            .ForMember(d => d.PaidAt, o => o.Ignore())
            .ForMember(d => d.Employee, o => o.Ignore())
            // 🔥 THAY ĐỔI: Cho phép cập nhật lại danh sách Item khi HR chỉnh sửa bảng lương
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
    }
}