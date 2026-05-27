using Acme.HRM.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.HRM.Entities
{
    public class WorkSchedule : FullAuditedEntity<long>
    {
        public string Name { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan CheckOutTime { get; set; }
        public int LateThresholdMinutes { get; set; }
        public WorkingDayFlags WorkingDays { get; set; }
        public bool IsDefault { get; set; }
    }

    public class Attendance : FullAuditedEntity<long>
    {
        public long EmployeeId { get; set; }
        public DateOnly WorkDate { get; set; }
        public long ScheduleId { get; set; }
        public TimeOnly? CheckInAt { get; set; }
        public TimeOnly? CheckOutAt { get; set; }
        public AttendanceStatus Status { get; set; }
        public int? LateMinutes { get; set; }
        public int? EarlyLeaveMinutes { get; set; }
        public string? Note { get; set; }
        public string? ExplainNote { get; set; }
        public AttendanceExplainStatus? ExplainStatus { get; set; } 
        public long? ExplainApprovedBy { get; set; }
        public int OtMinutes { get; set; }
        public AttendanceSource Source { get; set; }
        public bool IsLocked { get; set; }

        // Navigation
        public Employee Employee { get; set; }
        public WorkSchedule Schedule { get; set; }
    }
}
