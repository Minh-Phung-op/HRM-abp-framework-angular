using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.HRM.Migrations
{
    /// <inheritdoc />
    public partial class fix_relationship_hasone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppAttendances_AppEmployees_EmployeeId1",
                table: "AppAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_AppAttendances_AppWorkSchedules_ScheduleId1",
                table: "AppAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_AppEmployees_AppDepartments_DepartmentId1",
                table: "AppEmployees");

            migrationBuilder.DropForeignKey(
                name: "FK_AppEmployees_AppEmployees_ManagerId1",
                table: "AppEmployees");

            migrationBuilder.DropForeignKey(
                name: "FK_AppEmployees_AppPositions_PositionId1",
                table: "AppEmployees");

            migrationBuilder.DropForeignKey(
                name: "FK_AppLeaveBalances_AppEmployees_EmployeeId1",
                table: "AppLeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_AppLeaveBalances_AppLeaveTypes_LeaveTypeId1",
                table: "AppLeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_AppPayrollItems_AppPayrolls_PayrollId1",
                table: "AppPayrollItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppPayrolls_AppEmployees_EmployeeId1",
                table: "AppPayrolls");

            migrationBuilder.DropForeignKey(
                name: "FK_AppPositions_AppDepartments_DepartmentId1",
                table: "AppPositions");

            migrationBuilder.DropIndex(
                name: "IX_AppPositions_DepartmentId1",
                table: "AppPositions");

            migrationBuilder.DropIndex(
                name: "IX_AppPayrolls_EmployeeId1",
                table: "AppPayrolls");

            migrationBuilder.DropIndex(
                name: "IX_AppPayrollItems_PayrollId1",
                table: "AppPayrollItems");

            migrationBuilder.DropIndex(
                name: "IX_AppLeaveBalances_EmployeeId1",
                table: "AppLeaveBalances");

            migrationBuilder.DropIndex(
                name: "IX_AppLeaveBalances_LeaveTypeId1",
                table: "AppLeaveBalances");

            migrationBuilder.DropIndex(
                name: "IX_AppEmployees_DepartmentId1",
                table: "AppEmployees");

            migrationBuilder.DropIndex(
                name: "IX_AppEmployees_ManagerId1",
                table: "AppEmployees");

            migrationBuilder.DropIndex(
                name: "IX_AppEmployees_PositionId1",
                table: "AppEmployees");

            migrationBuilder.DropIndex(
                name: "IX_AppAttendances_EmployeeId1",
                table: "AppAttendances");

            migrationBuilder.DropIndex(
                name: "IX_AppAttendances_ScheduleId1",
                table: "AppAttendances");

            migrationBuilder.DropColumn(
                name: "DepartmentId1",
                table: "AppPositions");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "AppPayrolls");

            migrationBuilder.DropColumn(
                name: "PayrollId1",
                table: "AppPayrollItems");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "AppLeaveBalances");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId1",
                table: "AppLeaveBalances");

            migrationBuilder.DropColumn(
                name: "DepartmentId1",
                table: "AppEmployees");

            migrationBuilder.DropColumn(
                name: "ManagerId1",
                table: "AppEmployees");

            migrationBuilder.DropColumn(
                name: "PositionId1",
                table: "AppEmployees");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "AppAttendances");

            migrationBuilder.DropColumn(
                name: "ScheduleId1",
                table: "AppAttendances");

            migrationBuilder.AddColumn<DateOnly>(
                name: "SignDate",
                table: "AppContracts",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignDate",
                table: "AppContracts");

            migrationBuilder.AddColumn<long>(
                name: "DepartmentId1",
                table: "AppPositions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "EmployeeId1",
                table: "AppPayrolls",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PayrollId1",
                table: "AppPayrollItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "EmployeeId1",
                table: "AppLeaveBalances",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LeaveTypeId1",
                table: "AppLeaveBalances",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DepartmentId1",
                table: "AppEmployees",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ManagerId1",
                table: "AppEmployees",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PositionId1",
                table: "AppEmployees",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "EmployeeId1",
                table: "AppAttendances",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ScheduleId1",
                table: "AppAttendances",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_AppPositions_DepartmentId1",
                table: "AppPositions",
                column: "DepartmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppPayrolls_EmployeeId1",
                table: "AppPayrolls",
                column: "EmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppPayrollItems_PayrollId1",
                table: "AppPayrollItems",
                column: "PayrollId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveBalances_EmployeeId1",
                table: "AppLeaveBalances",
                column: "EmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppLeaveBalances_LeaveTypeId1",
                table: "AppLeaveBalances",
                column: "LeaveTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_DepartmentId1",
                table: "AppEmployees",
                column: "DepartmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_ManagerId1",
                table: "AppEmployees",
                column: "ManagerId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployees_PositionId1",
                table: "AppEmployees",
                column: "PositionId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppAttendances_EmployeeId1",
                table: "AppAttendances",
                column: "EmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppAttendances_ScheduleId1",
                table: "AppAttendances",
                column: "ScheduleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppAttendances_AppEmployees_EmployeeId1",
                table: "AppAttendances",
                column: "EmployeeId1",
                principalTable: "AppEmployees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppAttendances_AppWorkSchedules_ScheduleId1",
                table: "AppAttendances",
                column: "ScheduleId1",
                principalTable: "AppWorkSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppEmployees_AppDepartments_DepartmentId1",
                table: "AppEmployees",
                column: "DepartmentId1",
                principalTable: "AppDepartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppEmployees_AppEmployees_ManagerId1",
                table: "AppEmployees",
                column: "ManagerId1",
                principalTable: "AppEmployees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppEmployees_AppPositions_PositionId1",
                table: "AppEmployees",
                column: "PositionId1",
                principalTable: "AppPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppLeaveBalances_AppEmployees_EmployeeId1",
                table: "AppLeaveBalances",
                column: "EmployeeId1",
                principalTable: "AppEmployees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppLeaveBalances_AppLeaveTypes_LeaveTypeId1",
                table: "AppLeaveBalances",
                column: "LeaveTypeId1",
                principalTable: "AppLeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppPayrollItems_AppPayrolls_PayrollId1",
                table: "AppPayrollItems",
                column: "PayrollId1",
                principalTable: "AppPayrolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppPayrolls_AppEmployees_EmployeeId1",
                table: "AppPayrolls",
                column: "EmployeeId1",
                principalTable: "AppEmployees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppPositions_AppDepartments_DepartmentId1",
                table: "AppPositions",
                column: "DepartmentId1",
                principalTable: "AppDepartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
