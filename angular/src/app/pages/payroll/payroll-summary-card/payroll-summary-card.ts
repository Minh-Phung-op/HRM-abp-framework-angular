import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PayrollDto } from 'src/app/proxy/dtos/models';
import { ProgressBarModule } from 'primeng/progressbar';
import { DividerModule } from 'primeng/divider';

interface SummaryStats {
  totalGross: number;
  totalNet: number;
  totalPit: number;
  totalBhxh: number;
  totalCount: number;
  avgNet: number;
}

@Component({
  selector: 'app-payroll-summary-card',
  standalone: true,
  imports: [CommonModule, ProgressBarModule, DividerModule],
  templateUrl: './payroll-summary-card.html',
  styleUrl: './payroll-summary-card.scss',
})
export class PayrollSummaryCardComponent implements OnChanges {
  @Input() payrolls: PayrollDto[] = [];
  @Input() year = 0;
  @Input() month = 0;

  stats: SummaryStats = {
    totalGross: 0, totalNet: 0, totalPit: 0,
    totalBhxh: 0, totalCount: 0, avgNet: 0,
  };

  ngOnChanges(): void {
    this.stats = {
      totalGross: this.payrolls.reduce((s, p) => s + (p.grossSalary ?? 0), 0),
      totalNet: this.payrolls.reduce((s, p) => s + (p.netSalary ?? 0), 0),
      totalPit: this.payrolls.reduce((s, p) => s + (p.pit ?? 0), 0),
      totalBhxh: this.payrolls.reduce((s, p) => s + (p.bhxhEmployee ?? 0), 0),
      totalCount: this.payrolls.length,
      avgNet: this.payrolls.length
        ? this.payrolls.reduce((s, p) => s + (p.netSalary ?? 0), 0) / this.payrolls.length
        : 0,
    };
  }

  fmt(v: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(v);
  }
}