using Acme.HRM.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Acme.HRM.Dtos
{
    public class PayrollItemDto : FullAuditedEntityDto<long>
    {
        public long PayrollId { get; set; }
        public PayrollItemType Type { get; set; } // Khuyên dùng sang Enum
        public string Label { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }

    public class CreateUpdatePayrollItemDto
    {
        [Required]
        public PayrollItemType Type { get; set; } // Khuyên dùng sang Enum

        [Required, MaxLength(256)]
        public string Label { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }
    }

    // ─────────────────────────────────────────────
    // PAYROLL
    // ─────────────────────────────────────────────

    public class PayrollDto : FullAuditedEntityDto<long>
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DepartmentName { get; set; }
        public string PositionTitle { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal BhxhEmployee { get; set; }
        public decimal BhytEmployee { get; set; }
        public decimal BhtnEmployee { get; set; }
        public decimal Pit { get; set; }
        public PayrollStatus Status { get; set; } // Đổi sang Enum
        public DateTime? LockedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<PayrollItemDto> Items { get; set; } = new();
    }

    public class CreatePayrollDto
    {
        [Required]
        public long EmployeeId { get; set; }

        [Required, Range(2000, 2100)]
        public int Year { get; set; }

        [Required, Range(1, 12)]
        public int Month { get; set; }

        public decimal BaseSalary { get; set; }
        public List<CreateUpdatePayrollItemDto> Items { get; set; } = new();
    }

    public class UpdatePayrollDto
    {
        public decimal BaseSalary { get; set; }
        public List<CreateUpdatePayrollItemDto> Items { get; set; } = new();
    }

    public class GeneratePayrollInput
    {
        [Required, Range(2000, 2100)]
        public int Year { get; set; }

        [Required, Range(1, 12)]
        public int Month { get; set; }

        public long? DepartmentId { get; set; }
    }

    public class GetAllPayrollsInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
        public long? EmployeeId { get; set; }
        public long? DepartmentId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public PayrollStatus? Status { get; set; } // Đổi sang Enum?
        public decimal? NetSalaryFrom { get; set; }
        public decimal? NetSalaryTo { get; set; }
    }
}
