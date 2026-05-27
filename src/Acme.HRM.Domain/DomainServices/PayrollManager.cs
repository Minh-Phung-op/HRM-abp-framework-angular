using Acme.HRM.Entities;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Acme.HRM.DomainServices
{
    public class PayrollManager : DomainService
    {
        private readonly IRepository<Payroll, long> _payrollRepository;

        // Luật lao động hiện hành (Có thể nạp từ SettingManager nếu muốn cấu hình động)
        private const decimal BhxhRate = 0.08m;
        private const decimal BhytRate = 0.015m;
        private const decimal BhtnRate = 0.01m;

        public PayrollManager(IRepository<Payroll, long> payrollRepository)
        {
            _payrollRepository = payrollRepository;
        }

        public async Task EnsureNotDuplicateAsync(long employeeId, int year, int month)
        {
            var exists = await _payrollRepository.AnyAsync(x =>
                x.EmployeeId == employeeId && x.Year == year && x.Month == month);

            if (exists)
            {
                throw new UserFriendlyException($"Bảng lương tháng {month}/{year} của nhân viên này đã tồn tại trong hệ thống.");
            }
        }

        public void ExecuteSalaryCalculation(Payroll payroll)
        {
            // Inject thuật toán biểu thuế lũy tiến vào Entity để thực thi
            payroll.Calculate(BhxhRate, BhytRate, BhtnRate, CalculateProgressivePit);
        }

        /// <summary>
        /// Biểu thuế thu nhập cá nhân lũy tiến 7 bậc theo quy định pháp luật
        /// </summary>
        private decimal CalculateProgressivePit(decimal taxableIncome)
        {
            (decimal limit, decimal rate)[] brackets =
            {
                (5_000_000m,       0.05m),
                (10_000_000m,      0.10m),
                (18_000_000m,      0.15m),
                (32_000_000m,      0.20m),
                (52_000_000m,      0.25m),
                (80_000_000m,      0.30m),
                (decimal.MaxValue, 0.35m)
            };

            decimal tax = 0m;
            decimal previousLimit = 0m;

            foreach (var (limit, rate) in brackets)
            {
                if (taxableIncome <= previousLimit) break;

                decimal taxableSlice = Math.Min(taxableIncome, limit) - previousLimit;
                tax += taxableSlice * rate;
                previousLimit = limit;
            }

            return tax;
        }
    }
}