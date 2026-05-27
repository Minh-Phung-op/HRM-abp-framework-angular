import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService, ConfirmationService, MenuItem } from 'primeng/api';
import { PagedResultDto } from '@abp/ng.core';
import { LeaveTypeService } from 'src/app/proxy/application-services';
import {
  LeaveTypeDto,
  GetAllLeaveTypesInput,
  CreateUpdateLeaveTypeDto,
} from 'src/app/proxy/dtos/models';

import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { ToastModule }          from 'primeng/toast';
import { ConfirmDialogModule }  from 'primeng/confirmdialog';
import { DialogModule }         from 'primeng/dialog';
import { InputTextModule }      from 'primeng/inputtext';
import { InputNumberModule }    from 'primeng/inputnumber';
import { ToggleSwitchModule }   from 'primeng/toggleswitch';  // p-toggleswitch
import { SelectButtonModule }   from 'primeng/selectbutton';
import { PaginatorModule }      from 'primeng/paginator';
import { ButtonModule }         from 'primeng/button';
import { BadgeModule }          from 'primeng/badge';
import { TagModule }            from 'primeng/tag';
import { SkeletonModule }       from 'primeng/skeleton';
import { IconFieldModule }      from 'primeng/iconfield';
import { InputIconModule }      from 'primeng/inputicon';
import { TableModule }          from 'primeng/table';
import { MenuModule }           from 'primeng/menu';          // p-menu

@Component({
  selector: 'app-leave-type',
  templateUrl: './leave-type.html',
  styleUrls: ['./leave-type.scss'],
  standalone: true,
  providers: [MessageService, ConfirmationService],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    ToastModule,
    ConfirmDialogModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    ToggleSwitchModule,
    SelectButtonModule,
    PaginatorModule,
    ButtonModule,
    BadgeModule,
    TagModule,
    SkeletonModule,
    IconFieldModule,
    InputIconModule,
    TableModule,
    MenuModule,
  ],
})
export class LeaveTypeComponent implements OnInit {
  private leaveTypeService = inject(LeaveTypeService);
  private fb               = inject(FormBuilder);
  private messageService   = inject(MessageService);
  private confirmService   = inject(ConfirmationService);

  leaveTypes: LeaveTypeDto[] = [];
  totalCount   = 0;
  isLoading    = false;
  isSubmitting = false;

  pageIndex = 0;
  pageSize  = 10;

  showFormModal     = false;
  isEditMode        = false;
  selectedLeaveType: LeaveTypeDto | null = null;

  // ---- Filter form ----
  filterForm: FormGroup = this.fb.group({
    keyword:   [''],
    paid:      [null],
    carryOver: [null],
  });

