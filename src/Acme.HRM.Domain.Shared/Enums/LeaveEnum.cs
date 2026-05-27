using System;
using System.Collections.Generic;
using System.Text;

namespace Acme.HRM.Enums
{
    public enum LeaveRequestStatus
    {
        Pending = 1,
        Approved = 2, 
        Rejected = 3, 
        Cancelled = 4
    }

    // 2. Enum Giai đoạn phê duyệt
    public enum ApprovalStep
    {
        TeamLead = 1,
        HR = 2
    }

    // 3. Enum Hành động xử lý đơn
    public enum ApprovalAction
    {
        Approve = 1,
        Reject = 2,
        Cancel = 3
    }
}
