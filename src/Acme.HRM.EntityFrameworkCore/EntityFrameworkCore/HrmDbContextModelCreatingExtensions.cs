using Acme.HRM.Entities;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Acme.HRM.EntityFrameworkCore;

public static class HrmDbContextModelCreatingExtensions
{
    public static void ConfigureHrm(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        ConfigureDepartment(builder);
        ConfigurePosition(builder);
        ConfigureEmployee(builder);
        ConfigureContract(builder);
        ConfigureWorkSchedule(builder);
        ConfigureAttendance(builder);
        ConfigureLeaveType(builder);
        ConfigureLeaveBalance(builder);
        ConfigureLeaveRequest(builder);
        ConfigureLeaveRequestApprovalLog(builder);
        ConfigurePayroll(builder);
        ConfigurePayrollItem(builder);
        ConfigureProjectAndManyMany(builder);
    }

    private static void ConfigureDepartment(ModelBuilder builder)
    {
        builder.Entity<Department>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "Departments", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.HasIndex(x => x.Code).HasFilter("\"IsDeleted\" = false").IsUnique();

            b.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigurePosition(ModelBuilder builder)
    {
        builder.Entity<Position>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "Positions", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.Level).HasMaxLength(100);

            b.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEmployee(ModelBuilder builder)
    {
        builder.Entity<Employee>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "Employees", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);
            b.Property(x => x.Phone).HasMaxLength(20);
            b.Property(x => x.Gender).HasMaxLength(10);
            b.Property(x => x.NationalId).HasMaxLength(50);
            b.Property(x => x.Address).HasMaxLength(500);
            b.Property(x => x.Status).IsRequired().HasMaxLength(50);

            b.HasIndex(x => x.EmployeeCode)
             .HasFilter("\"IsDeleted\" = false")
             .IsUnique();

            // Sửa Index cho Email
            b.HasIndex(x => x.Email)
             .HasFilter("\"IsDeleted\" = false")
             .IsUnique();

            b.HasOne(e => e.User).WithOne().HasForeignKey<Employee>(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Contracts).WithOne(c => c.Employee).HasForeignKey(c => c.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureContract(ModelBuilder builder)
    {
        builder.Entity<Contract>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "Contracts", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ContractNumber).IsRequired().HasMaxLength(100);
            b.Property(x => x.ContractType).IsRequired().HasMaxLength(50);
            b.Property(x => x.Status).IsRequired().HasMaxLength(50);
            b.Property(x => x.BasicSalary).HasPrecision(18, 2);
            b.Property(x => x.InsuranceSalary).HasPrecision(18, 2);

            b.HasIndex(x => x.ContractNumber).HasFilter("\"IsDeleted\" = false").IsUnique();
            b.HasOne(x => x.Employee).WithMany(e => e.Contracts).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWorkSchedule(ModelBuilder builder)
    {
        builder.Entity<WorkSchedule>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "WorkSchedules", HRMConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.WorkingDays).IsRequired().HasMaxLength(100);
        });
    }

    private static void ConfigureAttendance(ModelBuilder builder)
    {
        builder.Entity<Attendance>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "Attendances", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Status).IsRequired().HasMaxLength(50);
            b.Property(x => x.Source).HasMaxLength(50);
            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.ExplainNote).HasMaxLength(500);

            b.HasIndex(x => new { x.EmployeeId, x.WorkDate }).IsUnique();
            b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Schedule).WithMany().HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureLeaveType(ModelBuilder builder)
    {
        builder.Entity<LeaveType>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "LeaveTypes", HRMConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.HasIndex(x => x.Code).HasFilter("\"IsDeleted\" = false").IsUnique();
        });
    }

    private static void ConfigureLeaveBalance(ModelBuilder builder)
    {
        builder.Entity<LeaveBalance>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "LeaveBalances", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.TotalDays).HasPrecision(5, 2);
            b.Property(x => x.UsedDays).HasPrecision(5, 2);
            b.Property(x => x.PendingDays).HasPrecision(5, 2);
            b.Ignore(x => x.TotalDays); // 🔥 Bỏ qua không map vào DB
            b.Ignore(x => x.RemainingDays);

            // Unique Index cho PostgreSQL kết hợp Soft Delete
            b.HasIndex(x => new { x.EmployeeId, x.LeaveTypeId, x.Year })
             .HasFilter("\"IsDeleted\" = false") // PostgreSQL dùng nháy kép cho tên cột và giá trị false/true
             .IsUnique();
            b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureLeaveRequest(ModelBuilder builder)
    {
        builder.Entity<LeaveRequest>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "LeaveRequests", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.TotalDays).HasPrecision(5, 2);
            b.Property(x => x.Reason).HasMaxLength(1000);

            // Ép cấu hình lưu trạng thái dạng chữ chuỗi Enum sạch sẽ
            b.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);

            b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.ApprovalLogs).WithOne(l => l.LeaveRequest).HasForeignKey(l => l.LeaveRequestId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureLeaveRequestApprovalLog(ModelBuilder builder)
    {
        builder.Entity<LeaveRequestApprovalLog>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "LeaveRequestApprovalLogs", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            // Sửa sang cấu hình cấu trúc dạng Enum chuỗi chính xác theo thiết kế mới
            b.Property(x => x.ActionStep).IsRequired().HasConversion<string>().HasMaxLength(50);
            b.Property(x => x.Action).IsRequired().HasConversion<string>().HasMaxLength(50);
            b.Property(x => x.Comment).HasMaxLength(500);

            b.HasIndex(x => x.LeaveRequestId);
            b.HasOne(x => x.LeaveRequest).WithMany(l => l.ApprovalLogs).HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePayroll(ModelBuilder builder)
    {
        builder.Entity<Payroll>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "Payrolls", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.BaseSalary).HasPrecision(18, 2);
            b.Property(x => x.GrossSalary).HasPrecision(18, 2);
            b.Property(x => x.NetSalary).HasPrecision(18, 2);
            b.Property(x => x.TotalDeduction).HasPrecision(18, 2);
            b.Property(x => x.BhxhEmployee).HasPrecision(18, 2);
            b.Property(x => x.BhytEmployee).HasPrecision(18, 2);
            b.Property(x => x.BhtnEmployee).HasPrecision(18, 2);
            b.Property(x => x.Pit).HasPrecision(18, 2);
            b.Property(x => x.Status).IsRequired().HasMaxLength(50);

            b.HasIndex(x => new { x.EmployeeId, x.Year, x.Month })
             .HasFilter("\"IsDeleted\" = false")
             .IsUnique();
            b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePayrollItem(ModelBuilder builder)
    {
        builder.Entity<PayrollItem>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "PayrollItems", HRMConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Type).IsRequired().HasMaxLength(50);
            b.Property(x => x.Label).IsRequired().HasMaxLength(256);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.Note).HasMaxLength(500);

            b.HasOne(x => x.Payroll).WithMany(p => p.Items).HasForeignKey(x => x.PayrollId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProjectAndManyMany(ModelBuilder builder)
    {
        // Thêm nốt các thực thể phụ từ file 1 gom về đây cho đồng bộ
        builder.Entity<EmployeeProject>(b =>
        {
            b.ToTable(HRMConsts.DbTablePrefix + "EmployeeProjects", HRMConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(ep => new { ep.EmployeeId, ep.ProjectId });
            b.HasOne(ep => ep.Employee).WithMany().HasForeignKey(ep => ep.EmployeeId);
            b.HasOne(ep => ep.Project).WithMany().HasForeignKey(ep => ep.ProjectId);
        });

        builder.Entity<Project>(b => {
            b.ToTable(HRMConsts.DbTablePrefix + "Projects", HRMConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
        });

        builder.Entity<EmployeeJobHistory>(b => {
            b.ToTable(HRMConsts.DbTablePrefix + "EmployeeJobHistories", HRMConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.OldBaseSalary).HasPrecision(18, 2);
            b.Property(x => x.NewBaseSalary).HasPrecision(18, 2);
            b.Property(x => x.Note).HasMaxLength(500);
        });
    }
}