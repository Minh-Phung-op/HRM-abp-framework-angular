import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PayrollDto } from 'src/app/proxy/dtos/models';
import { PayrollItemType } from 'src/app/proxy/enums';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-payslip-preview',
  standalone: true,
  imports: [CommonModule, ButtonModule, DividerModule],
  templateUrl: './payslip-preview.html',
  styleUrl: './payslip-preview.scss',
})
export class PayslipPreviewComponent {
  @Input() payroll!: PayrollDto;

  readonly PayrollItemType = PayrollItemType;

  get additions() {
    return (this.payroll.items ?? []).filter(i =>
      i.type === PayrollItemType.Bonus || i.type === PayrollItemType.Allowance
    );
  }

  get deductions() {
    return (this.payroll.items ?? []).filter(i =>
      i.type === PayrollItemType.Deduction
    );
  }

  fmt(v: number | undefined): string {
    if (v == null) return '—';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(v);
  }

  printPayslip(): void {
    window.print();
  }
}