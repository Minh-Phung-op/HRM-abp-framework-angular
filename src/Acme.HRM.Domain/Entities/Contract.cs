using System;
using Volo.Abp.Domain.Entities.Auditing;
using Acme.HRM.Enums; // Giả định bạn lưu ContractType, ContractStatus ở đây

namespace Acme.HRM.Entities
{
    public class Contract : FullAuditedEntity<long>
    {
        public long EmployeeId { get; set; }
        public string ContractNumber { get; set; } // Số hợp đồng (Ví dụ: 01/2026/HĐLĐ)
        public ContractType ContractType { get; set; } // Thử việc, Xác định thời hạn, Không xác định thời hạn

        public DateOnly StartDate { get; set; } // Ngày hiệu lực
        public DateOnly? EndDate { get; set; } // Ngày hết hạn (Null nếu là không xác định thời hạn)
        public DateOnly SignDate { get; set; }

        // 🔥 Trọng tâm tính lương
        public decimal BasicSalary { get; set; } // Lương cơ bản dùng làm căn cứ đóng BHXH và tính lương
        public decimal InsuranceSalary { get; set; } // Mức lương đóng bảo hiểm (thường bằng hoặc thấp hơn Gross)

        public ContractStatus Status { get; set; } // Active | Expired | Terminated

        // Navigation
        public Employee Employee { get; set; }
    }
}