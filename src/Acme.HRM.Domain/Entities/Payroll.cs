using Acme.HRM.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.HRM.Entities
{
    public class Payroll : FullAuditedEntity<long>
    {
        public long EmployeeId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal Pit { get; set; }            // Personal Income Tax
        public decimal BhxhEmployee { get; set; }  // Social Insurance (8%)
        public decimal BhytEmployee { get; set; }  // Health Insurance (1.5%)
        public decimal BhtnEmployee { get; set; }  // Unemployment Insurance (1%)
        //public decimal BhxhEmployer { get; set; }
        //public decimal BhytEmployer { get; set; } 
        //public decimal BhtnEmployer { get; set; }
        public PayrollStatus Status { get; set; }
        public DateTime? LockedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        // Navigation
        public Employee Employee { get; set; }
        public List<PayrollItem> Items { get; set; } = new();

        public void Calculate(
            decimal bhxhRate,
            decimal bhytRate,
            decimal bhtnRate,
            Func<decimal, decimal> pitCalculator)
        {
            if (LockedAt.HasValue)
            {
                throw new UserFriendlyException("Bảng lương đã bị khóa, không thể tính toán lại.");
            }

            decimal totalAddition = 0;
            decimal totalManualDeduction = 0;

            foreach (var item in Items)
            {
                if (item.Type == PayrollItemType.Allowance || item.Type == PayrollItemType.Bonus)
                {
                    totalAddition += item.Amount;
                }
                else if (item.Type == PayrollItemType.Deduction || item.Type == PayrollItemType.Advance)
                {
                    totalManualDeduction += item.Amount;
                }
            }

            GrossSalary = BaseSalary + totalAddition;

            // Tính các khoản bảo hiểm bắt buộc
            BhxhEmployee = Math.Round(BaseSalary * bhxhRate, 0);
            BhytEmployee = Math.Round(BaseSalary * bhytRate, 0);
            BhtnEmployee = Math.Round(BaseSalary * bhtnRate, 0);

            // Tính thu nhập chịu thuế (Giảm trừ bản thân mặc định 11 triệu)
            decimal taxableIncome = GrossSalary - BhxhEmployee - BhytEmployee - BhtnEmployee - 11_000_000m;

            // Gọi hàm tính thuế lũy tiến từ Domain Service truyền vào
            Pit = taxableIncome > 0 ? Math.Round(pitCalculator(taxableIncome), 0) : 0;

            TotalDeduction = BhxhEmployee + BhytEmployee + BhtnEmployee + Pit + totalManualDeduction;
            NetSalary = GrossSalary - TotalDeduction;
        }
    }

    public class PayrollItem : FullAuditedEntity<long>
    {
        public long PayrollId { get; set; }
        public PayrollItemType Type { get; set; }
        public string Label { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }

        // Navigation
        public Payroll Payroll { get; set; }
    }
}
