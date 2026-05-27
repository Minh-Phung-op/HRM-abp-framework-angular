import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import { PayrollService } from 'src/app/proxy/application-services';
import { PayrollDto } from 'src/app/proxy/dtos/models';
import { PayrollStatus } from 'src/app/proxy/enums';

import { PayrollSummaryCardComponent } from '../payroll-summary-card/payroll-summary-card';
import { PayrollDetailSidePanelComponent } from '../payroll-detail-side-panel/payroll-detail-side-panel';
import { PayslipPreviewComponent } from '../payslip-preview/payslip-preview';

import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { SkeletonModule } from 'primeng/skeleton';
import { MenuModule } from 'primeng/menu';
import { MessageService, ConfirmationService, MenuItem } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';

@Component({
  selector: 'app-payroll-worksheet',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    PayrollSummaryCardComponent, PayrollDetailSidePanelComponent, PayslipPreviewComponent,
    ButtonModule, TableModule, TagModule, InputTextModule, SelectModule,
    IconFieldModule, InputIconModule, ToastModule, ConfirmDialogModule,
    TooltipModule, SkeletonModule, MenuModule, DialogModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './payroll-worksheet.html',
  styleUrl: './payroll-worksheet.scss',
})
export class PayrollWorksheetComponent implements OnInit {
  private route          = inject(ActivatedRoute);
  private payrollService = inject(PayrollService);
  private fb             = inject(FormBuilder);
  private messageService = inject(MessageService);
  private confirmService = inject(ConfirmationService);

  year  = signal(0);
  month = signal(0);

  payrolls      = signal<PayrollDto[]>([]);
  filtered      = signal<PayrollDto[]>([]);
  isLoading     = signal(false);
  isProcessing  = signal(false);

  // Side panel
  selectedPayroll  = signal<PayrollDto | null>(null);
  showSidePanel    = signal(false);

  // Payslip preview
  previewPayroll   = signal<PayrollDto | null>(null);
  showPayslip      = signal(false);

  filterForm: FormGroup = this.fb.group({
    keyword:    [''],
    department: [''],
  });

  readonly PayrollStatus = PayrollStatus;

  monthName = computed(() =>
    `Tháng ${String(this.month()).padStart(2, '0')}/${this.year()}`
  );

  canApproveAll = computed(() =>
    this.filtered().some(p =>
      p.status === PayrollStatus.Calculated || p.status === PayrollStatus.Processing
    )
  );

  ngOnInit(): void {
    this.year.set(Number(this.route.snapshot.queryParamMap.get('year'))  || new Date().getFullYear());
    this.month.set(Number(this.route.snapshot.queryParamMap.get('month')) || new Date().getMonth() + 1);
    this.loadPayrolls();
  }

