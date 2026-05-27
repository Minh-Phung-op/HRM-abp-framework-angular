import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PasswordModule } from 'primeng/password';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { CreateUpdateEmployeeDto, DepartmentDto, PositionDto } from 'src/app/proxy/dtos/models';
import { Validators } from '@angular/forms';

// ABP Proxies
import { DepartmentService, EmployeeService, PositionService } from 'src/app/proxy/application-services';
import { EmployeeDto, GetAllEmployeesInput } from 'src/app/proxy/dtos/models';
import { Gender, EmployeeStatus, ContractType } from 'src/app/proxy/enums';

// PrimeNG v21 Modules
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select'; // Thay thế Dropdown
import { DatePickerModule } from 'primeng/datepicker'; // Thay thế Calendar
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { TagModule } from 'primeng/tag';
import { MenuModule } from 'primeng/menu';
import { AvatarModule } from 'primeng/avatar';
import { MenuItem } from 'primeng/api';
import { FileUploadService } from 'src/app/services/fileUploadService';

@Component({
  selector: 'app-employee',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PasswordModule,
    TableModule, ButtonModule, InputTextModule,
    SelectModule, DatePickerModule, IconFieldModule,
    InputIconModule, TagModule, MenuModule, AvatarModule,
    DialogModule, ToastModule, ButtonModule, ReactiveFormsModule, CommonModule
  ],
  templateUrl: './employee.html',
  styleUrl: './employee.scss',
  providers: [MessageService] // Cần thiết để hiển thị thông báo
})
export class Employee implements OnInit {
  private employeeService = inject(EmployeeService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);
  private positionService = inject(PositionService);
  private departmentService = inject(DepartmentService);
  private fileUploadService = inject(FileUploadService);

  // State cho Modal Tài khoản
  showAccountModal = signal(false);
  roleOptions = signal<string[]>([]); // Danh sách Role từ hệ thống

  // State Management với Signals
  employees = signal<EmployeeDto[]>([]);
  totalCount = signal(0);
  isLoading = signal(false);

  // State cho Modal
  showCreateModal = signal(false);
  isSubmitting = signal(false);
  isEditMode = signal(false);
  selectedEmployeeId = signal<number | null>(null);

  // Data cho các Selectors
  departmentOptions = signal<DepartmentDto[]>([]);
  positionOptions = signal<PositionDto[]>([]);
  managerOptions = signal<EmployeeDto[]>([]);

  // Signal để quản lý preview ảnh và file đang chờ upload
  avatarPreview = signal<string | null>(null);
  selectedFile = signal<File | null>(null);

  // Pagination
  rows = 10;
  first = 0;

  filterForm: FormGroup = this.fb.group({
    keyword: [''],
    departmentId: [null],
    positionId: [null],
    managerId: [null],
    contractType: [null],
    status: [null],
    gender: [null],
    startDateFrom: [null],
    startDateTo: [null],
  });

