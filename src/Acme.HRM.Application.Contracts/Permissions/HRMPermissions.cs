namespace Acme.HRM.Permissions;

public static class HRMPermissions
{
    public const string GroupName = "HRM";

    // ─────────────────────────────────────────────
    // MODULE 1: QUẢN LÝ TỔ CHỨC & NHÂN VIÊN
    // ─────────────────────────────────────────────
    public static class Employees
    {
        public const string Default = GroupName + ".Employees";
        public const string Create = Default + ".Create";       // Thêm nhân viên mới
        public const string Update = Default + ".Update";       // Cập nhật hồ sơ
        public const string Delete = Default + ".Delete";       // Xóa dữ liệu (Admin)
        public const string Offboard = Default + ".Offboard";   // Offboard nhân viên (HR)
        public const string Export = Default + ".Export";
    }

    public static class Departments
    {
        public const string Default = GroupName + ".Departments";
        public const string Manage = Default + ".Manage";       // Thêm/Sửa/Xóa phòng ban
    }

    public static class Positions
    {
        public const string Default = GroupName + ".Positions";
        public const string Manage = Default + ".Manage";       // Quản lý chức vụ
    }

    public static class Projects
    {
        public const string Default = GroupName + ".Projects";
        public const string Manage = Default + ".Manage";       // Quản lý dự án
    }

    // ─────────────────────────────────────────────
    // MODULE 2: CA LÀM VIỆC & CHẤM CÔNG
    // ─────────────────────────────────────────────
    public static class WorkSchedules
    {
        public const string Default = GroupName + ".WorkSchedules";
        public const string Manage = Default + ".Manage";       // Cấu hình ca / ngày lễ
    }

    public static class Attendances
    {
        public const string Default = GroupName + ".Attendances";
        public const string CheckInOut = Default + ".CheckInOut"; // Nhân viên tự check-in/out
        public const string ViewTeam = Default + ".ViewTeam";     // TL xem công team
        public const string Update = Default + ".Update";         // HR sửa dữ liệu chấm công
        public const string Lock = Default + ".Lock";             // HR Khóa bảng công

        // Giải trình chấm công bất thường
        public const string RequestExplain = Default + ".RequestExplain";
        public const string ApproveExplain = Default + ".ApproveExplain";
    }

    // ─────────────────────────────────────────────
    // MODULE 3: QUẢN LÝ NGHỈ PHÉP
    // ─────────────────────────────────────────────
    public static class LeaveTypes
    {
        public const string Default = GroupName + ".LeaveTypes";
        public const string Manage = Default + ".Manage";       // Cấu hình loại phép
    }

    public static class LeaveBalances
    {
        public const string Default = GroupName + ".LeaveBalances";
        public const string Manage = Default + ".Manage";       // Điều chỉnh quỹ phép cty
    }

    public static class LeaveRequests
    {
        public const string Default = GroupName + ".LeaveRequests";
        public const string Create = Default + ".Create";       // Nhân viên tự đăng ký phép
        public const string Update = Default + ".Update";       // Nhân viên tự đăng ký phép
        public const string ApproveTeam = Default + ".ApproveTeam"; // Team Leader duyệt phép team
        public const string ApproveCompany = Default + ".ApproveCompany"; // HR duyệt phép toàn cty
    }

    // ─────────────────────────────────────────────
    // MODULE 4: LƯƠNG THƯỞNG
    // ─────────────────────────────────────────────
    public static class Payrolls
    {
        public const string Default = GroupName + ".Payrolls";
        public const string Management = Default + ".Management"; // Xem lương toàn bộ nhân viên
        public const string Configure = Default + ".Configure"; // Cấu hình công thức lương
        public const string Create = Default + ".Create";       // Chạy tính lương
        public const string Approve = Default + ".Approve";     // Đóng kỳ lương / Duyệt
        public const string MarkPaid = Default + ".MarkPaid";
        public const string SendPayslip = Default + ".SendPayslip"; // Gửi payslip hàng loạt
    }

    // ─────────────────────────────────────────────
    // MODULE PHỤ: HỆ THỐNG / AUDIT / BÁO CÁO (Dành cho Super Admin & HR)
    // ─────────────────────────────────────────────
    public static class SystemSettings
    {
        public const string Default = GroupName + ".SystemSettings";
        public const string ManageCompanyInfo = Default + ".ManageCompanyInfo"; // Cấu hình thông tin cty
        public const string DeviceIntegration = Default + ".DeviceIntegration"; // Tích hợp máy chấm công
        public const string BackupRestore = Default + ".BackupRestore";
    }

    public static class Reports
    {
        public const string Default = GroupName + ".Reports";
        public const string ViewDashboard = Default + ".ViewDashboard";
        public const string ExportExcel = Default + ".ExportExcel";
    }
}
