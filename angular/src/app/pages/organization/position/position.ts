import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PagedResultDto } from '@abp/ng.core';

// ABP Proxy Services
import { PositionService } from 'src/app/proxy/application-services/position.service';
import { DepartmentService } from 'src/app/proxy/application-services/department.service';
import { PositionDto, DepartmentDto, GetAllPositionsInput, CreateUpdatePositionDto } from 'src/app/proxy/dtos/models';

// PrimeNG v21 Modules (Sử dụng standalone components)
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { SelectModule } from 'primeng/select';
import { ToggleSwitchModule } from 'primeng/toggleswitch'; import { MenuModule } from 'primeng/menu';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { TagModule } from 'primeng/tag';

// PrimeNG Services
import { ConfirmationService, MessageService, MenuItem } from 'primeng/api';

@Component({
  selector: 'app-position',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, TableModule, ButtonModule, InputTextModule,
    DialogModule, ConfirmDialogModule, ToastModule, SelectModule,
    ToggleSwitchModule, MenuModule, IconFieldModule, InputIconModule, TagModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './position.html',
  styleUrl: './position.scss',
})
export class Position implements OnInit {
  private positionService = inject(PositionService);
  private departmentService = inject(DepartmentService);
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  // Data Signals (Tận dụng tính năng của Angular mới và PrimeNG v21)
  positions = signal<PositionDto[]>([]);
  departments = signal<DepartmentDto[]>([]);
  totalCount = signal(0);
  isLoading = signal(false);

  // Pagination & State
  rows = 10;
  showFormModal = false;
  isEditMode = false;
  isSubmitting = false;
  selectedPosition: PositionDto | null = null;

  // Forms
  filterForm: FormGroup = this.fb.group({
    keyword: [''],
    departmentId: [null],
    isActive: [null],
  });

  positionForm: FormGroup = this.fb.group({
    title: ['', Validators.required],
    level: [''],
    departmentId: [null, Validators.required],
    isActive: [true],
  });

  activeOptions = [
    { label: 'Tất cả', value: null },
    { label: 'Đang hoạt động', value: true },
    { label: 'Ngừng hoạt động', value: false },
  ];

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.departmentService.getList({ maxResultCount: 1000, skipCount: 0 }).subscribe({
      next: (result) => this.departments.set(result.items || []),
    });
  }

  // Lấy dữ liệu theo cơ chế Lazy của PrimeNG
  loadPositions(event?: TableLazyLoadEvent): void {
    this.isLoading.set(true);

    const input: GetAllPositionsInput = {
      ...this.filterForm.value,
      skipCount: event?.first ?? 0,
      maxResultCount: event?.rows ?? this.rows,
    };

    this.positionService.getList(input).subscribe({
      next: (result: PagedResultDto<PositionDto>) => {
        this.positions.set(result.items || []);
        this.totalCount.set(result.totalCount || 0);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  onFilter(): void {
    this.loadPositions();
  }

  resetFilter(): void {
    this.filterForm.reset({ keyword: '', departmentId: null, isActive: null });
    this.onFilter();
  }

  // Action Menu
  getActionMenu(pos: PositionDto): MenuItem[] {
    return [
      { label: 'Chỉnh sửa', icon: 'pi pi-pencil', command: () => this.openEdit(pos) },
      {
        label: 'Xóa',
        icon: 'pi pi-trash',
        styleClass: 'text-red-500',
        command: () => this.confirmDelete(pos)
      }
    ];
  }

  openCreate(): void {
    this.isEditMode = false;
    this.selectedPosition = null;
    this.positionForm.reset({ isActive: true });
    this.showFormModal = true;
  }

  openEdit(pos: PositionDto): void {
    this.isEditMode = true;
    this.selectedPosition = pos;
    this.positionForm.patchValue(pos);
    this.showFormModal = true;
  }

  submitForm(): void {
    if (this.positionForm.invalid) return;
    this.isSubmitting = true;

    const payload: CreateUpdatePositionDto = this.positionForm.value;
    const request$ = this.isEditMode && this.selectedPosition?.id
      ? this.positionService.update(this.selectedPosition.id, payload)
      : this.positionService.create(payload);

    request$.subscribe({
      next: () => {
        this.isSubmitting = false;
        this.showFormModal = false;
        this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Dữ liệu đã được lưu' });
        this.loadPositions();
      },
      error: () => this.isSubmitting = false
    });
  }

  confirmDelete(pos: PositionDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa vị trí <b>${pos.title}</b>?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      acceptButtonStyleClass: 'p-button-danger',
      rejectLabel: 'Hủy',
      accept: () => {
        this.positionService.delete(pos.id!).subscribe(() => {
          this.messageService.add({ severity: 'success', summary: 'Đã xóa', detail: 'Vị trí đã được loại bỏ' });
          this.loadPositions();
        });
      }
    });
  }
}