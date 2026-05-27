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
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Acme.HRM.ApplicationServices
{
    [Authorize(HRMPermissions.LeaveRequests.Default)]
    public class LeaveRequestAppService : ApplicationService, ILeaveRequestAppService
    {
        private readonly IRepository<LeaveRequest, long> _leaveRequestRepository;
        private readonly IRepository<LeaveBalance, long> _leaveBalanceRepository;
        private readonly IRepository<Employee, long> _employeeRepository;
        private readonly IRepository<LeaveRequestApprovalLog, long> _approvalLogRepository;

        public LeaveRequestAppService(
            IRepository<LeaveRequest, long> leaveRequestRepository,
            IRepository<LeaveBalance, long> leaveBalanceRepository,
            IRepository<LeaveRequestApprovalLog, long> approvalLogRepository,
            IRepository<Employee, long> employeeRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _leaveBalanceRepository = leaveBalanceRepository;
            _employeeRepository = employeeRepository;
            _approvalLogRepository = approvalLogRepository;
        }

        private IQueryable<LeaveRequest> ApplyBaseFilters(IQueryable<LeaveRequest> query, GetAllLeaveRequestsInput input)
        {
            return query
                // Lọc theo từ khóa (Tên nhân viên hoặc Mã nhân viên)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                    x => x.Employee.FullName.Contains(input.Keyword) ||
                         x.Employee.EmployeeCode.Contains(input.Keyword))

                // Lọc theo ID nhân viên cụ thể
                .WhereIf(input.EmployeeId.HasValue, x => x.EmployeeId == input.EmployeeId)

                // Lọc theo phòng ban
                .WhereIf(input.DepartmentId.HasValue, x => x.Employee.DepartmentId == input.DepartmentId)

                // Lọc theo loại nghỉ (Phép năm, ốm, thai sản...)
                .WhereIf(input.LeaveTypeId.HasValue, x => x.LeaveTypeId == input.LeaveTypeId)

                // Lọc theo trạng thái đơn (Pending, Approved, Rejected...)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)

                // Lọc theo tháng/năm của ngày bắt đầu nghỉ
                .WhereIf(input.Month.HasValue, x => x.StartDate.Month == input.Month)
                .WhereIf(input.Year.HasValue, x => x.StartDate.Year == input.Year)

                // Lọc theo khoảng ngày (Date Range)
                .WhereIf(input.StartDateFrom.HasValue, x => x.StartDate >= input.StartDateFrom)
                .WhereIf(input.StartDateTo.HasValue, x => x.StartDate <= input.StartDateTo);
        }
        // ── GET SINGLE ──────────────────────────────────────────────
        public async Task<LeaveRequestDto> GetAsync(long id)
        {
            var entity = await GetWithNavigationAsync(id);
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();

            var isHR = await AuthorizationService.IsGrantedAsync(HRMPermissions.LeaveRequests.Update);

            // Kiểm tra quyền xem: 
            // 1. Là HR
            // 2. Là chủ đơn
            // 3. Là cấp trên trực tiếp (ManagerId của nhân viên làm đơn == ID người đang xem)
            bool isOwner = entity.EmployeeId == currentEmployeeId;
            bool isManager = entity.Employee?.ManagerId == currentEmployeeId;

            if (!isHR && !isOwner && !isManager)
            {
                throw new UserFriendlyException("Bạn không có quyền truy cập thông tin đơn này.");
            }

            return ObjectMapper.Map<LeaveRequest, LeaveRequestDto>(entity);
        }

        // ── GET LIST (Phân Scope Dữ Liệu Theo Quyền) ─────────────────
        public async Task<PagedResultDto<LeaveRequestDto>> GetListAsync(GetAllLeaveRequestsInput input)
        {
            var query = await _leaveRequestRepository.WithDetailsAsync(
                x => x.Employee,
                x => x.LeaveType
            );

            // 1. Áp dụng các bộ lọc cơ bản
            query = ApplyBaseFilters(query, input);

            // 2. 🔥 PHÂN SCOPE DỮ LIỆU (Security Mapping)
            var isHR = await AuthorizationService.IsGrantedAsync(HRMPermissions.LeaveRequests.Update);

            if (!isHR)
            {
                var currentEmployeeId = await GetCurrentEmployeeIdAsync();

                // Logic: Xem đơn của chính mình HOẶC đơn của cấp dưới trực tiếp
                query = query.Where(x =>
                    x.EmployeeId == currentEmployeeId ||
                    x.Employee.ManagerId == currentEmployeeId
                );
            }

            // 3. Thực thi paging và mapping
            var totalCount = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(
                query.OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime desc" : input.Sorting)
                     .PageBy(input.SkipCount, input.MaxResultCount)
            );

            return new PagedResultDto<LeaveRequestDto>(
                totalCount,
                ObjectMapper.Map<List<LeaveRequest>, List<LeaveRequestDto>>(items)
            );
        }

        public async Task<PagedResultDto<LeaveRequestDto>> GetPendingApprovalsAsync()
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();
            var query = await _leaveRequestRepository.WithDetailsAsync(x => x.Employee, x => x.LeaveType);

            // Chỉ lấy những đơn có Status = Pending AND Manager là tôi
            var queryPending = query.Where(x =>
                x.Employee.ManagerId == currentEmployeeId &&
                x.Status == LeaveRequestStatus.Pending // Giả sử bạn có Enum Status
            );

            var items = await AsyncExecuter.ToListAsync(queryPending);

            return new PagedResultDto<LeaveRequestDto>(
                items.Count,
                ObjectMapper.Map<List<LeaveRequest>, List<LeaveRequestDto>>(items)
            );
        }

        public async Task<List<LeaveRequestApprovalLogDto>> GetApprovalHistoryAsync(long id)
        {
            // Kiểm tra quyền xem đơn trước khi cho xem log (tái sử dụng logic bảo mật)
            await GetAsync(id);

            var logs = await _approvalLogRepository.GetListAsync(x => x.LeaveRequestId == id);
            return ObjectMapper.Map<List<LeaveRequestApprovalLog>, List<LeaveRequestApprovalLogDto>>(
                logs.OrderBy(x => x.CreationTime).ToList()
            );
        }

        // ── CREATE (Nộp Đơn Mới) ─────────────────────────────────────
        public async Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestDto input)
        {
            var employeeId = await GetCurrentEmployeeIdAsync();

            // 1. NGHIỆP VỤ CŨ: Kiểm tra logic ngày tháng cơ bản
            if (input.EndDate < input.StartDate)
                throw new UserFriendlyException("Ngày kết thúc phải sau hoặc trùng với ngày bắt đầu.");

            if (input.StartDate.Year != input.EndDate.Year)
                throw new UserFriendlyException("Đơn không được phép vắt qua hai năm. Vui lòng tách làm 2 đơn.");

            // 2. NGHIỆP VỤ CŨ: Tính số ngày làm việc thực tế
            decimal totalDays = CalculateWorkingDays(input.StartDate, input.EndDate);
            if (totalDays <= 0)
                throw new UserFriendlyException("Thời gian đăng ký không nằm vào ngày làm việc.");

            // 3. LOGIC MỚI: Tính toán quỹ phép thực tế (Thay vì chỉ tin vào balance.RemainingDays)
            var balance = await GetBalanceAsync(employeeId, input.LeaveTypeId, input.StartDate.Year);

            // Truy vấn tổng số ngày đã được duyệt hoặc đang chờ từ bảng LeaveRequest
            var queryable = await _leaveRequestRepository.GetQueryableAsync();

            var takenAndPendingInDb = await AsyncExecuter.SumAsync(
                queryable.Where(x => x.EmployeeId == employeeId && x.LeaveTypeId == input.LeaveTypeId &&
                                    x.StartDate.Year == input.StartDate.Year &&
                                    (x.Status == LeaveRequestStatus.Approved || x.Status == LeaveRequestStatus.Pending)),
                x => x.TotalDays);

            // Tính số ngày còn lại thực tế
            decimal actualRemaining = balance.TotalDays - takenAndPendingInDb;

            if (actualRemaining < totalDays)
                throw new UserFriendlyException($"Số ngày nghỉ còn lại thực tế ({actualRemaining} ngày) không đủ để đăng ký nghỉ {totalDays} ngày.");

            // 4. NGHIỆP VỤ CŨ: Chặn trùng lịch nghỉ
            var hasOverlap = await _leaveRequestRepository.AnyAsync(x =>
                x.EmployeeId == employeeId &&
                x.Status != LeaveRequestStatus.Rejected && x.Status != LeaveRequestStatus.Cancelled &&
                x.StartDate <= input.EndDate && x.EndDate >= input.StartDate);

            if (hasOverlap)
                throw new UserFriendlyException("Khoảng thời gian này bạn đã đăng ký một đơn nghỉ phép khác rồi.");

            // 5. THỰC THI: Lưu đơn và cập nhật Balance
            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employeeId,
                LeaveTypeId = input.LeaveTypeId,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                TotalDays = totalDays,
                Status = LeaveRequestStatus.Pending,
                Reason = input.Reason
            };

            await _leaveRequestRepository.InsertAsync(leaveRequest, autoSave: false);

            // Cập nhật lại cache PendingDays trong bảng Balance để hiển thị nhanh (nếu cần)
            balance.PendingDays = takenAndPendingInDb + totalDays;
            await _leaveBalanceRepository.UpdateAsync(balance, autoSave: false);

            await CurrentUnitOfWork.SaveChangesAsync();
            return await GetAsync(leaveRequest.Id);
        }

        // ── 🔥 BỔ SUNG: UPDATE (Sửa Đơn Khi Đang Chờ Duyệt) ───────────
        public async Task<LeaveRequestDto> UpdateAsync(long id, UpdateLeaveRequestDto input)
        {
            var request = await _leaveRequestRepository.GetAsync(id);
            var employeeId = await GetCurrentEmployeeIdAsync();

            if (request.EmployeeId != employeeId)
                throw new UserFriendlyException("Bạn không thể chỉnh sửa đơn của người khác.");

            if (request.Status != LeaveRequestStatus.Pending)
                throw new UserFriendlyException("Đơn đã được xử lý, không thể chỉnh sửa thông tin.");

            if (input.EndDate < input.StartDate)
                throw new UserFriendlyException("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");

            if (input.StartDate.Year != input.EndDate.Year)
                throw new UserFriendlyException("Đơn không được phép vắt qua hai năm khác nhau.");

            // Hoàn lại quỹ cũ trước khi tính quỹ mới
            var oldBalance = await GetBalanceAsync(employeeId, request.LeaveTypeId, request.StartDate.Year);
            oldBalance.PendingDays -= request.TotalDays;

            decimal newTotalDays = CalculateWorkingDays(input.StartDate, input.EndDate);

            // Kiểm tra quỹ mới dựa trên loại phép mới hoặc năm mới nếu có đổi
            var balance = await GetBalanceAsync(employeeId, input.LeaveTypeId, input.StartDate.Year);

            // 1. Tính tổng ngày nghỉ KHÔNG bao gồm đơn hiện tại đang sửa
            var queryable = await _leaveRequestRepository.GetQueryableAsync();
            var otherRequestsDays = await AsyncExecuter.SumAsync(
                queryable.Where(x => x.Id != id && x.EmployeeId == employeeId && x.LeaveTypeId == input.LeaveTypeId &&
                                    x.StartDate.Year == input.StartDate.Year &&
                                    (x.Status == LeaveRequestStatus.Approved || x.Status == LeaveRequestStatus.Pending)),
                x => x.TotalDays);

            // 2. Kiểm tra quỹ: (Tổng cấp - Các đơn khác) có đủ cho đơn mới không
            if ((balance.TotalDays - otherRequestsDays) < newTotalDays)
                throw new UserFriendlyException($"Không đủ quỹ phép. Còn lại: {balance.TotalDays - otherRequestsDays} ngày.");

            // 3. Cập nhật đơn
            request.LeaveTypeId = input.LeaveTypeId;
            request.StartDate = input.StartDate;
            request.EndDate = input.EndDate;
            request.TotalDays = newTotalDays;
            request.Reason = input.Reason;

            // 4. Cập nhật lại số PendingDays của Balance
            balance.PendingDays = otherRequestsDays + newTotalDays;

            await _leaveBalanceRepository.UpdateAsync(balance, autoSave: false);

            await CurrentUnitOfWork.SaveChangesAsync();
            return await GetAsync(id);
        }

        // ── APPROVE (Phê Duyệt Đơn) ──────────────────────────────────
        [Authorize(HRMPermissions.LeaveRequests.Update)]
        public async Task<LeaveRequestDto> ApproveAsync(long id, CreateLeaveRequestApprovalLogDto input)
        {
            var request = await GetWithNavigationAsync(id);
            if (request.Status != LeaveRequestStatus.Pending)
                throw new UserFriendlyException("Đơn không ở trạng thái chờ duyệt.");

            // 1. Kiểm tra quyền duyệt
            var currentUserId = CurrentUser.GetId();
            var isHRAdmin = await AuthorizationService.IsGrantedAsync(HRMPermissions.LeaveRequests.ApproveCompany);

            // Lấy thông tin cấp trên của người làm đơn
            var requester = await _employeeRepository.GetAsync(request.EmployeeId);

            // Logic: Nếu tôi là Manager của người đó HOẶC tôi là HR Admin
            bool canApprove = (requester.ManagerId.HasValue && requester.Manager.UserId == currentUserId) || isHRAdmin;

            if (!canApprove)
                throw new UserFriendlyException("Bạn không có quyền duyệt đơn cho nhân viên này.");

            // 2. Cập nhật trạng thái đơn
            request.Status = LeaveRequestStatus.Approved;

            // 3. Khấu trừ quỹ phép thực tế (Chuyển từ Pending sang Used)
            var balance = await GetBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);
            balance.PendingDays -= request.TotalDays;
            balance.UsedDays += request.TotalDays;

            // 4. Ghi Log
            await _approvalLogRepository.InsertAsync(new LeaveRequestApprovalLog
            {
                LeaveRequestId = request.Id,
                UserId = currentUserId,
                Action = ApprovalAction.Approve,
                Comment = input.Comment
            });

            await _leaveBalanceRepository.UpdateAsync(balance);
            await _leaveRequestRepository.UpdateAsync(request);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<LeaveRequest, LeaveRequestDto>(request);
        }

        // ── REJECT (Từ Chối Đơn) ──────────────────────────────────────
        [Authorize(HRMPermissions.LeaveRequests.Update)]
        public async Task<LeaveRequestDto> RejectAsync(long id, CreateLeaveRequestApprovalLogDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Comment))
                throw new UserFriendlyException("Bắt buộc phải nhập lý do từ chối đơn!");

            var request = await GetWithNavigationAsync(id);
            if (request.Status != LeaveRequestStatus.Pending)
                throw new UserFriendlyException("Đơn đã được xử lý, không thể từ chối.");

            // Kiểm tra quyền (giống Approve)
            var currentUserId = CurrentUser.GetId();
            var isHRAdmin = await AuthorizationService.IsGrantedAsync(HRMPermissions.LeaveRequests.ApproveCompany);
            var requester = await _employeeRepository.GetAsync(request.EmployeeId);

            bool canReject = (requester.ManagerId.HasValue && requester.Manager.UserId == currentUserId) || isHRAdmin;

            if (!canReject)
                throw new UserFriendlyException("Bạn không có quyền từ chối đơn cho nhân viên này.");

            // 1. Cập nhật trạng thái
            request.Status = LeaveRequestStatus.Rejected;

            // 2. Giải phóng số ngày Pending trả lại quỹ
            var balance = await GetBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);
            balance.PendingDays -= request.TotalDays;

            // 3. Ghi Log
            await _approvalLogRepository.InsertAsync(new LeaveRequestApprovalLog
            {
                LeaveRequestId = request.Id,
                UserId = currentUserId,
                Action = ApprovalAction.Reject,
                Comment = input.Comment
            });

            await _leaveBalanceRepository.UpdateAsync(balance);
            await _leaveRequestRepository.UpdateAsync(request);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<LeaveRequest, LeaveRequestDto>(request);
        }

        // ── CANCEL (Hủy Đơn Đã Nộp) ───────────────────────────────────
        public async Task<LeaveRequestDto> CancelAsync(long id)
        {
            var request = await GetWithNavigationAsync(id);
            var employeeId = await GetCurrentEmployeeIdAsync();

            if (request.EmployeeId != employeeId)
                throw new UserFriendlyException("Bạn không thể hủy đơn nghỉ phép của đồng nghiệp.");

            var balance = await GetBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);

            if (request.Status == LeaveRequestStatus.Approved)
            {
                // 🔥 ĐẶC BIỆT: Nếu đơn đã duyệt nhưng chưa đến ngày nghỉ, cho phép hủy và hoàn lại UsedDays
                if (request.StartDate < Clock.Now.Date)
                {
                    throw new UserFriendlyException("Không thể hủy đơn nghỉ phép đã diễn ra trong quá khứ.");
                }
                balance.UsedDays -= request.TotalDays;
            }
            else if (request.Status == LeaveRequestStatus.Pending)
            {
                // Đang chờ duyệt -> Trả lại quỹ PendingDays công bằng
                balance.PendingDays -= request.TotalDays;
            }
            else
            {
                throw new UserFriendlyException("Đơn đã hủy hoặc đã bị từ chối từ trước.");
            }

            request.Status = LeaveRequestStatus.Cancelled;

            await _leaveBalanceRepository.UpdateAsync(balance, autoSave: false);
            await _leaveRequestRepository.UpdateAsync(request, autoSave: false);

            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<LeaveRequest, LeaveRequestDto>(request);
        }

        // ── PRIVATE HELPERS ──────────────────────────────────────────

        private async Task<LeaveRequest> GetWithNavigationAsync(long id)
        {
            var query = await _leaveRequestRepository.WithDetailsAsync(
                x => x.Employee, x => x.LeaveType
            );
            var entity = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == id);

            if (entity == null)
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(LeaveRequest), id);

            return entity;
        }

        private async Task<LeaveBalance> GetBalanceAsync(long employeeId, long leaveTypeId, int year)
        {
            var balance = await _leaveBalanceRepository.FirstOrDefaultAsync(x =>
                x.EmployeeId == employeeId && 
                x.LeaveTypeId == leaveTypeId && 
                x.Year == year);

            if (balance == null)
                throw new UserFriendlyException($"Bạn không có cấu hình số dư quỹ phép năm {year} cho loại nghỉ này.");

            return balance;
        }

        private async Task<long> GetCurrentEmployeeIdAsync()
        {
            if (!CurrentUser.Id.HasValue)
                throw new UnauthorizedAccessException("Bạn chưa đăng nhập vào hệ thống.");

            // Sửa logic mapping từ CreatorId thành UserId chính xác
            var employee = await _employeeRepository.FirstOrDefaultAsync(x => x.UserId == CurrentUser.Id);

            if (employee == null)
                throw new UserFriendlyException("Tài khoản hệ thống của bạn chưa được liên kết với bất kỳ mã nhân sự nào.");

            return employee.Id;
        }

        private static decimal CalculateWorkingDays(DateTime start, DateTime end)
        {
            var days = 0;
            var current = start.Date;
            while (current <= end.Date)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                    days++;
                current = current.AddDays(1);
            }
            return days;
        }
    }
}