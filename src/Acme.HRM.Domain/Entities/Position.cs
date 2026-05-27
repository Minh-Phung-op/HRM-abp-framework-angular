using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.HRM.Entities
{
    public class Position : FullAuditedEntity<long>
    {
        public string Title { get; set; }
        public string Level { get; set; }
        public long DepartmentId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Department Department { get; set; }
    }
}
