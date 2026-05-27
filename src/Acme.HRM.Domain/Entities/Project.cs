using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.HRM.Entities
{
    public class Project : FullAuditedEntity<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // 2. Bảng trung gian Nhiều - Nhiều (Employee <-> Project)
    public class EmployeeProject : Entity
    {
        public long EmployeeId { get; set; }
        public long ProjectId { get; set; }

        // Cấu hình Khóa chính phức hợp cho ABP
        public override object[] GetKeys()
        {
            return new object[] { EmployeeId, ProjectId };
        }

        // Navigation properties
        public Employee Employee { get; set; }
        public Project Project { get; set; }
    }
}
