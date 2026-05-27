using System;
using System.Collections.Generic;
using System.Text;

namespace Acme.HRM.Enums
{
    public enum PayrollStatus
    {
        Draft = 1,
        Processing = 2,
        Calculated = 3,
        Approved = 4,
        Paid = 5
    }

    public enum PayrollItemType
    {
        Allowance = 1,
        Bonus = 2, 
        Deduction = 3,
        Advance = 4
    }
}
