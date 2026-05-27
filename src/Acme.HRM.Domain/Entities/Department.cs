using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;


namespace Acme.HRM.Entities
{
    public class Department : FullAuditedEntity<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public long? ParentId { get; set; }
        public long? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Department Parent { get; set; }
        public Employee Manager { get; set; }
    }
}
