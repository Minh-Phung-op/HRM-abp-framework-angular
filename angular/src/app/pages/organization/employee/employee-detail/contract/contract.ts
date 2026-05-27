import { Component, OnInit, inject, signal, computed, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';

import { ContractService } from 'src/app/proxy/application-services';
import {
  ContractDto,
  CreateContractDto,
  UpdateContractDto,
  GetAllContractsInput,
} from 'src/app/proxy/dtos/models';
import { ContractType, ContractStatus } from 'src/app/proxy/enums';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DividerModule } from 'primeng/divider';
// import { EmptyModule } from 'primeng/empty';

@Component({
  selector: 'app-contract',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    TagModule,
    DialogModule,
    SelectModule,
    DatePickerModule,
    InputTextModule,
    InputNumberModule,
    ProgressSpinnerModule,
    TooltipModule,
    ConfirmDialogModule,
    ToastModule,
    DividerModule,
    // EmptyModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './contract.html',
  styleUrl: './contract.scss',
})
export class Contract implements OnInit {
  @Input() employeeId!: number;

  private contractService = inject(ContractService);
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  contracts = signal<ContractDto[]>([]);
  isLoading = signal(false);
  isSubmitting = signal(false);
  showFormDialog = signal(false);
  isEditMode = signal(false);
  editingId = signal<number | null>(null);

  // ---- Options ----
  contractTypeOptions = [
    { label: 'Toàn thời gian', value: ContractType.Fulltime },
    { label: 'Bán thời gian',  value: ContractType.Partime  },
    { label: 'Hợp đồng',       value: ContractType.Contract },
  ];

  contractStatusOptions = [
    { label: 'Đang hiệu lực', value: ContractStatus.Active     },
    { label: 'Hết hạn',       value: ContractStatus.Expired    },
    { label: 'Đã chấm dứt',   value: ContractStatus.Terminated },
  ];

  // ---- Form ----
  contractForm: FormGroup = this.fb.group(
    {
      contractNumber: ['', [Validators.required, Validators.maxLength(50)]],
      contractType:   [null, Validators.required],
      signDate:       [null, Validators.required],
      startDate:      [null, Validators.required],
      endDate:        [null],
      basicSalary:    [null, [Validators.min(0)]],
      insuranceSalary:[null, [Validators.min(0)]],
      status:         [null, Validators.required],
    },
    { validators: this.dateRangeValidator }
  );

  // ---- Error messages ----
  private readonly errorMessages: Record<string, Record<string, string>> = {
    contractNumber: {
      required:  'Số hợp đồng không được để trống',
      maxlength: 'Số hợp đồng không được vượt quá 50 ký tự',
    },
    contractType: {
      required: 'Vui lòng chọn loại hợp đồng',
    },
    signDate: {
      required: 'Vui lòng chọn ngày ký',
    },
    startDate: {
      required:   'Vui lòng chọn ngày bắt đầu',
      dateRange:  'Ngày bắt đầu phải trước ngày kết thúc',
    },
    endDate: {
      dateRange: 'Ngày kết thúc phải sau ngày bắt đầu',
    },
    basicSalary: {
      min: 'Lương cơ bản không được âm',
    },
    insuranceSalary: {
      min: 'Lương bảo hiểm không được âm',
    },
    status: {
      required: 'Vui lòng chọn trạng thái hợp đồng',
    },
  };

  ngOnInit(): void {
    this.loadContracts();
  }

  loadContracts(): void {
    this.isLoading.set(true);
    const input: GetAllContractsInput = {
      employeeId: this.employeeId,
      maxResultCount: 100,
      skipCount: 0,
    };
    this.contractService.getList(input).subscribe({
      next: (result) => {
        this.contracts.set(result.items!);
        this.isLoading.set(false);
      },
      error: () => { this.isLoading.set(false); },
    });
  }

  openCreate(): void {
    this.isEditMode.set(false);
    this.editingId.set(null);
    this.contractForm.reset();
    this.showFormDialog.set(true);
  }

  openEdit(contract: ContractDto): void {
    this.isEditMode.set(true);
    this.editingId.set(contract.id!);
    this.contractForm.patchValue({
      contractNumber:  contract.contractNumber  ?? '',
      contractType:    contract.contractType    ?? null,
      signDate:        this.parseDateOrNull(contract.signDate),
      startDate:       this.parseDateOrNull(contract.startDate),
      endDate:         this.parseDateOrNull(contract.endDate),
      basicSalary:     contract.basicSalary     ?? null,
      insuranceSalary: contract.insuranceSalary ?? null,
      status:          contract.status          ?? null,
    });
    this.showFormDialog.set(true);
  }

  closeDialog(): void {
    this.showFormDialog.set(false);
    this.contractForm.reset();
  }

