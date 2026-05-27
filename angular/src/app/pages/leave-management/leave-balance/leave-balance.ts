import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder, FormGroup, Validators,
  ReactiveFormsModule, AbstractControl, ValidationErrors
} from '@angular/forms';

import { LeaveBalanceService } from 'src/app/proxy/application-services';
import {
  LeaveBalanceDto,
  CreateUpdateLeaveBalanceDto,
  AdjustLeaveBalanceDto,
  BulkInitializeLeaveBalanceDto,
  GetAllLeaveBalancesInput,
} from 'src/app/proxy/dtos/models';

// PrimeNG
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { DividerModule } from 'primeng/divider';
import { ProgressBarModule } from 'primeng/progressbar';
import { ConfirmationService, MessageService } from 'primeng/api';

// Giả sử có LeaveTypeService để lấy danh sách loại nghỉ
import { LeaveTypeService } from 'src/app/proxy/application-services';

@Component({
  selector: 'app-leave-balance',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    TableModule,
    DialogModule,
    SelectModule,
    InputTextModule,
    InputNumberModule,
    IconFieldModule,
    InputIconModule,
    TagModule,
    TooltipModule,
    ConfirmDialogModule,
    ToastModule,
    DividerModule,
    ProgressBarModule,
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './leave-balance.html',
  styleUrl: './leave-balance.scss',
})
export class LeaveBalanceComponent implements OnInit {
  private leaveBalanceService = inject(LeaveBalanceService);
  private leaveTypeService = inject(LeaveTypeService);
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  // ---- State ----
  balances = signal<LeaveBalanceDto[]>([]);
  leaveTypes = signal<{ id: number; name: string }[]>([]);
  isLoading = signal(false);
  isSubmitting = signal(false);
  totalCount = signal(0);
  first = 0;
  rows = 10;

  // Dialog visibility
  showFormDialog = signal(false);
  showAdjustDialog = signal(false);
  showBulkInitDialog = signal(false);
  isEditMode = signal(false);
  editingId = signal<number | null>(null);
  adjustingBalance = signal<LeaveBalanceDto | null>(null);

  // ---- Year options (5 năm gần nhất + năm tới) ----
  currentYear = new Date().getFullYear();
  yearOptions = Array.from({ length: 6 }, (_, i) => ({
    label: String(this.currentYear - 2 + i),
    value: this.currentYear - 2 + i,
  }));

  // ---- Filter form ----
  filterForm: FormGroup = this.fb.group({
    keyword: [''],
    leaveTypeId: [null],
    year: [this.currentYear],
  });

  // ---- Create/Update form ----
  balanceForm: FormGroup = this.fb.group({
    employeeId: [null, Validators.required],
    leaveTypeId: [null, Validators.required],
    year: [this.currentYear, [Validators.required, Validators.min(2000), Validators.max(2100)]],
    allocatedDays: [null, [Validators.min(0), Validators.max(365)]],
    carriedOverDays: [null, [Validators.min(0), Validators.max(365)]],
  });

  // ---- Adjust form ----
  adjustForm: FormGroup = this.fb.group({
    adjustmentDays: [null, [Validators.required, Validators.min(-365), Validators.max(365)]],
  });

  // ---- Bulk init form ----
  bulkInitForm: FormGroup = this.fb.group({
    leaveTypeId: [null, Validators.required],
    year: [this.currentYear, [Validators.required, Validators.min(2000), Validators.max(2100)]],
    defaultDays: [null, [Validators.min(0), Validators.max(365)]],
  });

  // ---- Error messages ----
  private readonly errorMessages: Record<string, Record<string, string>> = {
    employeeId: {
      required: 'Vui lòng chọn nhân viên',
    },
    leaveTypeId: {
      required: 'Vui lòng chọn loại nghỉ phép',
    },
    year: {
      required: 'Vui lòng nhập năm',
      min: 'Năm không hợp lệ (tối thiểu 2000)',
      max: 'Năm không hợp lệ (tối đa 2100)',
    },
    allocatedDays: {
      min: 'Số ngày không được âm',
      max: 'Số ngày không được vượt quá 365',
    },
    carriedOverDays: {
      min: 'Số ngày không được âm',
      max: 'Số ngày không được vượt quá 365',
    },
    adjustmentDays: {
      required: 'Vui lòng nhập số ngày điều chỉnh',
      min: 'Không được nhỏ hơn -365',
      max: 'Không được lớn hơn 365',
    },
    defaultDays: {
      min: 'Số ngày không được âm',
      max: 'Số ngày không được vượt quá 365',
    },
  };

