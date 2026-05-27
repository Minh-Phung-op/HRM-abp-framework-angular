import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PagedResultDto } from '@abp/ng.core';
import { DepartmentService } from 'src/app/proxy/application-services/department.service';
import { DepartmentDto, GetAllDepartmentsInput, CreateUpdateDepartmentDto } from 'src/app/proxy/dtos/models';

// Import PrimeNG Services & Modules
import { ConfirmationService, MessageService, MenuItem } from 'primeng/api';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MenuModule } from 'primeng/menu';
import { SelectModule } from 'primeng/select';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
// PrimeNG v21
import { MessageModule } from 'primeng/message'; // Để hiện câu thông báo lỗi

@Component({
  selector: 'app-department',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule, ButtonModule, InputTextModule,
    DialogModule, ConfirmDialogModule, MenuModule,
    SelectModule, ToggleSwitchModule, MessageModule
  ],
  providers: [ConfirmationService, MessageService], // Cần thiết cho các dịch vụ popup của Prime
  templateUrl: './department.html',
  styleUrl: './department.scss',
})
export class Department implements OnInit {
  private departmentService = inject(DepartmentService);
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  // Data
  departments: DepartmentDto[] = [];
  totalCount = 0;
  isLoading = false;
  title = 'phòng ban';
  // Pagination (PrimeNG tự quản lý qua event nhưng ta vẫn giữ state)
  rows = 10;
  first = 0;

  // Modal states
  showFormModal = false;
  isEditMode = false;
  isSubmitting = false;
  selectedDepartment: DepartmentDto | null = null;

  // Forms
  filterForm: FormGroup = this.fb.group({
    keyword: [''],
    isActive: [null],
  });

  departmentForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    code: ['', Validators.required],
    parentId: [null],
    managerId: [null],
    isActive: [true],
  });

  activeOptions = [
    { label: 'Tất cả', value: null },
    { label: 'Đang hoạt động', value: true },
    { label: 'Ngừng hoạt động', value: false },
  ];

  ngOnInit(): void {
    // Không cần load trực tiếp ở đây nếu dùng lazy load của p-table
  }

  // ---- Logic Load Data (Lazy Load) ----
  loadDepartments(event?: any): void {
    this.isLoading = true;

    // Cập nhật skipCount dựa trên event của p-table
    const skipCount = event ? event.first : 0;
    const maxResultCount = event ? event.rows : this.rows;

    const input: GetAllDepartmentsInput = {
      ...this.filterForm.value,
      skipCount: skipCount,
      maxResultCount: maxResultCount,
    };

    this.departmentService.getList(input).subscribe({
      next: (result: PagedResultDto<DepartmentDto>) => {
        this.departments = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; },
    });
  }

  // ---- Filter Logic ----
  onFilter(): void {
    this.first = 0; // Reset về trang đầu
    this.loadDepartments();
  }

  resetFilter(): void {
    this.filterForm.reset({ keyword: '', isActive: null });
    this.onFilter();
  }

  // ---- Action Menu cho từng dòng ----
  getMenuItems(dept: DepartmentDto): MenuItem[] {
    return [
      {
        label: 'Chỉnh sửa',
        icon: 'pi pi-pencil',
        command: () => this.openEdit(dept)
      },
      {
        label: 'Xóa',
        icon: 'pi pi-trash',
        styleClass: 'text-red-500',
        command: () => this.confirmDelete(dept)
      }
    ];
  }

  // ---- Create/Edit ----
  openCreate(): void {
    this.isEditMode = false;
    this.selectedDepartment = null;
    this.departmentForm.reset({ isActive: true });
    this.showFormModal = true;
  }

  openEdit(dept: DepartmentDto): void {
    this.isEditMode = true;
    this.selectedDepartment = dept;
    this.departmentForm.patchValue(dept);
    this.showFormModal = true;
  }

  submitForm(): void {
    // if (this.departmentForm.invalid) return;
    if (this.departmentForm.invalid) {
      this.departmentForm.markAllAsTouched(); // Ép hiện đỏ khi nhấn submit
      return;
    }
    this.isSubmitting = true;

    const payload: CreateUpdateDepartmentDto = this.departmentForm.value;
    const request$ = this.isEditMode && this.selectedDepartment?.id
      ? this.departmentService.update(this.selectedDepartment.id, payload)
      : this.departmentService.create(payload);

    request$.subscribe({
      next: () => {
        this.isSubmitting = false;
        this.showFormModal = false;
        this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Dữ liệu đã được cập nhật' });
        this.loadDepartments();
      },
      error: () => { this.isSubmitting = false; },
    });
  }

  // ---- Delete (Dùng ConfirmationService) ----
  confirmDelete(dept: DepartmentDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa phòng ban <b>${dept.name}</b> không?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.departmentService.delete(dept.id!).subscribe(() => {
          this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã xóa phòng ban' });
          this.loadDepartments();
        });
      }
    });
  }
}