using Acme.HRM.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Acme.HRM.Entities
{
    public class Employee : FullAuditedEntity<long>
    {
        public Guid? UserId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Gender Gender { get; set; } 
        public string NationalId { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
        public long DepartmentId { get; set; }
        public long PositionId { get; set; }
        public long? ManagerId { get; set; }
        public DateOnly StartDate { get; set; }
        public EmployeeStatus Status { get; set; }

        // Navigation
        public Department Department { get; set; }
        public Position Position { get; set; }
        public Employee Manager { get; set; }
        public IdentityUser User { get; set; }
        // 🔥 Thêm liên kết danh sách hợp đồng
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
