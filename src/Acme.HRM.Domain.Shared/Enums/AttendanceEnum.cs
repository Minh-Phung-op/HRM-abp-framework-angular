using System;
using System.Collections.Generic;
using System.Text;

namespace Acme.HRM.Enums
{
    [Flags]
    public enum WorkingDayFlags
    {
        None = 0,
        Monday = 1 << 0,
        Tuesday = 1 << 1,
        Wednesday = 1 << 2,
        Thursday = 1 << 3,
        Friday = 1 << 4,
        Saturday = 1 << 5,
        Sunday = 1 << 6,
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday
    }
    public enum AttendanceStatus
    {
        Present = 1,
        Late = 2,
        Early = 3,
        Absent = 4,
        HalfDay = 5,
        OnLeave = 6
    }

    public enum AttendanceSource
    {
        Manual = 1,
        Device = 2,
        Mobile = 3
    }
    public enum AttendanceExplainStatus
    {
        Pending = 1,
        Approved = 2, 
        Rejected = 3
    }
}