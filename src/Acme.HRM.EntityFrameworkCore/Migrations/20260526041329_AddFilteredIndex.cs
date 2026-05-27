using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.HRM.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppPayrolls_EmployeeId_Year_Month",
                table: "AppPayrolls");

            migrationBuilder.DropIndex(
                name: "IX_AppLeaveTypes_Code",
                table: "AppLeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_AppLeaveBalances_EmployeeId_LeaveTypeId_Year",
                table: "AppLeaveBalances");

            migrationBuilder.DropIndex(
                name: "IX_AppEmployees_Email",
                table: "AppEmployees");

            migrationBuilder.DropIndex(
                name: "IX_AppEmployees_EmployeeCode",
                table: "AppEmployees");

            migrationBuilder.DropIndex(
                name: "IX_AppDepartments_Code",
                table: "AppDepartments");

            migrationBuilder.DropIndex(
                name: "IX_AppContracts_ContractNumber",
                table: "AppContracts");

            migrationBuilder.CreateIndex(
                name: "IX_AppPayrolls_EmployeeId_Year_Month",
                table: "AppPayrolls",
                columns: new[] { "EmployeeId", "Year", "Month" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveTypes_Code",
                table: "AppLeaveTypes",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveBalances_EmployeeId_LeaveTypeId_Year",
                table: "AppLeaveBalances",
                columns: new[] { "EmployeeId", "LeaveTypeId", "Year" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_Email",
                table: "AppEmployees",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_EmployeeCode",
                table: "AppEmployees",
                column: "EmployeeCode",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppDepartments_Code",
                table: "AppDepartments",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_ContractNumber",
                table: "AppContracts",
                column: "ContractNumber",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppPayrolls_EmployeeId_Year_Month",
                table: "AppPayrolls");

            migrationBuilder.DropIndex(
                name: "IX_AppLeaveTypes_Code",
                table: "AppLeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_AppLeaveBalances_EmployeeId_LeaveTypeId_Year",
                table: "AppLeaveBalances");

            migrationBuilder.DropIndex(
                name: "IX_AppEmployees_Email",
                table: "AppEmployees");

            migrationBuilder.DropIndex(
                name: "IX_AppEmployees_EmployeeCode",
                table: "AppEmployees");

            migrationBuilder.DropIndex(
                name: "IX_AppDepartments_Code",
                table: "AppDepartments");

            migrationBuilder.DropIndex(
                name: "IX_AppContracts_ContractNumber",
                table: "AppContracts");

            migrationBuilder.CreateIndex(
                name: "IX_AppPayrolls_EmployeeId_Year_Month",
                table: "AppPayrolls",
                columns: new[] { "EmployeeId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveTypes_Code",
                table: "AppLeaveTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveBalances_EmployeeId_LeaveTypeId_Year",
                table: "AppLeaveBalances",
                columns: new[] { "EmployeeId", "LeaveTypeId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_Email",
                table: "AppEmployees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_EmployeeCode",
                table: "AppEmployees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDepartments_Code",
                table: "AppDepartments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_ContractNumber",
                table: "AppContracts",
                column: "ContractNumber",
                unique: true);
        }
    }
}