  ngOnInit(): void {
    this.loadLeaveTypes();
    this.loadBalances();
  }

  // ---- Load data ----
  loadLeaveTypes(): void {
    this.leaveTypeService.getList({ maxResultCount: 100, skipCount: 0 }).subscribe({
      next: (res) => this.leaveTypes.set(res.items!.map(t => ({ id: t.id!, name: t.name! }))),
    });
  }

  loadBalances(event?: any): void {
    this.isLoading.set(true);
    const f = this.filterForm.value;
    const input: GetAllLeaveBalancesInput = {
      keyword: f.keyword || undefined,
      leaveTypeId: f.leaveTypeId || undefined,
      year: f.year || undefined,
      skipCount: event ? event.first : this.first,
      maxResultCount: event ? event.rows : this.rows,
    };
    this.leaveBalanceService.getList(input).subscribe({
      next: (res) => {
        this.balances.set(res.items!);
        this.totalCount.set(res.totalCount!);
        this.isLoading.set(false);
      },
      error: () => { this.isLoading.set(false); },
    });
  }

  onFilterChange(): void {
    this.first = 0;
    this.loadBalances();
  }

  resetFilter(): void {
    this.filterForm.reset({ keyword: '', leaveTypeId: null, year: this.currentYear });
    this.loadBalances();
  }

  // ---- CRUD ----
  openCreate(): void {
    this.isEditMode.set(false);
    this.editingId.set(null);
    this.balanceForm.reset({ year: this.currentYear });
    this.showFormDialog.set(true);
  }

  openEdit(balance: LeaveBalanceDto): void {
    this.isEditMode.set(true);
    this.editingId.set(balance.id!);
    this.balanceForm.patchValue({
      employeeId: balance.employeeId ?? null,
      leaveTypeId: balance.leaveTypeId ?? null,
      year: balance.year ?? this.currentYear,
      allocatedDays: balance.allocatedDays ?? null,
      carriedOverDays: balance.carriedOverDays ?? null,
    });
    this.showFormDialog.set(true);
  }

  closeFormDialog(): void {
    this.showFormDialog.set(false);
    this.balanceForm.reset();
  }

  save(): void {
    if (this.balanceForm.invalid) {
      this.balanceForm.markAllAsTouched();
      return;
    }
    const v = this.balanceForm.value;
    const payload: CreateUpdateLeaveBalanceDto = {
      employeeId: v.employeeId,
      leaveTypeId: v.leaveTypeId,
      year: v.year,
      allocatedDays: v.allocatedDays ?? undefined,
      carriedOverDays: v.carriedOverDays ?? undefined,
    };

    this.isSubmitting.set(true);

    if (this.isEditMode()) {
      this.leaveBalanceService.update(this.editingId()!, payload).subscribe({
        next: (result) => {
          this.balances.update(list => list.map(b => b.id === result.id ? result : b));
          this.toast('success', 'Đã cập nhật số dư nghỉ phép');
          this.isSubmitting.set(false);
          this.closeFormDialog();
        },
        error: () => { this.toastError(); this.isSubmitting.set(false); },
      });
    } else {
      this.leaveBalanceService.create(payload).subscribe({
        next: (result) => {
          this.balances.update(list => [result, ...list]);
          this.totalCount.update(n => n + 1);
          this.toast('success', 'Đã tạo số dư nghỉ phép');
          this.isSubmitting.set(false);
          this.closeFormDialog();
        },
        error: () => { this.toastError(); this.isSubmitting.set(false); },
      });
    }
  }

