using Acme.HRM.Dtos;
using Acme.HRM.Entities;
using Acme.HRM.Enums;
using Acme.HRM.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Acme.HRM.ApplicationServices;

[Authorize(HRMPermissions.Attendances.Default)]
public class AttendanceAppService :
    CrudAppService<
        Attendance,
        AttendanceDto,
        long,
        GetAllAttendancesInput,
        CreateAttendanceDto,
        UpdateAttendanceDto>,
    IAttendanceAppService // 🔥 BỔ SUNG: Thực thi Interface chính thức
{
    private readonly IRepository<Employee, long> _employeeRepository;
    private readonly IRepository<WorkSchedule, long> _scheduleRepository;
    private readonly IClock _clock; // Đồng bộ múi giờ hệ thống của ABP

    public AttendanceAppService(
        IRepository<Attendance, long> repository,
        IRepository<Employee, long> employeeRepository,
        IRepository<WorkSchedule, long> scheduleRepository,
        IClock clock)
        : base(repository)
    {
        _employeeRepository = employeeRepository;
        _scheduleRepository = scheduleRepository;
        _clock = clock;

        // Cấu hình policy mặc định của ABP cho các hàm CRUD cơ bản
        GetPolicyName = HRMPermissions.Attendances.Default;
        GetListPolicyName = HRMPermissions.Attendances.Default;
        CreatePolicyName = HRMPermissions.Attendances.Update;
        UpdatePolicyName = HRMPermissions.Attendances.Update;
        DeletePolicyName = HRMPermissions.Attendances.Update;
    }

    // =========================================================================
    // ── NHÓM HÀM TRUY VẤN (GET SINGLE / GET LIST)
    // =========================================================================

    public override async Task<AttendanceDto> GetAsync(long id)
    {
        var query = await Repository.WithDetailsAsync(
            x => x.Employee,
            x => x.Schedule
        );

        var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);

        if (entity == null)
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Attendance), id);

        return ObjectMapper.Map<Attendance, AttendanceDto>(entity);
    }

    public override async Task<PagedResultDto<AttendanceDto>> GetListAsync(GetAllAttendancesInput input)
    {
        var query = await Repository.WithDetailsAsync(
             x => x.Employee,
             x => x.Schedule
        );

        // Áp dụng bộ lọc linh hoạt
        query = query
            .WhereIf(input.DepartmentId.HasValue, x => x.Employee.DepartmentId == input.DepartmentId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                x => x.Employee.FullName.Contains(input.Keyword) || x.Employee.EmployeeCode.Contains(input.Keyword))
            .WhereIf(input.EmployeeId.HasValue, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.ScheduleId.HasValue, x => x.ScheduleId == input.ScheduleId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.Source.HasValue, x => x.Source == input.Source)
            .WhereIf(input.IsLocked.HasValue, x => x.IsLocked == input.IsLocked)
            .WhereIf(input.WorkDateFrom.HasValue, x => x.WorkDate >= input.WorkDateFrom)
            .WhereIf(input.WorkDateTo.HasValue, x => x.WorkDate <= input.WorkDateTo)
            .WhereIf(input.Month.HasValue, x => x.WorkDate.Month == input.Month)
            .WhereIf(input.Year.HasValue, x => x.WorkDate.Year == input.Year);

        // Kiểm tra phân cấp quyền dữ liệu (Data Scope Filter)
        query = await ApplyDataScopeFilterAsync(query);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? $"{nameof(Attendance.WorkDate)} desc" : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<AttendanceDto>(
            totalCount,
            ObjectMapper.Map<List<Attendance>, List<AttendanceDto>>(items)
        );
    }

    // =========================================================================
    // ── NHÓM HÀM THAO TÁC QUẢN LÝ (HR MANAGER / ADMIN)
    // =========================================================================

    public override async Task<AttendanceDto> CreateAsync(CreateAttendanceDto input)
    {
        // Kiểm tra khóa sổ tháng
        await CheckIfPeriodIsLockedAsync(input.WorkDate.Year, input.WorkDate.Month);

        var exists = await Repository.AnyAsync(x =>
            x.EmployeeId == input.EmployeeId && x.WorkDate == input.WorkDate);

        if (exists)
            throw new UserFriendlyException($"Đã có dữ liệu chấm công của nhân viên này vào ngày {input.WorkDate:dd/MM/yyyy}.");

        var attendance = ObjectMapper.Map<CreateAttendanceDto, Attendance>(input);
        attendance.IsLocked = false;

        // Tính toán số phút tự động dựa trên Ca làm việc được chọn
        var schedule = await _scheduleRepository.GetAsync(input.ScheduleId);
        CalculateAttendanceMinutes(attendance, schedule);

        //if (attendance.Status == null)
            attendance.Status = DetermineStatus(attendance);

        await Repository.InsertAsync(attendance, autoSave: true);
        return await GetAsync(attendance.Id);
    }

    public override async Task<AttendanceDto> UpdateAsync(long id, UpdateAttendanceDto input)
    {
        var attendance = await Repository.GetAsync(id);

        if (attendance.IsLocked)
            throw new UserFriendlyException("Bản ghi chấm công đã bị khóa, không thể chỉnh sửa.");

        ObjectMapper.Map(input, attendance);

        // Tính toán lại số phút đi muộn về sớm sau khi HR cập nhật giờ thủ công
        var schedule = await _scheduleRepository.GetAsync(attendance.ScheduleId);
        CalculateAttendanceMinutes(attendance, schedule);

        //if (string.IsNullOrWhiteSpace(attendance.Status))
            attendance.Status = DetermineStatus(attendance);

        await Repository.UpdateAsync(attendance, autoSave: true);
        return await GetAsync(id);
    }

    public override async Task DeleteAsync(long id)
    {
        var attendance = await Repository.GetAsync(id);

        if (attendance.IsLocked)
            throw new UserFriendlyException("Bản ghi chấm công đã bị khóa, không thể xóa.");

        await Repository.DeleteAsync(id, autoSave: true);
    }

    // =========================================================================
    // ── NHÓM CHỨC NĂNG TỰ PHỤC VỤ (EMPLOYEE CHECK-IN/OUT)
    // =========================================================================

    [Authorize(HRMPermissions.Attendances.CheckInOut)]
    public async Task<AttendanceDto> CheckInAsync()
    {
        var employee = await GetCurrentEmployeeAsync();
        var now = _clock.Now;
        var today = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);

        await CheckIfPeriodIsLockedAsync(today.Year, today.Month);

        var attendance = await Repository.FirstOrDefaultAsync(x => x.EmployeeId == employee.Id && x.WorkDate == today);
        if (attendance != null)
        {
            throw new UserFriendlyException("Bạn đã thực hiện Check-In cho ngày hôm nay rồi!");
        }

        var schedule = await GetApplicableScheduleAsync(employee);

        attendance = new Attendance
        {
            EmployeeId = employee.Id,
            WorkDate = today,
            ScheduleId = schedule.Id,
            CheckInAt = currentTime,
            Source = AttendanceSource.Manual,
            IsLocked = false
        };

        CalculateAttendanceMinutes(attendance, schedule);
        attendance.Status = DetermineStatus(attendance);

        var inserted = await Repository.InsertAsync(attendance, autoSave: true);
        return ObjectMapper.Map<Attendance, AttendanceDto>(inserted);
    }

    [Authorize(HRMPermissions.Attendances.CheckInOut)]
    public async Task<AttendanceDto> CheckOutAsync()
    {
        var employee = await GetCurrentEmployeeAsync();
        var now = _clock.Now;
        var today = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);

        await CheckIfPeriodIsLockedAsync(today.Year, today.Month);

        var schedule = await GetApplicableScheduleAsync(employee);
        var attendance = await Repository.FirstOrDefaultAsync(x => x.EmployeeId == employee.Id && x.WorkDate == today);

        if (attendance == null)
        {
            // Trường hợp quên Check-In sáng, tạo mới bản ghi chỉ có giờ Check-Out
            attendance = new Attendance
            {
                EmployeeId = employee.Id,
                WorkDate = today,
                ScheduleId = schedule.Id,
                Source = AttendanceSource.Manual,
                IsLocked = false
            };
        }

        if (attendance.IsLocked)
            throw new UserFriendlyException("Ngày công này đã bị khóa sổ, không thể ghi nhận Check-Out.");

        attendance.CheckOutAt = currentTime;

        CalculateAttendanceMinutes(attendance, schedule);
        attendance.Status = DetermineStatus(attendance);

        var updated = await Repository.UpdateAsync(attendance, autoSave: true);
        return ObjectMapper.Map<Attendance, AttendanceDto>(updated);
    }

    // =========================================================================
    // ── NHÓM CHỨC NĂNG GIẢI TRÌNH CHẤM CÔNG (EXPLAIN / APPROVE)
    // =========================================================================

    [Authorize(HRMPermissions.Attendances.RequestExplain)]
    public async Task RequestExplainAsync(long id, string explainNote)
    {
        var attendance = await Repository.GetAsync(id);

        if (attendance.IsLocked)
            throw new UserFriendlyException("Không thể giải trình dữ liệu công đã khóa.");

        if (string.IsNullOrWhiteSpace(explainNote))
            throw new UserFriendlyException("Vui lòng nhập lý do giải trình.");

        attendance.ExplainNote = explainNote;
        attendance.ExplainStatus = AttendanceExplainStatus.Pending;

        await Repository.UpdateAsync(attendance);
    }

    [Authorize(HRMPermissions.Attendances.ApproveExplain)]
    public async Task ApproveExplainAsync(long id, string note)
    {
        var attendance = await Repository.GetAsync(id);

        if (attendance.IsLocked)
            throw new UserFriendlyException("Bảng công tháng này đã khóa, không thể duyệt giải trình.");

        attendance.ExplainStatus = AttendanceExplainStatus.Approved;
        var employee = await _employeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id);

        if (employee == null)
            throw new UserFriendlyException("Tài khoản chưa liên kết với hồ sơ nhân viên nào.");

        attendance.ExplainApprovedBy = employee.Id;

        // 💡 Nghiệp vụ đặc biệt: Duyệt giải trình thành công => Xóa phạt đi muộn/về sớm
        attendance.LateMinutes = 0;
        attendance.EarlyLeaveMinutes = 0;
        attendance.Status = AttendanceStatus.Present;

        await Repository.UpdateAsync(attendance);
    }

    // =========================================================================
    // ── NHÓM CHỨC NĂNG KHÓA CÔNG (LOCK / UNLOCK)
    // =========================================================================

    [Authorize(HRMPermissions.Attendances.Lock)]
    public async Task<AttendanceDto> LockAsync(long id)
    {
        var attendance = await Repository.GetAsync(id);
        if (attendance.IsLocked)
            throw new UserFriendlyException("Bản ghi đã được khóa trước đó.");

        attendance.IsLocked = true;
        await Repository.UpdateAsync(attendance, autoSave: true);
        return await GetAsync(id);
    }

    [Authorize(HRMPermissions.Attendances.Lock)]
    public async Task<AttendanceDto> UnlockAsync(long id)
    {
        var attendance = await Repository.GetAsync(id);
        attendance.IsLocked = false;
        await Repository.UpdateAsync(attendance, autoSave: true);
        return await GetAsync(id);
    }

    [Authorize(HRMPermissions.Attendances.Lock)]
    public async Task BulkLockAsync(GetAllAttendancesInput filter)
    {
        var query = await Repository.WithDetailsAsync(x => x.Employee);

        // 🔥 TỐI ƯU HÓA: Lấy toàn bộ thực thể ra để cập nhật hàng loạt thay vì lặp từng ID truy vấn đơn lẻ
        var records = await AsyncExecuter.ToListAsync(
            query
                .WhereIf(filter.EmployeeId.HasValue, x => x.EmployeeId == filter.EmployeeId)
                .WhereIf(filter.DepartmentId.HasValue, x => x.Employee.DepartmentId == filter.DepartmentId)
                .WhereIf(filter.Month.HasValue, x => x.WorkDate.Month == filter.Month)
                .WhereIf(filter.Year.HasValue, x => x.WorkDate.Year == filter.Year)
                .Where(x => !x.IsLocked)
        );

        foreach (var record in records)
        {
            record.IsLocked = true;
            await Repository.UpdateAsync(record);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    // =========================================================================
    // ── TRỢ THỦ NỘI BỘ (PRIVATE HELPER METHODS)
    // =========================================================================

    private static AttendanceStatus DetermineStatus(Attendance a)
    {
        if (a.CheckInAt == null && a.CheckOutAt == null) return AttendanceStatus.Absent; // Vắng mặt
        if (a.CheckInAt == null || a.CheckOutAt == null) return AttendanceStatus.HalfDay; // Thiếu ca / Nửa công
        if (a.LateMinutes > 0) return AttendanceStatus.Late; // Đi muộn
        if (a.EarlyLeaveMinutes > 0) return AttendanceStatus.Early; // Về sớm

        return AttendanceStatus.Present; // Đi làm đầy đủ, đúng giờ
    }

    private static void CalculateAttendanceMinutes(Attendance a, WorkSchedule schedule)
    {
        // 1. Tính số phút đi muộn (So sánh với CheckInTime)
        // Nếu dữ liệu dạng DateTime?, bạn đổi thành: a.CheckInAt.Value.TimeOfDay
        if (a.CheckInAt.HasValue && a.CheckInAt.Value.ToTimeSpan() > schedule.CheckInTime)
        {
            var diff = a.CheckInAt.Value.ToTimeSpan() - schedule.CheckInTime;
            int totalLateMinutes = (int)diff.TotalMinutes;

            // Áp dụng ngưỡng đi muộn cho phép (LateThresholdMinutes)
            // Nếu vượt quá số phút cho phép mới bắt đầu tính phút muộn, ngược lại coi như đúng giờ (= 0)
            a.LateMinutes = totalLateMinutes > schedule.LateThresholdMinutes ? totalLateMinutes : 0;
        }
        else
        {
            a.LateMinutes = 0;
        }

        // 2. Tính số phút về sớm (So sánh với CheckOutTime)
        if (a.CheckOutAt.HasValue && a.CheckOutAt.Value.ToTimeSpan() < schedule.CheckOutTime)
        {
            var diff = schedule.CheckOutTime - a.CheckOutAt.Value.ToTimeSpan();
            a.EarlyLeaveMinutes = (int)diff.TotalMinutes;
        }
        else
        {
            a.EarlyLeaveMinutes = 0;
        }
    }

    private async Task<Employee> GetCurrentEmployeeAsync()
    {
        if (CurrentUser.Id == null) throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn.");

        var employee = await _employeeRepository.FirstOrDefaultAsync(x => x.UserId == CurrentUser.Id);
        if (employee == null) throw new UserFriendlyException("Tài khoản chưa được liên kết tới bất kỳ hồ sơ nhân viên nào.");

        return employee;
    }

    private async Task<WorkSchedule> GetApplicableScheduleAsync(Employee employee)
    {
        // 🔥 ĐÃ FIX: Vì bảng Employee không gán cứng ScheduleId, hệ thống tự bốc ca đầu tiên (Mặc định) từ DB lên áp dụng
        var schedule = (await _scheduleRepository.GetListAsync()).FirstOrDefault();

        if (schedule == null)
            throw new UserFriendlyException("Hệ thống chưa thiết lập cấu hình Ca làm việc gốc.");

        return schedule;
    }

    private async Task CheckIfPeriodIsLockedAsync(int year, int month)
    {
        var isLocked = await Repository.AnyAsync(x =>
            x.WorkDate.Year == year && x.WorkDate.Month == month && x.IsLocked);

        if (isLocked)
            throw new UserFriendlyException($"Kỳ công tháng {month}/{year} đã đóng và khóa dữ liệu. Không được phép điều chỉnh!");
    }

    private async Task<IQueryable<Attendance>> ApplyDataScopeFilterAsync(IQueryable<Attendance> query)
    {
        // 🔥 ĐÃ FIX: Thay PermissionChecker bằng AuthorizationService tích hợp sẵn của ABP để hết lỗi static/instance
        var isHR = await AuthorizationService.IsGrantedAsync(HRMPermissions.Attendances.Update);
        if (!isHR && CurrentUser.Id.HasValue)
        {
            var employee = await _employeeRepository.FirstOrDefaultAsync(x => x.UserId == CurrentUser.Id);
            if (employee != null)
            {
                query = query.Where(x => x.EmployeeId == employee.Id);
            }
        }
        return query;
    }
}