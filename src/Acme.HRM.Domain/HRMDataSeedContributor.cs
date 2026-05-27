using System;
using System.Threading.Tasks;
using Acme.HRM.Entities;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Acme.HRM.Data;

public class HRMDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<WorkSchedule, long> _scheduleRepository;

    public HRMDataSeedContributor(IRepository<WorkSchedule, long> scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Kiểm tra xem dưới DB đã có ca làm việc nào chưa, nếu chưa có mới tạo
        if (await _scheduleRepository.GetCountAsync() > 0)
        {
            return;
        }

        // 1. Tạo ca Hành chính mặc định
        var officeSchedule = new WorkSchedule
        {
            Name = "Ca Hành Chính Văn Phòng",
            CheckInTime = new TimeSpan(8, 0, 0),   // 08:00 sáng
            CheckOutTime = new TimeSpan(17, 0, 0),  // 17:00 chiều
            IsDefault = true // Thêm cờ này trong Entity nếu cần đánh dấu ca mặc định
        };

        // 2. Tạo thêm ca sáng/chiều phụ nếu muốn (Tùy chọn)
        var morningSchedule = new WorkSchedule
        {
            Name = "Ca Sáng Nhà Máy",
            CheckInTime = new TimeSpan(6, 0, 0),
            CheckOutTime = new TimeSpan(14, 0, 0),
            IsDefault = false
        };

        await _scheduleRepository.InsertAsync(officeSchedule, autoSave: true);
        await _scheduleRepository.InsertAsync(morningSchedule, autoSave: true);
    }
}