using Acme.HRM.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Acme.HRM.Dtos
{
    public class DepartmentDto : FullAuditedEntityDto<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public long? ParentId { get; set; }
        public string ParentName { get; set; }
        public long? ManagerId { get; set; }
        public string ManagerName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateUpdateDepartmentDto
    {
        [Required, MaxLength(256)]
        public string Name { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; }

        public long? ParentId { get; set; }
        public long? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class GetAllDepartmentsInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
        public long? ParentId { get; set; }
        public bool? IsActive { get; set; }
    }

    // ─────────────────────────────────────────────
    // POSITION
    // ─────────────────────────────────────────────

    public class PositionDto : FullAuditedEntityDto<long>
    {
        public string Title { get; set; }
        public string Level { get; set; }
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateUpdatePositionDto
    {
        [Required, MaxLength(256)]
        public string Title { get; set; }

        [MaxLength(100)]
        public string Level { get; set; }

        [Required]
        public long DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class GetAllPositionsInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
        public long? DepartmentId { get; set; }
        public bool? IsActive { get; set; }
    }

    // ─────────────────────────────────────────────
    // EMPLOYEE
    // ─────────────────────────────────────────────

    public class EmployeeDto : FullAuditedEntityDto<long>
    {
        public Guid? UserId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateOnly? DateOfBirth { get; set; } // Đổi sang DateOnly
        public Gender Gender { get; set; } // Đổi sang Enum
        public ICollection<string> Roles { get; set; }
        public string NationalId { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; }
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public long PositionId { get; set; }
        public string PositionTitle { get; set; }
        public long? ManagerId { get; set; }
        public string ManagerName { get; set; }
        public DateOnly StartDate { get; set; } // Đổi sang DateOnly
        public ContractType ContractType { get; set; } // Đổi sang Enum
        public DateOnly? ContractEndDate { get; set; } // Bổ sung trường thiếu
        public EmployeeStatus Status { get; set; } // Đổi sang Enum
        // Thêm vào trong class EmployeeDto để lấy danh sách lịch sử hợp đồng
        public ICollection<ContractDto> Contracts { get; set; }
    }

    public class CreateUpdateEmployeeDto
    {
        [Required, MaxLength(50)]
        public string EmployeeCode { get; set; }

        [Required, MaxLength(256)]
        public string FullName { get; set; }

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public DateOnly? DateOfBirth { get; set; } // Đổi sang DateOnly

        [Required]
        public Gender Gender { get; set; } // Đổi sang Enum

        [MaxLength(50)]
        public string NationalId { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        public string AvatarUrl { get; set; }

        [Required]
        public long DepartmentId { get; set; }

        [Required]
        public long PositionId { get; set; }

        public long? ManagerId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; } // Đổi sang DateOnly

        [Required]
        public ContractType ContractType { get; set; } // Đổi sang Enum

        public DateOnly? ContractEndDate { get; set; } // Bổ sung trường thiếu

        [Required]
        public EmployeeStatus Status { get; set; } // Đổi sang Enum
    }

    public class GetAllEmployeesInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
        public long? DepartmentId { get; set; }
        public long? PositionId { get; set; }
        public long? ManagerId { get; set; }
        public ContractType? ContractType { get; set; } // Enum?
        public EmployeeStatus? Status { get; set; } // Enum?
        public Gender? Gender { get; set; } // Enum?
        public DateOnly? StartDateFrom { get; set; }
        public DateOnly? StartDateTo { get; set; }
    }

    // =========================================================================
    // ── 4. 🔥 BỔ SUNG MỚI: CONTRACT DTOS (HỢP ĐỒNG LAO ĐỘNG)
    // =========================================================================

    public class ContractDto : FullAuditedEntityDto<long>
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }

        public string ContractNumber { get; set; }
        public ContractType ContractType { get; set; }

        public DateOnly SignDate { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal InsuranceSalary { get; set; }
        public ContractStatus Status { get; set; } // Ví dụ: Active, Expired, Terminated
    }

    public class CreateContractDto
    {
        [Required]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Số hợp đồng không được để trống")]
        [MaxLength(100)]
        public string ContractNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại hợp đồng")]
        public ContractType ContractType { get; set; }

        [Required]
        public DateOnly SignDate { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [Range(0, 999999999999)]
        public decimal BasicSalary { get; set; }

        [Range(0, 999999999999)]
        public decimal InsuranceSalary { get; set; }

        [Required]
        public ContractStatus Status { get; set; }
    }

    public class UpdateContractDto
    {
        [Required(ErrorMessage = "Số hợp đồng không được để trống")]
        [MaxLength(100)]
        public string ContractNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại hợp đồng")]
        public ContractType ContractType { get; set; }

        [Required]
        public DateOnly SignDate { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [Range(0, 999999999999)]
        public decimal BasicSalary { get; set; }

        [Range(0, 999999999999)]
        public decimal InsuranceSalary { get; set; }

        [Required]
        public ContractStatus Status { get; set; }
    }

    public class GetAllContractsInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
        public long? EmployeeId { get; set; }
        public ContractType? ContractType { get; set; }
        public ContractStatus? Status { get; set; }
        public DateOnly? StartDateFrom { get; set; }
        public DateOnly? StartDateTo { get; set; }
    }
}
