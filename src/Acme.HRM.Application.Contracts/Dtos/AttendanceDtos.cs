using Acme.HRM.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Acme.HRM.Dtos
{
    public class WorkScheduleDto : FullAuditedEntityDto<long>
    {
        public string Name { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan CheckOutTime { get; set; }
        public int LateThresholdMinutes { get; set; }
        public WorkingDayFlags WorkingDays { get; set; } // Đổi sang Enum Flags
    }

    public class CreateUpdateWorkScheduleDto
    {
        [Required, MaxLength(256)]
        public string Name { get; set; }

        [Required]
        public TimeSpan CheckInTime { get; set; }

        [Required]
        public TimeSpan CheckOutTime { get; set; }

        public int LateThresholdMinutes { get; set; } = 15;

        [Required]
        public WorkingDayFlags WorkingDays { get; set; } // Đổi sang Enum Flags
    }

    public class GetAllWorkSchedulesInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
    }

    // ─────────────────────────────────────────────
    // ATTENDANCE
    // ─────────────────────────────────────────────

    public class AttendanceDto : FullAuditedEntityDto<long>
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public DateOnly WorkDate { get; set; }
        public long ScheduleId { get; set; }
        public string ScheduleName { get; set; }
        public TimeOnly? CheckInAt { get; set; }
        public TimeOnly? CheckOutAt { get; set; }
        public AttendanceStatus Status { get; set; } // Đổi sang Enum
        public int? LateMinutes { get; set; } // Bổ sung trường thiếu
        public int? EarlyLeaveMinutes { get; set; } // Bổ sung trường thiếu
        public string? ExplainNote { get; set; } // Bổ sung trường thiếu
        public AttendanceExplainStatus? ExplainStatus { get; set; } // Bổ sung trường thiếu chuẩn Enum
        public long? ExplainApprovedBy { get; set; } // Bổ sung trường thiếu
        public int OtMinutes { get; set; }
        public string Note { get; set; }
        public AttendanceSource Source { get; set; } // Đổi sang Enum
        public bool IsLocked { get; set; }
    }

    public class CreateAttendanceDto
    {
        [Required]
        public long EmployeeId { get; set; }

        [Required]
        public DateOnly WorkDate { get; set; }

        [Required]
        public long ScheduleId { get; set; }

        public TimeOnly? CheckInAt { get; set; }
        public TimeOnly? CheckOutAt { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; } // Đổi sang Enum

        public int? LateMinutes { get; set; }
        public int? EarlyLeaveMinutes { get; set; }
        public int OtMinutes { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        [Required]
        public AttendanceSource Source { get; set; } = AttendanceSource.Manual; // Đổi sang Enum
    }

    public class UpdateAttendanceDto
    {
        public TimeOnly? CheckInAt { get; set; }
        public TimeOnly ? CheckOutAt { get; set; }
        public AttendanceStatus? Status { get; set; } // Đổi sang Enum?
        public int? LateMinutes { get; set; }
        public int? EarlyLeaveMinutes { get; set; }
        public int OtMinutes { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        // Dùng cho việc nhân viên làm giải trình giải trình
        [MaxLength(500)]
        public string ExplainNote { get; set; }
    }

    public class GetAllAttendancesInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
        public long? EmployeeId { get; set; }
        public long? DepartmentId { get; set; }
        public long? ScheduleId { get; set; }
        public AttendanceStatus? Status { get; set; } // Đổi sang Enum?
        public AttendanceSource? Source { get; set; } // Đổi sang Enum?
        public AttendanceExplainStatus? ExplainStatus { get; set; } // Bổ sung lọc theo trạng thái giải trình
        public bool? IsLocked { get; set; }
        public DateOnly? WorkDateFrom { get; set; }
        public DateOnly? WorkDateTo { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
