using Acme.HRM.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.HRM.Entities
{
    public class LeaveType : FullAuditedEntity<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int DefaultDaysPerYear { get; set; }
        public bool CarryOver { get; set; }
        public int MaxCarryDays { get; set; }
        public bool Paid { get; set; }
    }

    public class LeaveBalance : FullAuditedEntity<long>
    {
        public long EmployeeId { get; set; }
        public long LeaveTypeId { get; set; }
        public int Year { get; set; }
        [NotMapped] // 🔥 Thêm dòng này để EF Core bỏ qua nó
        public decimal TotalDays => AllocatedDays + CarriedOverDays;
        public decimal UsedDays { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal CarriedOverDays { get; set; }
        public decimal PendingDays { get; set; }

        [NotMapped] // 🔥 Thêm dòng này để EF Core bỏ qua nó
        public decimal RemainingDays => TotalDays - UsedDays - PendingDays;

        // Navigation
        public Employee Employee { get; set; }
        public LeaveType LeaveType { get; set; }
    }

    public class LeaveRequest : FullAuditedEntity<long>
    {
        public long EmployeeId { get; set; }
        public long LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; }
        public LeaveRequestStatus Status { get; set; }
        //public long? TlApprovedBy { get; set; }
        //public DateTime? TlApprovedAt { get; set; }
        //public long? HrApprovedBy { get; set; }
        //public DateTime? HrApprovedAt { get; set; }
        //public string RejectedReason { get; set; }

        // Navigation
        public Employee Employee { get; set; }
        public LeaveType LeaveType { get; set; }
        //public Employee Approver { get; set; }

        // Danh sách lịch sử duyệt của đơn này (1 đơn có nhiều log duyệt)
        public ICollection<LeaveRequestApprovalLog> ApprovalLogs { get; set; }
    }
}