  save(): void {
    if (this.contractForm.invalid) {
      this.contractForm.markAllAsTouched();
      return;
    }

    const v = this.contractForm.value;
    const base = {
      contractNumber:  v.contractNumber,
      contractType:    v.contractType,
      signDate:        this.toIsoDateString(v.signDate),
      startDate:       this.toIsoDateString(v.startDate),
      endDate:         v.endDate ? this.toIsoDateString(v.endDate) : null,
      basicSalary:     v.basicSalary     ?? undefined,
      insuranceSalary: v.insuranceSalary ?? undefined,
      status:          v.status,
    };

    this.isSubmitting.set(true);

    if (this.isEditMode()) {
      const payload: UpdateContractDto = base;
      this.contractService.update(this.editingId()!, payload).subscribe({
        next: (updated) => {
          this.contracts.update(list =>
            list.map(c => c.id === updated.id ? updated : c)
          );
          this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã cập nhật hợp đồng' });
          this.isSubmitting.set(false);
          this.closeDialog();
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Cập nhật thất bại' });
          this.isSubmitting.set(false);
        },
      });
    } else {
      const payload: CreateContractDto = { employeeId: this.employeeId, ...base };
      this.contractService.create(payload).subscribe({
        next: (created) => {
          this.contracts.update(list => [created, ...list]);
          this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã thêm hợp đồng' });
          this.isSubmitting.set(false);
          this.closeDialog();
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Thêm hợp đồng thất bại' });
          this.isSubmitting.set(false);
        },
      });
    }
  }

  confirmDelete(contract: ContractDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc muốn xóa hợp đồng <strong>${contract.contractNumber}</strong>?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.contractService.delete(contract.id!).subscribe({
          next: () => {
            this.contracts.update(list => list.filter(c => c.id !== contract.id));
            this.messageService.add({ severity: 'success', summary: 'Đã xóa', detail: 'Hợp đồng đã được xóa' });
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Xóa thất bại' });
          },
        });
      },
    });
  }

  // ---- Validation helpers ----
  isInvalid(field: string): boolean {
    const ctrl = this.contractForm.get(field);
    return !!(ctrl && ctrl.invalid && (ctrl.touched || ctrl.dirty));
  }

  getError(field: string): string {
    const ctrl = this.contractForm.get(field);
    if (!ctrl) return '';

    // Form-level errors (dateRange) mapped to fields
    if (field === 'startDate' && this.contractForm.errors?.['dateRange'] && (ctrl.touched || ctrl.dirty)) {
      return this.errorMessages['startDate']['dateRange'];
    }
    if (field === 'endDate' && this.contractForm.errors?.['dateRange'] && (ctrl.touched || ctrl.dirty)) {
      return this.errorMessages['endDate']['dateRange'];
    }

    if (!ctrl.errors) return '';
    const firstKey = Object.keys(ctrl.errors)[0];
    return this.errorMessages[field]?.[firstKey] ?? 'Giá trị không hợp lệ';
  }

  isFieldOrFormInvalid(field: string): boolean {
    const ctrl = this.contractForm.get(field);
    if (!ctrl) return false;
    const hasCtrlError = ctrl.invalid && (ctrl.touched || ctrl.dirty);
    const hasFormError = !!this.contractForm.errors?.['dateRange'] && (ctrl.touched || ctrl.dirty);
    return hasCtrlError || hasFormError;
  }

  // ---- Display helpers ----
  getContractTypeLabel(type: ContractType | undefined): string {
    return this.contractTypeOptions.find(o => o.value === type)?.label ?? '—';
  }

  getStatusLabel(status: ContractStatus | undefined): string {
    return this.contractStatusOptions.find(o => o.value === status)?.label ?? '—';
  }

  getStatusSeverity(status: ContractStatus | undefined): 'success' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case ContractStatus.Active:     return 'success';
      case ContractStatus.Expired:    return 'warn';
      case ContractStatus.Terminated: return 'danger';
      default:                        return 'secondary';
    }
  }

  getContractTypeIcon(type: ContractType | undefined): string {
    switch (type) {
      case ContractType.Fulltime: return 'pi pi-briefcase';
      case ContractType.Partime:  return 'pi pi-clock';
      case ContractType.Contract: return 'pi pi-file';
      default:                    return 'pi pi-file';
    }
  }

  formatCurrency(value: number | undefined): string {
    if (value == null) return '—';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }

  formatDate(value: string | null | undefined): string {
    if (!value) return '—';
    const d = new Date(value);
    return isNaN(d.getTime()) ? '—' : d.toLocaleDateString('vi-VN');
  }

  // ---- Private utils ----
  private dateRangeValidator(group: AbstractControl): ValidationErrors | null {
    const start = group.get('startDate')?.value;
    const end   = group.get('endDate')?.value;
    if (!start || !end) return null;
    return new Date(start) >= new Date(end) ? { dateRange: true } : null;
  }

  private parseDateOrNull(value: string | null | undefined): Date | null {
    if (!value) return null;
    const d = new Date(value);
    return isNaN(d.getTime()) ? null : d;
  }

  private toIsoDateString(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}