  // Form Group
  employeeForm: FormGroup = this.fb.group({
    employeeCode: ['', [Validators.required, Validators.pattern(/^[A-Z]{3}[0-9]{6}$/)]],
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.pattern(/^(\+84|0)[3-9]\d{8}$/)]],
    dateOfBirth: [null, this.pastDateValidator],
    gender: [Gender.Male, [Validators.required]],
    nationalId: ['', [Validators.pattern(/^\d{9}(\d{3})?$/)]],
    address: [''],
    avatarUrl: [''], // Để trống theo yêu cầu
    departmentId: [null, [Validators.required]],
    positionId: [null, [Validators.required]],
    managerId: [null],
    startDate: [new Date(), [Validators.required]],
    contractType: [ContractType.Fulltime, [Validators.required]],
    contractEndDate: [null],
    status: [EmployeeStatus.Active, [Validators.required]],
  });

  accountForm = this.fb.group({
    employeeId: [null as number | null],
    userName: [{ value: '', disabled: true }], // Khóa username
    password: ['Abp@123456', [Validators.required, Validators.minLength(6)]],
    roleName: [null, [Validators.required]]
  });

  // Options
  genderOptions = [
    { label: 'Tất cả', value: null },
    { label: 'Nam', value: Gender.Male },
    { label: 'Nữ', value: Gender.Female },
    { label: 'Khác', value: Gender.Other },
  ];

  statusOptions = [
    { label: 'Tất cả', value: null },
    { label: 'Đang làm việc', value: EmployeeStatus.Active },
    { label: 'Nghỉ phép', value: EmployeeStatus.Onleave },
    { label: 'Đã nghỉ', value: EmployeeStatus.Resigned },
    { label: 'Đã chấm dứt', value: EmployeeStatus.Terminated },
  ];

  contractTypeOptions = [
    { label: 'Tất cả', value: null },
    { label: 'Toàn thời gian', value: ContractType.Fulltime },
    { label: 'Bán thời gian', value: ContractType.Partime },
    { label: 'Hợp đồng', value: ContractType.Contract },
  ];

  ngOnInit(): void {
    // Lazy load sẽ tự gọi lần đầu qua event của p-table
    this.loadDepartmentOptions();
  }

  loadDepartmentOptions() {
    this.departmentService.getList({ maxResultCount: 1000, isActive: true }).subscribe(res => {
      this.departmentOptions.set(res.items || []);
    });
  }

  // 1. Lấy danh sách Roles (Trừ admin)
  loadRoles() {
    // Gọi API lấy Role của ABP Identity
    this.employeeService.getAssignableRoles().subscribe(res => {
      this.roleOptions.set(res.filter(r => r.toLowerCase() !== 'admin')); // Loại bỏ role Admin nếu có
    });
  }

  // Khi chọn file từ máy tính
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      // Kiểm tra định dạng và kích thước (VD: < 2MB)
      if (file.size > 2 * 1024 * 1024) {
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Ảnh không được vượt quá 2MB' });
        return;
      }

      this.selectedFile.set(file);

      // Tạo preview để hiển thị lên giao diện ngay lập tức
      const reader = new FileReader();
      reader.onload = () => {
        this.avatarPreview.set(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  // Xóa ảnh đang chọn
  removeAvatar() {
    this.selectedFile.set(null);
    this.avatarPreview.set(null);
    this.employeeForm.get('avatarUrl')?.setValue('');
    // Reset input file để có thể chọn lại cùng 1 file vừa xóa
    const fileInput = document.getElementById('avatarUpload') as HTMLInputElement;
    if (fileInput) fileInput.value = '';
  }

  // 2. Khi thay đổi phòng ban -> Load Position và đề xuất Manager cùng phòng
  onDepartmentChange(deptId: number) {
    this.employeeForm.get('positionId')?.setValue(null); // Reset position cũ
    this.employeeForm.get('managerId')?.setValue(null);  // Reset manager cũ

    if (!deptId) {
      this.positionOptions.set([]);
      this.managerOptions.set([]);
      return;
    }

    // Load Position theo DepartmentId
    this.positionService.getList({ departmentId: deptId, isActive: true, maxResultCount: 100 }).subscribe(res => {
      this.positionOptions.set(res.items || []);
    });

    // Load đề xuất Manager (Nhân viên cùng phòng)
    this.loadManagerSuggestions('', deptId);
  }

  // 3. Tìm kiếm Manager (Dùng cho cả đề xuất và tìm kiếm manual)
  loadManagerSuggestions(keyword: string = '', deptId?: number) {
    this.employeeService.getList({
      keyword: keyword,
      departmentId: deptId,
      maxResultCount: 20
    }).subscribe(res => {
      this.managerOptions.set(res.items || []);
    });
  }

  // ---- Validator ngày phải là quá khứ ----
  private pastDateValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    const selected = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return selected >= today ? { pastDate: true } : null;
  }

  // ---- Error message map ----
  private readonly errorMessages: Record<string, Record<string, string>> = {
    employeeCode: {
      required: 'Mã nhân viên không được để trống',
      pattern: 'Mã nhân viên chỉ được chứa chữ cái, số và dấu gạch ngang',
    },
    fullName: {
      required: 'Họ và tên không được để trống',
      minlength: 'Họ và tên phải có ít nhất 2 ký tự',
      maxlength: 'Họ và tên không được vượt quá 100 ký tự',
    },
    email: {
      required: 'Email không được để trống',
      email: 'Email không đúng định dạng',
    },
    phone: {
      pattern: 'Số điện thoại không hợp lệ (VD: 0912345678)',
    },
    dateOfBirth: {
      pastDate: 'Ngày sinh phải là ngày trong quá khứ',
    },
    gender: {
      required: 'Vui lòng chọn giới tính',
    },
    departmentId: {
      required: 'Vui lòng chọn phòng ban',
    },
    positionId: {
      required: 'Vui lòng chọn chức vụ',
    },
    startDate: {
      required: 'Vui lòng chọn ngày bắt đầu làm việc',
    },
    contractType: {
      required: 'Vui lòng chọn loại hợp đồng',
    },
    nationalId: {
      pattern: 'CMND/CCCD phải có 9 hoặc 12 chữ số',
    },
    status: {
      required: 'Vui lòng chọn trạng thái',
    },
  };

  // ---- Helper: field có lỗi và đã touched/dirty? ----
  isInvalid(field: string): boolean {
    const ctrl = this.employeeForm.get(field);
    return !!(ctrl && ctrl.invalid && (ctrl.touched || ctrl.dirty));
  }

  // ---- Helper: lấy message lỗi đầu tiên của field ----
  getError(field: string): string {
    const ctrl = this.employeeForm.get(field);
    if (!ctrl || !ctrl.errors) return '';
    const firstKey = Object.keys(ctrl.errors)[0];
    return this.errorMessages[field]?.[firstKey] ?? 'Giá trị không hợp lệ';
  }

  // Mở modal
  openCreateModal() {
    this.isEditMode.set(false);
    this.selectedEmployeeId.set(null);
    this.resetAvatarState();
    this.employeeForm.reset({
      gender: Gender.Male,
      status: EmployeeStatus.Active,
      contractType: ContractType.Fulltime,
      startDate: new Date(),
      avatarUrl: ''
    });
    this.showCreateModal.set(true);
  }

  openEdit(emp: EmployeeDto) {
    this.isEditMode.set(true);
    this.selectedEmployeeId.set(emp.id!);
    this.resetAvatarState();
    this.avatarPreview.set(emp.avatarUrl || null);
    // Load Position theo department của emp trước khi patch value
    if (emp.departmentId) {
      this.positionService.getList({ departmentId: emp.departmentId, isActive: true, maxResultCount: 100 }).subscribe(res => {
        this.positionOptions.set(res.items || []);

        // Patch giá trị vào form
        this.employeeForm.patchValue({
          ...emp,
          dateOfBirth: emp.dateOfBirth ? new Date(emp.dateOfBirth) : null,
          startDate: emp.startDate ? new Date(emp.startDate) : new Date(),
          contractEndDate: emp.contractEndDate ? new Date(emp.contractEndDate) : null,
        });
        this.loadManagerSuggestions('', emp.departmentId);
      });
    }
    this.showCreateModal.set(true);
  }

  // --- Chức năng Tạo tài khoản ---
  // 2. Mở Modal tạo tài khoản
  openAccountModal(emp: EmployeeDto) {
    this.accountForm.reset({
      employeeId: emp.id,
      userName: emp.email, // Email làm Username
      password: 'Abp@123456',
      roleName: null
    });

    if (this.roleOptions().length === 0) {
      this.loadRoles();
    }
    this.showAccountModal.set(true);
  }
  // 2 thực hiện tạo tài khoản luôn để tránh 2 lần gọi API
  // confirmCreateAccount(emp: EmployeeDto) {
  //   if (this.accountForm.invalid) return;
  //   // Bạn có thể dùng ConfirmationService của PrimeNG để hỏi trước
  //   const defaultPassword = 'Password123@'; // Hoặc mở 1 prompt nhập mật khẩu
  //   this.employeeService.createAccountForEmployee(emp.id!, emp.email!, defaultPassword).subscribe({
  //     next: () => {
  //       this.messageService.add({ severity: 'success', summary: 'Thành công', detail: `Đã tạo tài khoản cho ${emp.fullName}` });
  //     },
  //     error: (err) => this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể tạo tài khoản' })
  //   });
  // }
  confirmCreateAccount() {
    if (this.accountForm.invalid) return;

    const { employeeId, password, roleName } = this.accountForm.getRawValue();
    const email = this.accountForm.get('userName')?.value; // Lấy từ field bị disabled

    this.isSubmitting.set(true);
    this.employeeService.createAccountForEmployee(employeeId!, email!, password!, roleName!).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã cấp tài khoản hệ thống' });
        this.showAccountModal.set(false);
        this.loadEmployees(); // Reload để cập nhật trạng thái userId trên bảng
        this.isSubmitting.set(false);
      },
      error: () => this.isSubmitting.set(false)
    });
  }

  // Thêm hàm helper này vào class hoặc một file util
  private formatDateOnly(date: any): string | null {
    if (!date) return null;
    const d = new Date(date);
    const month = '' + (d.getMonth() + 1);
    const day = '' + d.getDate();
    const year = d.getFullYear();

    return [year, month.padStart(2, '0'), day.padStart(2, '0')].join('-');
  }

  async saveEmployee() {
    if (this.employeeForm.invalid) {
      this.employeeForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const rawValue = this.employeeForm.value;

    try {
      let finalAvatarUrl = rawValue.avatarUrl;

      // Nếu có file mới được chọn -> Upload lên Firebase trước
      if (this.selectedFile()) {
        finalAvatarUrl = await this.fileUploadService.uploadAvatar(
          this.selectedFile()!,
          rawValue.employeeCode || 'TEMP'
        );
      }

      const payload: CreateUpdateEmployeeDto = {
        ...rawValue,
        dateOfBirth: this.formatDateOnly(rawValue.dateOfBirth),
        startDate: this.formatDateOnly(rawValue.startDate)!,
        contractEndDate: this.formatDateOnly(rawValue.contractEndDate),
        avatarUrl: finalAvatarUrl // Gán URL từ Firebase hoặc URL cũ
      };

      const request = this.isEditMode()
        ? this.employeeService.update(this.selectedEmployeeId()!, payload)
        : this.employeeService.create(payload);

      request.subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã lưu thông tin' });
          this.showCreateModal.set(false);
          this.loadEmployees();
          this.resetAvatarState(); // Xóa sạch state ảnh sau khi xong
          this.isSubmitting.set(false);
        },
        error: () => this.isSubmitting.set(false)
      });

    } catch (error) {
      this.messageService.add({ severity: 'error', summary: 'Lỗi upload', detail: 'Không thể tải ảnh lên Firebase' });
      this.isSubmitting.set(false);
    }
  }

  // Reset state ảnh khi đóng modal hoặc lưu xong
  private resetAvatarState() {
    this.selectedFile.set(null);
    this.avatarPreview.set(null);
  }

  loadEmployees(event?: TableLazyLoadEvent): void {
    this.isLoading.set(true);

    const input: GetAllEmployeesInput = {
      ...this.filterForm.value,
      skipCount: event?.first ?? 0,
      maxResultCount: event?.rows ?? this.rows,
    };

    this.employeeService.getList(input).subscribe({
      next: (result) => {
        this.employees.set(result.items || []);
        this.totalCount.set(result.totalCount || 0);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onFilterChange(): void {
    this.first = 0; // Reset về trang 1 khi lọc
    this.loadEmployees();
  }

  resetFilter(): void {
    this.filterForm.reset();
    this.onFilterChange();
  }

  // Action Menu items
  getMenuItems(emp: EmployeeDto): MenuItem[] {
    return [
      { label: 'Xem chi tiết', icon: 'pi pi-eye', command: () => this.goToDetail(emp.id!) },
      { label: 'Chỉnh sửa', icon: 'pi pi-pencil', command: () => this.openEdit(emp) },
      {
        label: 'Tạo tài khoản',
        icon: 'pi pi-key',
        visible: !emp.userId,
        command: () => this.openAccountModal(emp)
      },
      { separator: true },
      { label: 'Offboard', icon: 'pi pi-user-minus', styleClass: 'text-red-500' }
    ];
  }

  goToDetail(id: number): void {
    this.router.navigate(['/organization/employee/detail', id]);
  }

  // Helpers cho hiển thị Tag
  getStatusSeverity(status: EmployeeStatus | undefined): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" {
    switch (status) {
      case EmployeeStatus.Active: return 'success';
      case EmployeeStatus.Onleave: return 'warn';
      case EmployeeStatus.Resigned: return 'secondary';
      case EmployeeStatus.Terminated: return 'danger';
      default: return 'secondary';
    }
  }

  getStatusLabel(status: EmployeeStatus | undefined): string {
    return this.statusOptions.find(o => o.value === status)?.label || '—';
  }

  getGenderLabel(gender: Gender | undefined): string {
    return this.genderOptions.find(o => o.value === gender)?.label || '—';
  }
}