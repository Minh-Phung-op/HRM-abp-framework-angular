using Acme.HRM.Enums;
using System;
using System.Collections.Generic; // Thêm using này để dùng ICollection
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Acme.HRM.Dtos
{
    // =========================================================================
    // ── 1. LEAVE TYPE DTOS (LOẠI NGHỈ PHÉP)
    // =========================================================================

    public class LeaveTypeDto : FullAuditedEntityDto<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int DefaultDaysPerYear { get; set; }
        public bool CarryOver { get; set; }
        public int MaxCarryDays { get; set; }
        public bool Paid { get; set; }
    }

    public class CreateUpdateLeaveTypeDto
    {
        [Required(ErrorMessage = "Tên loại nghỉ không được để trống")]
        [MaxLength(256)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mã loại nghỉ không được để trống")]
        [MaxLength(50)]
        public string Code { get; set; }

        [Range(0, 365, ErrorMessage = "Số ngày mặc định phải từ 0 đến 365")]
        public int DefaultDaysPerYear { get; set; }

        public bool CarryOver { get; set; }

        [Range(0, 365, ErrorMessage = "Số ngày chuyển dư tối đa phải từ 0 đến 365")]
        public int? MaxCarryDays { get; set; }

        public bool Paid { get; set; } = true;
    }

    public class GetAllLeaveTypesInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
        public bool? Paid { get; set; }
        public bool? CarryOver { get; set; }
    }

    // =========================================================================
    // ── 2. LEAVE BALANCE DTOS (QUỸ/SỐ DƯ PHÉP NĂM)
    // =========================================================================

    public class LeaveBalanceDto : FullAuditedEntityDto<long>
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public long LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public int Year { get; set; }

        public decimal AllocatedDays { get; set; }    // Phép được cấp trong năm nay
        public decimal CarriedOverDays { get; set; }   // Phép năm ngoái chuyển sang
        public decimal TotalDays => AllocatedDays + CarriedOverDays; // Tổng quỹ phép (Read-only)

        public decimal UsedDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal RemainingDays => TotalDays - UsedDays - PendingDays;
    }

    public class CreateUpdateLeaveBalanceDto
    {
        [Required]
        public long EmployeeId { get; set; }

        [Required]
        public long LeaveTypeId { get; set; }

        [Required, Range(2000, 2100)]
        public int Year { get; set; }

        [Range(0, 365)]
        public decimal AllocatedDays { get; set; }

        [Range(0, 365)]
        public decimal CarriedOverDays { get; set; }
    }

    public class BulkInitializeLeaveBalanceDto
    {
        [Required]
        public long LeaveTypeId { get; set; }

        [Required, Range(2000, 2100)]
        public int Year { get; set; }

        [Range(0, 365)]
        public int? DefaultDays { get; set; }
    }

    public class AdjustLeaveBalanceDto
    {
        [Required]
        public decimal AdjustmentDays { get; set; }
    }

    public class GetAllLeaveBalancesInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
        public long? EmployeeId { get; set; }
        public long? DepartmentId { get; set; }
        public long? LeaveTypeId { get; set; }
        public int? Year { get; set; }
    }

    // =========================================================================
    // ── 3. LEAVE REQUEST DTOS (ĐƠN XIN NGHỈ PHÉP)
    // =========================================================================

    public class LeaveRequestDto : FullAuditedEntityDto<long>
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public long LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; }
        public LeaveRequestStatus Status { get; set; }

        // 🔥 CHỖ NÀY ĐÃ SỬA: Xóa bỏ Approver cũ, thay bằng List logs lịch sử để Frontend vẽ dòng thời gian Timeline
        public ICollection<LeaveRequestApprovalLogDto> ApprovalLogs { get; set; }
    }

    public class CreateLeaveRequestDto
    {
        [Required(ErrorMessage = "Vui lòng chọn loại nghỉ phép")]
        public long LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime EndDate { get; set; }

        [MaxLength(1000, ErrorMessage = "Lý do không được vượt quá 1000 ký tự")]
        public string Reason { get; set; }
    }

    public class UpdateLeaveRequestDto
    {
        [Required]
        public long LeaveTypeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(1000)]
        public string Reason { get; set; }
    }

    public class GetAllLeaveRequestsInput : PagedAndSortedResultRequestDto
    {
        public string? Keyword { get; set; }
        public long? EmployeeId { get; set; }
        public long? DepartmentId { get; set; }
        public long? LeaveTypeId { get; set; }
        public LeaveRequestStatus? Status { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
    }

    // =========================================================================
    // ── 4. 🔥 TẠO MỚI hoàn toàn: LEAVE REQUEST APPROVAL LOG DTOS
    // =========================================================================

    public class LeaveRequestApprovalLogDto : CreationAuditedEntityDto<long>
    {
        public long LeaveRequestId { get; set; }

        // ID tài khoản của người duyệt (Lấy từ hệ thống)
        public Guid UserId { get; set; }

        // Tên tài khoản hệ thống (ví dụ: admin, khanhnv)
        public string UserName { get; set; }

        // Họ tên đầy đủ của người duyệt để hiển thị lên UI
        public string UserFullName { get; set; }

        // Giai đoạn duyệt: TeamLead hoặc HR
        public ApprovalStep ActionStep { get; set; }

        // Hành động: Approve, Reject, hoặc Cancel
        public ApprovalAction Action { get; set; }

        // Ý kiến phê duyệt / Lý do từ chối đơn
        public string Comment { get; set; }
    }

    public class CreateLeaveRequestApprovalLogDto
    {
        // Giai đoạn xử lý: TeamLead hay HR phê duyệt bước này?
        [Required(ErrorMessage = "Giai đoạn duyệt bắt buộc phải xác định")]
        public ApprovalStep ActionStep { get; set; }

        // Hành động: Chọn Approve (Duyệt) hoặc Reject (Từ chối)
        [Required(ErrorMessage = "Hành động duyệt không được để trống")]
        public ApprovalAction Action { get; set; }

        // Lời phê hoặc Lý do từ chối (Gợi ý: Client nên check bắt buộc điền nếu Action == Reject)
        [MaxLength(500, ErrorMessage = "Lời phê không được vượt quá 500 ký tự")]
        public string Comment { get; set; }
    }
}