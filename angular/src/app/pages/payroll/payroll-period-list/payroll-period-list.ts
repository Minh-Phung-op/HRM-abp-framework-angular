import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { DepartmentService, PayrollService } from 'src/app/proxy/application-services';
import { PayrollDto, GeneratePayrollInput, GetAllPayrollsInput, DepartmentDto } from 'src/app/proxy/dtos/models';
import { PayrollStatus } from 'src/app/proxy/enums';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { ProgressBarModule } from 'primeng/progressbar';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { MessageService, ConfirmationService } from 'primeng/api';
import { InputNumberModule } from 'primeng/inputnumber';

interface PeriodSummary {
  year: number;
  month: number;
  totalCount: number;
  approvedCount: number;
  paidCount: number;
  totalNet: number;
  status: 'empty' | 'draft' | 'partial' | 'approved' | 'paid';
}

@Component({
  selector: 'app-payroll-period-list',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    ButtonModule, CardModule, TagModule, DialogModule, SelectModule,
    ProgressBarModule, SkeletonModule, ToastModule, ConfirmDialogModule,
    TooltipModule, DividerModule, InputNumberModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './payroll-period-list.html',
  styleUrl: './payroll-period-list.scss',
})
export class PayrollPeriodListComponent implements OnInit {
  private payrollService = inject(PayrollService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);
  private confirmService = inject(ConfirmationService);
  private departmentService = inject(DepartmentService);

  periods = signal<PeriodSummary[]>([]);
  isLoading = signal(false);
  isGenerating = signal(false);
  showGenerateDialog = signal(false);
  departmentOptions = signal<DepartmentDto[]>([]);

  currentYear = new Date().getFullYear();
  currentMonth = new Date().getMonth() + 1;

  yearOptions = Array.from({ length: 5 }, (_, i) => ({
    label: String(this.currentYear - 1 + i),
    value: this.currentYear - 1 + i,
  }));

  monthOptions = Array.from({ length: 12 }, (_, i) => ({
    label: `Tháng ${String(i + 1).padStart(2, '0')}`,
    value: i + 1,
  }));

  generateForm: FormGroup = this.fb.group({
    year: [this.currentYear, [Validators.required, Validators.min(2000), Validators.max(2100)]],
    month: [this.currentMonth, Validators.required],
    departmentId: [null],
  });

  private readonly errorMessages: Record<string, Record<string, string>> = {
    year: { required: 'Vui lòng chọn năm', min: 'Năm không hợp lệ', max: 'Năm không hợp lệ' },
    month: { required: 'Vui lòng chọn tháng' },
  };

  ngOnInit(): void {
    this.loadPeriods();
    this.loadDepartmentOptions();
  }

  loadPeriods(): void {
    this.isLoading.set(true);
    // Lấy danh sách payroll, group theo tháng/năm
    this.payrollService.getList({
      maxResultCount: 500,
      skipCount: 0,
    }).subscribe({
      next: (res) => {
        this.periods.set(this.groupByPeriod(res.items || []));
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  loadDepartmentOptions() {
    this.departmentService.getList({ maxResultCount: 1000, isActive: true }).subscribe(res => {
      this.departmentOptions.set(res.items || []);
    });
  }

  private groupByPeriod(payrolls: PayrollDto[]): PeriodSummary[] {
    const map = new Map<string, PayrollDto[]>();
    for (const p of payrolls) {
      const key = `${p.year}-${p.month}`;
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(p);
    }

    const result: PeriodSummary[] = [];
    map.forEach((items, key) => {
      const [year, month] = key.split('-').map(Number);
      const approvedCount = items.filter(p =>
        p.status === PayrollStatus.Approved || p.status === PayrollStatus.Paid
      ).length;
      const paidCount = items.filter(p => p.status === PayrollStatus.Paid).length;
      const totalNet = items.reduce((s, p) => s + (p.netSalary ?? 0), 0);

      let status: PeriodSummary['status'] = 'draft';
      if (paidCount === items.length) status = 'paid';
      else if (approvedCount === items.length) status = 'approved';
      else if (approvedCount > 0) status = 'partial';

      result.push({ year, month, totalCount: items.length, approvedCount, paidCount, totalNet, status });
    });

    return result.sort((a, b) => b.year !== a.year ? b.year - a.year : b.month - a.month);
  }

  openGenerate(): void {
    this.generateForm.reset({ year: this.currentYear, month: this.currentMonth });
    this.showGenerateDialog.set(true);
  }

  closeGenerate(): void {
    this.showGenerateDialog.set(false);
    this.generateForm.reset();
  }

  submitGenerate(): void {
    if (this.generateForm.invalid) {
      this.generateForm.markAllAsTouched();
      return;
    }
    const v = this.generateForm.value;
    const payload: GeneratePayrollInput = {
      year: v.year,
      month: v.month,
      departmentId: v.departmentId ?? null,
    };
    this.isGenerating.set(true);
    this.payrollService.generate(payload).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Thành công', detail: `Đã khởi tạo kỳ lương tháng ${v.month}/${v.year}` });
        this.isGenerating.set(false);
        this.closeGenerate();
        this.loadPeriods();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Khởi tạo thất bại' });
        this.isGenerating.set(false);
      },
    });
  }

  goToWorksheet(period: PeriodSummary): void {
    this.router.navigate(['/payroll/worksheet'], {
      queryParams: { year: period.year, month: period.month },
    });
  }

  getProgressValue(period: PeriodSummary): number {
    if (!period.totalCount) return 0;
    return Math.round((period.approvedCount / period.totalCount) * 100);
  }

  getPeriodStatusLabel(status: PeriodSummary['status']): string {
    const map: Record<string, string> = {
      draft: 'Nháp', partial: 'Đang xử lý',
      approved: 'Đã duyệt', paid: 'Đã thanh toán', empty: 'Trống',
    };
    return map[status] ?? '—';
  }

  getPeriodStatusSeverity(status: PeriodSummary['status']): string {
    const map: Record<string, string> = {
      draft: 'secondary', partial: 'warn',
      approved: 'info', paid: 'success', empty: 'secondary',
    };
    return map[status] ?? 'secondary';
  }

  isInvalid(field: string): boolean {
    const ctrl = this.generateForm.get(field);
    return !!(ctrl?.invalid && (ctrl.touched || ctrl.dirty));
  }

  getError(field: string): string {
    const ctrl = this.generateForm.get(field);
    if (!ctrl?.errors) return '';
    const key = Object.keys(ctrl.errors)[0];
    return this.errorMessages[field]?.[key] ?? 'Không hợp lệ';
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }
}