  confirmDelete(balance: LeaveBalanceDto): void {
    this.confirmationService.confirm({
      message: `Xóa số dư nghỉ phép <strong>${balance.leaveTypeName}</strong> năm ${balance.year} của <strong>${balance.employeeName}</strong>?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.leaveBalanceService.delete(balance.id!).subscribe({
          next: () => {
            this.balances.update(list => list.filter(b => b.id !== balance.id));
            this.totalCount.update(n => n - 1);
            this.toast('success', 'Đã xóa');
          },
          error: () => this.toastError(),
        });
      },
    });
  }

  // ---- Adjust balance ----
  openAdjust(balance: LeaveBalanceDto): void {
    this.adjustingBalance.set(balance);
    this.adjustForm.reset();
    this.showAdjustDialog.set(true);
  }

  closeAdjustDialog(): void {
    this.showAdjustDialog.set(false);
    this.adjustForm.reset();
    this.adjustingBalance.set(null);
  }

  submitAdjust(): void {
    if (this.adjustForm.invalid) {
      this.adjustForm.markAllAsTouched();
      return;
    }
    const payload: AdjustLeaveBalanceDto = {
      adjustmentDays: this.adjustForm.value.adjustmentDays,
    };
    this.isSubmitting.set(true);
    this.leaveBalanceService.adjustBalance(this.adjustingBalance()!.id!, payload).subscribe({
      next: () => {
        this.toast('success', 'Đã điều chỉnh số dư');
        this.isSubmitting.set(false);
        this.closeAdjustDialog();
        this.loadBalances();
      },
      error: () => { this.toastError(); this.isSubmitting.set(false); },
    });
  }

  // ---- Recalculate ----
  confirmRecalculate(balance: LeaveBalanceDto): void {
    this.confirmationService.confirm({
      message: `Tính lại số dư nghỉ phép <strong>${balance.leaveTypeName}</strong> của <strong>${balance.employeeName}</strong>?`,
      header: 'Xác nhận tính lại',
      icon: 'pi pi-refresh',
      acceptLabel: 'Tính lại',
      rejectLabel: 'Hủy',
      accept: () => {
        this.leaveBalanceService.recalculateBalance(balance.id!).subscribe({
          next: () => {
            this.toast('success', 'Đã tính lại số dư');
            this.loadBalances();
          },
          error: () => this.toastError(),
        });
      },
    });
  }

  // ---- Bulk Initialize ----
  openBulkInit(): void {
    this.bulkInitForm.reset({ year: this.currentYear });
    this.showBulkInitDialog.set(true);
  }

  closeBulkInitDialog(): void {
    this.showBulkInitDialog.set(false);
    this.bulkInitForm.reset();
  }

  submitBulkInit(): void {
    if (this.bulkInitForm.invalid) {
      this.bulkInitForm.markAllAsTouched();
      return;
    }
    const v = this.bulkInitForm.value;
    const payload: BulkInitializeLeaveBalanceDto = {
      leaveTypeId: v.leaveTypeId,
      year: v.year,
      defaultDays: v.defaultDays ?? undefined,
    };
    this.isSubmitting.set(true);
    this.leaveBalanceService.bulkInitializeYearly(payload).subscribe({
      next: () => {
        this.toast('success', 'Đã khởi tạo số dư hàng loạt');
        this.isSubmitting.set(false);
        this.closeBulkInitDialog();
        this.loadBalances();
      },
      error: () => { this.toastError(); this.isSubmitting.set(false); },
    });
  }

  // ---- Validation helpers ----
  isInvalid(form: FormGroup, field: string): boolean {
    const ctrl = form.get(field);
    return !!(ctrl && ctrl.invalid && (ctrl.touched || ctrl.dirty));
  }

  getError(form: FormGroup, field: string): string {
    const ctrl = form.get(field);
    if (!ctrl?.errors) return '';
    const firstKey = Object.keys(ctrl.errors)[0];
    return this.errorMessages[field]?.[firstKey] ?? 'Giá trị không hợp lệ';
  }

  // ---- Display helpers ----
  getUsedPercent(balance: LeaveBalanceDto): number {
    if (!balance.totalDays || balance.totalDays === 0) return 0;
    return Math.round(((balance.usedDays ?? 0) / balance.totalDays) * 100);
  }

  getProgressSeverity(percent: number): string {
    if (percent >= 90) return 'danger';
    if (percent >= 70) return 'warn';
    return 'success';
  }

  getLeaveTypeName(id: number | null): string {
    return this.leaveTypes().find(t => t.id === id)?.name ?? '—';
  }

  // ---- Toast shortcuts ----
  private toast(severity: string, detail: string): void {
    this.messageService.add({ severity, summary: severity === 'success' ? 'Thành công' : 'Lỗi', detail, life: 3000 });
  }

  private toastError(): void {
    this.toast('error', 'Thao tác thất bại, vui lòng thử lại');
  }
}