  loadPayrolls(): void {
    this.isLoading.set(true);
    this.payrollService.getList({
      year: this.year(),
      month: this.month(),
      maxResultCount: 500,
      skipCount: 0,
    }).subscribe({
      next: (res) => {
        this.payrolls.set(res.items!);
        this.applyFilter();
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  applyFilter(): void {
    const { keyword, department } = this.filterForm.value;
    let list = [...this.payrolls()];
    if (keyword) {
      const kw = keyword.toLowerCase();
      list = list.filter(p =>
        p.employeeName?.toLowerCase().includes(kw) ||
        p.employeeCode?.toLowerCase().includes(kw)
      );
    }
    if (department) {
      list = list.filter(p => p.departmentName === department);
    }
    this.filtered.set(list);
  }

  get departments(): string[] {
    return [...new Set(this.payrolls().map(p => p.departmentName ?? '').filter(Boolean))];
  }

  // ---- Side panel ----
  openSidePanel(payroll: PayrollDto): void {
    this.selectedPayroll.set(payroll);
    this.showSidePanel.set(true);
  }

  closeSidePanel(): void {
    this.showSidePanel.set(false);
    this.selectedPayroll.set(null);
  }

  onPayrollUpdated(updated: PayrollDto): void {
    this.payrolls.update(list => list.map(p => p.id === updated.id ? updated : p));
    this.selectedPayroll.set(updated);
    this.applyFilter();
  }

  // ---- Payslip ----
  openPayslip(payroll: PayrollDto): void {
    this.previewPayroll.set(payroll);
    this.showPayslip.set(true);
  }

  // ---- Approve all ----
  confirmApproveAll(): void {
    const targets = this.filtered().filter(p =>
      p.status === PayrollStatus.Calculated || p.status === PayrollStatus.Processing
    );
    this.confirmService.confirm({
      message: `Duyệt <strong>${targets.length}</strong> bản lương đang hiển thị?`,
      header: 'Xác nhận chốt bảng lương',
      icon: 'pi pi-check-circle',
      acceptLabel: 'Duyệt tất cả',
      rejectLabel: 'Hủy',
      accept: () => this.approveAll(targets),
    });
  }

  private approveAll(targets: PayrollDto[]): void {
    this.isProcessing.set(true);
    let done = 0;
    for (const p of targets) {
      this.payrollService.approve(p.id!).subscribe({
        next: (updated) => {
          this.payrolls.update(list => list.map(x => x.id === updated.id ? updated : x));
          this.applyFilter();
          done++;
          if (done === targets.length) {
            this.isProcessing.set(false);
            this.messageService.add({ severity: 'success', summary: 'Thành công', detail: `Đã duyệt ${done} bản lương` });
          }
        },
        error: () => {
          done++;
          if (done === targets.length) this.isProcessing.set(false);
        },
      });
    }
  }

  // ---- Per-row actions ----
  getMenuItems(p: PayrollDto): MenuItem[] {
    const items: MenuItem[] = [
      { label: 'Xem phiếu lương', icon: 'pi pi-file-pdf', command: () => this.openPayslip(p) },
      { separator: true },
    ];

    if (p.status === PayrollStatus.Draft || p.status === PayrollStatus.Processing) {
      items.push({ label: 'Submit', icon: 'pi pi-send', command: () => this.changeStatus(p, 'submit') });
    }
    if (p.status === PayrollStatus.Calculated) {
      items.push({ label: 'Duyệt', icon: 'pi pi-check', command: () => this.changeStatus(p, 'approve') });
    }
    if (p.status === PayrollStatus.Approved) {
      items.push({ label: 'Đánh dấu đã trả', icon: 'pi pi-wallet', command: () => this.changeStatus(p, 'paid') });
      items.push({ label: 'Khoá', icon: 'pi pi-lock', command: () => this.changeStatus(p, 'lock') });
    }

    return items;
  }

  changeStatus(p: PayrollDto, action: 'submit' | 'approve' | 'paid' | 'lock'): void {
    const call = action === 'submit'  ? this.payrollService.submit(p.id!)
               : action === 'approve' ? this.payrollService.approve(p.id!)
               : action === 'paid'    ? this.payrollService.markAsPaid(p.id!)
               :                        this.payrollService.lock(p.id!);

    call.subscribe({
      next: (updated) => {
        this.payrolls.update(list => list.map(x => x.id === updated.id ? updated : x));
        this.applyFilter();
        this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã cập nhật trạng thái' });
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Thao tác thất bại' }),
    });
  }

  // ---- Helpers ----
  getStatusLabel(status: PayrollStatus | undefined): string {
    const map: Record<number, string> = {
      1: 'Nháp', 2: 'Đang xử lý', 3: 'Đã tính', 4: 'Đã duyệt', 5: 'Đã trả',
    };
    return status != null ? (map[status] ?? '—') : '—';
  }

  getStatusSeverity(status: PayrollStatus | undefined): string {
    const map: Record<number, string> = {
      1: 'secondary', 2: 'warn', 3: 'info', 4: 'success', 5: 'success',
    };
    return status != null ? (map[status] ?? 'secondary') : 'secondary';
  }

  fmt(v: number | undefined): string {
    if (v == null) return '—';
    return new Intl.NumberFormat('vi-VN').format(v) + ' đ';
  }
}