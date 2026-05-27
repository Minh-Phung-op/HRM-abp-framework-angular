using Acme.HRM.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.HRM.Entities
{
    public class LeaveRequestApprovalLog : CreationAuditedEntity<long>
    {
        public long LeaveRequestId { get; set; }

        // Ai là người xử lý (Lấy từ CurrentUser.Id)
        public Guid UserId { get; set; }

        // Giai đoạn duyệt: ví dụ "TeamLead" hoặc "HR"
        public ApprovalStep ActionStep { get; set; }

        // Hành động: ví dụ "Approved" hoặc "Rejected"
        public ApprovalAction Action { get; set; }

        // Lời phê / Lý do từ chối (Gộp chung vào đây cực kỳ sạch sẽ)
        public string Comment { get; set; }

        // Navigation
        public LeaveRequest LeaveRequest { get; set; }
        public Volo.Abp.Identity.IdentityUser User { get; set; } // Kết nối trực tiếp sang User hệ thống
    }
}