  // ---- Create / Edit form ----
  leaveTypeForm: FormGroup = this.fb.group({
    name:               ['',   [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    code:               ['',   [Validators.required, Validators.pattern(/^[A-Z0-9_]+$/)]],
    defaultDaysPerYear: [null, [Validators.min(0), Validators.max(365)]],
    paid:               [true],
    carryOver:          [false],
    maxCarryDays:       [null, [Validators.min(0)]],
  });

  triStateOptions = [
    { label: 'Tất cả', value: null  },
    { label: 'Có',     value: true  },
    { label: 'Không',  value: false },
  ];

  // ---- Lifecycle ----
  ngOnInit(): void {
    this.loadLeaveTypes();

    // selectbutton filter auto-reload (paid, carryOver)
    this.filterForm.get('paid')?.valueChanges.subscribe(() => this.onFilterChange());
    this.filterForm.get('carryOver')?.valueChanges.subscribe(() => this.onFilterChange());

    // Auto-uppercase code
    this.leaveTypeForm.get('code')?.valueChanges.subscribe(v => {
      if (v && v !== v.toUpperCase()) {
        this.leaveTypeForm.get('code')?.setValue(v.toUpperCase(), { emitEvent: false });
      }
    });

    // maxCarryDays required only when carryOver = true
    this.leaveTypeForm.get('carryOver')?.valueChanges.subscribe(enabled => {
      const ctrl = this.leaveTypeForm.get('maxCarryDays');
      if (enabled) {
        ctrl?.setValidators([Validators.required, Validators.min(0)]);
      } else {
        ctrl?.clearValidators();
        ctrl?.setValue(null);
      }
      ctrl?.updateValueAndValidity();
    });
  }

  // ---- Filter ----
  /**
   * Gọi khi: keyword keyup.enter, paid/carryOver selectbutton thay đổi.
   * Reset về trang 1 rồi load lại.
   */
  onFilterChange(): void {
    this.pageIndex = 0;
    this.loadLeaveTypes();
  }

  resetFilter(): void {
    this.filterForm.reset({ keyword: '', paid: null, carryOver: null });
    this.onFilterChange();
  }

  // ---- Load ----
  loadLeaveTypes(): void {
    this.isLoading = true;
    const fv = this.filterForm.value;
    const input: GetAllLeaveTypesInput = {
      keyword:        fv.keyword || undefined,
      paid:           fv.paid,
      carryOver:      fv.carryOver,
      skipCount:      this.pageIndex * this.pageSize,
      maxResultCount: this.pageSize,
    };

    this.leaveTypeService.getList(input).subscribe({
      next: (result: PagedResultDto<LeaveTypeDto>) => {
        this.leaveTypes = result.items      || [];
        this.totalCount = result.totalCount || 0;
        this.isLoading  = false;
      },
      error: () => { this.isLoading = false; },
    });
  }

  // ---- p-menu items per row ----
  getMenuItems(lt: LeaveTypeDto): MenuItem[] {
    return [
      {
        label:   'Chỉnh sửa',
        icon:    'pi pi-pencil',
        command: () => this.openEdit(lt),
      },
      { separator: true },
      {
        label:      'Xóa',
        icon:       'pi pi-trash',
        styleClass: 'danger-menu-item',
        command:    () => this.confirmDelete(lt),
      },
    ];
  }

  // ---- Create ----
  openCreate(): void {
    this.isEditMode        = false;
    this.selectedLeaveType = null;
    this.leaveTypeForm.reset({ paid: true, carryOver: false });
    this.showFormModal = true;
  }

  // ---- Edit ----
  openEdit(lt: LeaveTypeDto): void {
    this.isEditMode        = true;
    this.selectedLeaveType = lt;
    this.leaveTypeForm.patchValue({
      name:               lt.name,
      code:               lt.code,
      defaultDaysPerYear: lt.defaultDaysPerYear ?? null,
      paid:               lt.paid      ?? true,
      carryOver:          lt.carryOver ?? false,
      maxCarryDays:       lt.maxCarryDays ?? null,
    });
    this.showFormModal = true;
  }

  closeFormModal(): void {
    this.showFormModal = false;
    this.leaveTypeForm.reset();
  }

  // ---- Submit ----
  submitForm(): void {
    this.leaveTypeForm.markAllAsTouched();
    if (this.leaveTypeForm.invalid) return;

    const fv = this.leaveTypeForm.value;
    const payload: CreateUpdateLeaveTypeDto = {
      ...fv,
      maxCarryDays: fv.carryOver ? fv.maxCarryDays : null,
    };

    this.isSubmitting = true;
    const req$ = this.isEditMode && this.selectedLeaveType?.id
      ? this.leaveTypeService.update(this.selectedLeaveType.id, payload)
      : this.leaveTypeService.create(payload);

    req$.subscribe({
      next: () => {
        this.isSubmitting = false;
        this.closeFormModal();
        this.loadLeaveTypes();
        this.messageService.add({
          severity: 'success',
          summary:  'Thành công',
          detail:   this.isEditMode ? 'Đã cập nhật loại phép' : 'Đã tạo loại phép mới',
        });
      },
      error: () => {
        this.isSubmitting = false;
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể lưu. Vui lòng thử lại.' });
      },
    });
  }

  // ---- Delete ----
  confirmDelete(lt: LeaveTypeDto): void {
    this.confirmService.confirm({
      message:                `Xóa loại phép <strong>${lt.name}</strong>? Hành động này không thể hoàn tác.`,
      header:                 'Xác nhận xóa',
      acceptLabel:            'Xóa',
      rejectLabel:            'Hủy',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.leaveTypeService.delete(lt.id!).subscribe({
          next: () => {
            this.loadLeaveTypes();
            this.messageService.add({ severity: 'success', summary: 'Đã xóa', detail: 'Xóa loại phép thành công' });
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể xóa loại phép này' });
          },
        });
      },
    });
  }

  // ---- Pagination ----
  onPageChange(event: { page: number; rows: number }): void {
    this.pageIndex = event.page;
    this.pageSize  = event.rows;
    this.loadLeaveTypes();
  }

  // ---- Validation helpers ----
  isFieldInvalid(field: string): boolean {
    const ctrl = this.leaveTypeForm.get(field);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  getFieldError(field: string): string {
    const ctrl = this.leaveTypeForm.get(field);
    if (!ctrl?.errors || !ctrl.touched) return '';
    if (ctrl.errors['required'])  return 'Trường này là bắt buộc';
    if (ctrl.errors['minlength']) return `Tối thiểu ${ctrl.errors['minlength'].requiredLength} ký tự`;
    if (ctrl.errors['maxlength']) return `Tối đa ${ctrl.errors['maxlength'].requiredLength} ký tự`;
    if (ctrl.errors['pattern'])   return 'Chỉ dùng chữ in hoa, số và dấu _';
    if (ctrl.errors['min'])       return `Giá trị phải ≥ ${ctrl.errors['min'].min}`;
    if (ctrl.errors['max'])       return `Giá trị phải ≤ ${ctrl.errors['max'].max}`;
    return 'Giá trị không hợp lệ';
  }

  // ---- Getters ----
  get carryOverEnabled(): boolean {
    return !!this.leaveTypeForm.get('carryOver')?.value;
  }
}