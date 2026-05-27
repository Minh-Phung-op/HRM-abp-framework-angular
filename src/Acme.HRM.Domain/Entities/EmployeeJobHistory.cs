using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.HRM.Entities
{
    public class EmployeeJobHistory : FullAuditedEntity<long>
    {
        public long EmployeeId { get; set; }
        public long? OldDepartmentId { get; set; }
        public long NewDepartmentId { get; set; }
        public long? OldPositionId { get; set; }
        public long NewPositionId { get; set; }
        public decimal OldBaseSalary { get; set; }
        public decimal NewBaseSalary { get; set; }
        public DateOnly ApplyDate { get; set; }
        public string Note { get; set; }

        // Navigation
        public Employee Employee { get; set; }
    }
}
