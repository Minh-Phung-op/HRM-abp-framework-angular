using Acme.HRM.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Acme.HRM.Permissions;

public class HRMPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        // 1. Tạo nhóm quyền lớn hiển thị ở menu bên trái của bảng phân quyền
        var hrmGroup = context.AddGroup(HRMPermissions.GroupName, L("Permission:HRM"));

        // 2. Định nghĩa các quyền cho MODULE NHÂN VIÊN
        var employeePerm = hrmGroup.AddPermission(HRMPermissions.Employees.Default, L("Permission:Employees"));
        employeePerm.AddChild(HRMPermissions.Employees.Create, L("Permission:Employees.Create"));
        employeePerm.AddChild(HRMPermissions.Employees.Update, L("Permission:Employees.Update"));
        employeePerm.AddChild(HRMPermissions.Employees.Offboard, L("Permission:Employees.Offboard"));
        employeePerm.AddChild(HRMPermissions.Employees.Delete, L("Permission:Employees.Delete"));
        employeePerm.AddChild(HRMPermissions.Employees.Export, L("Permission:Employees.Export"));

        // department
        var departmentPerm = hrmGroup.AddPermission(HRMPermissions.Departments.Default, L("Permission:Departments"));
        departmentPerm.AddChild(HRMPermissions.Departments.Manage, L("Permission:Departments.Manage"));

        // position
        var positionPerm = hrmGroup.AddPermission(HRMPermissions.Positions.Default, L("Permission:Positions"));
        departmentPerm.AddChild(HRMPermissions.Positions.Manage, L("Permission:Position.Manage"));


        // 3. Định nghĩa các quyền cho MODULE CHẤM CÔNG
        hrmGroup.AddPermission(HRMPermissions.WorkSchedules.Manage, L("Permission:WorkSchedules.Manage"));

        var attendancePerm = hrmGroup.AddPermission(HRMPermissions.Attendances.Default, L("Permission:Attendances"));
        attendancePerm.AddChild(HRMPermissions.Attendances.CheckInOut, L("Permission:Attendances.CheckInOut"));
        attendancePerm.AddChild(HRMPermissions.Attendances.ViewTeam, L("Permission:Attendances.ViewTeam"));
        attendancePerm.AddChild(HRMPermissions.Attendances.Update, L("Permission:Attendances.Update"));
        attendancePerm.AddChild(HRMPermissions.Attendances.Lock, L("Permission:Attendances.Lock"));
        attendancePerm.AddChild(HRMPermissions.Attendances.RequestExplain, L("Permission:Attendances.RequestExplain"));
        attendancePerm.AddChild(HRMPermissions.Attendances.ApproveExplain, L("Permission:Attendances.ApproveExplain"));

        // 4. Định nghĩa các quyền cho MODULE NGHỈ PHÉP
        hrmGroup.AddPermission(HRMPermissions.LeaveTypes.Default, L("Permission:LeaveTypes.Default"));
        hrmGroup.AddPermission(HRMPermissions.LeaveTypes.Manage, L("Permission:LeaveTypes.Manage"));
        hrmGroup.AddPermission(HRMPermissions.LeaveBalances.Default, L("Permission:LeaveBalances.Default"));
        hrmGroup.AddPermission(HRMPermissions.LeaveBalances.Manage, L("Permission:LeaveBalances.Manage"));

        var leaveRequestPerm = hrmGroup.AddPermission(HRMPermissions.LeaveRequests.Default, L("Permission:LeaveRequests"));

        leaveRequestPerm.AddChild(HRMPermissions.LeaveRequests.Create, L("Permission:LeaveRequests.Create"));
        leaveRequestPerm.AddChild(HRMPermissions.LeaveRequests.Update, L("Permission:LeaveRequests.Update"));
        leaveRequestPerm.AddChild(HRMPermissions.LeaveRequests.ApproveTeam, L("Permission:LeaveRequests.ApproveTeam"));
        leaveRequestPerm.AddChild(HRMPermissions.LeaveRequests.ApproveCompany, L("Permission:LeaveRequests.ApproveCompany"));

        // 5. Định nghĩa các quyền cho MODULE LƯƠNG
        var payrollPerm = hrmGroup.AddPermission(HRMPermissions.Payrolls.Default, L("Permission:Payrolls"));
        payrollPerm.AddChild(HRMPermissions.Payrolls.Management,L("Permission:Payrolls.Management"));
        payrollPerm.AddChild(HRMPermissions.Payrolls.Configure, L("Permission:Payrolls.Configure"));
        payrollPerm.AddChild(HRMPermissions.Payrolls.Create, L("Permission:Payrolls.Create"));
        payrollPerm.AddChild(HRMPermissions.Payrolls.Approve, L("Permission:Payrolls.Approve"));
        payrollPerm.AddChild(HRMPermissions.Payrolls.MarkPaid, L("Permission:Payrolls.MarkPaid"));
        payrollPerm.AddChild(HRMPermissions.Payrolls.SendPayslip, L("Permission:Payrolls.SendPayslip"));

        // 6. HỆ THỐNG & BÁO CÁO
        hrmGroup.AddPermission(HRMPermissions.SystemSettings.ManageCompanyInfo, L("Permission:SystemSettings.ManageCompanyInfo"));
        hrmGroup.AddPermission(HRMPermissions.SystemSettings.DeviceIntegration, L("Permission:SystemSettings.DeviceIntegration"));
        hrmGroup.AddPermission(HRMPermissions.SystemSettings.BackupRestore, L("Permission:SystemSettings.BackupRestore"));

        var reportPerm = hrmGroup.AddPermission(HRMPermissions.Reports.Default, L("Permission:Reports"));
        reportPerm.AddChild(HRMPermissions.Reports.ViewDashboard, L("Permission:Reports.ViewDashboard"));
        reportPerm.AddChild(HRMPermissions.Reports.ExportExcel, L("Permission:Reports.ExportExcel"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HRMResource>(name);
    }
}
