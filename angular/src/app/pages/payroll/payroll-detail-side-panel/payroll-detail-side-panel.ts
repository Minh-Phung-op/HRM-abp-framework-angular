import { Component, Input, Output, EventEmitter, OnChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';

import { PayrollService } from 'src/app/proxy/application-services';
import { PayrollDto, UpdatePayrollDto, CreateUpdatePayrollItemDto } from 'src/app/proxy/dtos/models';
import { PayrollItemType, PayrollStatus } from 'src/app/proxy/enums';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-payroll-detail-side-panel',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    ButtonModule, InputTextModule, InputNumberModule,
    SelectModule, TagModule, DividerModule, ToastModule, TooltipModule,
  ],
  providers: [MessageService],
  templateUrl: './payroll-detail-side-panel.html',
  styleUrl: './payroll-detail-side-panel.scss',
})
export class PayrollDetailSidePanelComponent implements OnChanges {
  @Input() payroll!: PayrollDto;
  @Output() close = new EventEmitter<void>();
  @Output() updated = new EventEmitter<PayrollDto>();
  @Output() previewPayslip = new EventEmitter<PayrollDto>();

  private payrollService = inject(PayrollService);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);

  isSaving = false;

  itemTypeOptions = [
    { label: 'Thưởng', value: PayrollItemType.Bonus },
    { label: 'Khấu trừ', value: PayrollItemType.Deduction },
    { label: 'Phụ cấp', value: PayrollItemType.Allowance },
    { label: 'Tạm ứng', value: PayrollItemType.Advance },
  ];

  editForm: FormGroup = this.fb.group({
    baseSalary: [null, [Validators.min(0)]],
    items: this.fb.array([]),
  });

  get itemsArray(): FormArray {
    return this.editForm.get('items') as FormArray;
  }

  readonly PayrollStatus = PayrollStatus;

  ngOnChanges(): void {
    if (!this.payroll) return;
    this.editForm.patchValue({ baseSalary: this.payroll.baseSalary ?? null });
    this.itemsArray.clear();
    (this.payroll.items ?? []).forEach(item => this.addItemRow(item));
  }

  addItemRow(item?: any): void {
    this.itemsArray.push(this.fb.group({
      type: [item?.type ?? PayrollItemType.Bonus, Validators.required],
      label: [item?.label ?? '', [Validators.required, Validators.maxLength(100)]],
      amount: [item?.amount ?? null, [Validators.required, Validators.min(0)]],
      note: [item?.note ?? ''],
    }));
  }

  removeItem(index: number): void {
    this.itemsArray.removeAt(index);
  }

  isItemInvalid(index: number, field: string): boolean {
    const ctrl = this.itemsArray.at(index).get(field);
    return !!(ctrl?.invalid && (ctrl.touched || ctrl.dirty));
  }

  save(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    const v = this.editForm.value;
    const payload: UpdatePayrollDto = {
      baseSalary: v.baseSalary ?? undefined,
      items: v.items as CreateUpdatePayrollItemDto[],
    };
    this.isSaving = true;
    this.payrollService.update(this.payroll.id!, payload).subscribe({
      next: (result) => {
        this.updated.emit(result);
        this.isSaving = false;
        this.messageService.add({ severity: 'success', summary: 'Đã lưu', detail: 'Cập nhật bảng lương thành công' });
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Lưu thất bại' });
        this.isSaving = false;
      },
    });
  }

  getItemTypeSeverity(type: PayrollItemType): string {
    return type === PayrollItemType.Bonus || type === PayrollItemType.Allowance
      ? 'success' : 'danger';
  }

  getItemTypeLabel(type: PayrollItemType): string {
    return this.itemTypeOptions.find(o => o.value === type)?.label ?? '—';
  }

  canEdit(): boolean {
    return this.payroll?.status !== PayrollStatus.Paid &&
      this.payroll?.status !== PayrollStatus.Approved;
  }

  fmt(v: number | undefined): string {
    if (v == null) return '—';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(v);
  }
}