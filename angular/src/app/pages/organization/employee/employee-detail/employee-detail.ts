import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EmployeeService } from 'src/app/proxy/application-services';
import { EmployeeDto, CreateUpdateEmployeeDto } from 'src/app/proxy/dtos/models';
import { Gender, EmployeeStatus, ContractType } from 'src/app/proxy/enums';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { AvatarModule } from 'primeng/avatar';
import { TagModule } from 'primeng/tag';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { InputTextModule } from 'primeng/inputtext';
import { Contract } from './contract/contract';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    AvatarModule,
    TagModule,
    TableModule,
    DialogModule,
    SelectModule,
    DatePickerModule,
    ProgressSpinnerModule,
    MessageModule,
    InputTextModule,
    Contract
  ],
  templateUrl: './employee-detail.html',
  styleUrl: './employee-detail.scss',
})
export class EmployeeDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private employeeService = inject(EmployeeService);
  private fb = inject(FormBuilder);

  employee: EmployeeDto | null = null;
  isLoading = false;
  isSubmitting = false;
  employeeId!: number;

  // Inline edit
  isEditing = false;
  editForm: FormGroup = this.fb.group({
    fullName:        ['', Validators.required],
    email:           ['', [Validators.required, Validators.email]],
    phone:           [''],
    gender:          [null, Validators.required],
    dateOfBirth:     [null],
    nationalId:      [''],
    address:         [''],
    startDate:       [null, Validators.required],
    contractType:    [null, Validators.required],
    contractEndDate: [null],
  });

  // Modal states
  showChangeStatusModal = false;
  showOnboardModal = false;
  showOffboardModal = false;

  changeStatusForm: FormGroup = this.fb.group({
    status: [null, Validators.required],
  });

  onboardForm: FormGroup = this.fb.group({
    startDate: [null, Validators.required],
  });

  offboardForm: FormGroup = this.fb.group({
    terminationDate: [null, Validators.required],
  });

  readonly Gender = Gender;
  readonly EmployeeStatus = EmployeeStatus;
  readonly ContractType = ContractType;

  statusOptions = [
    { label: 'Đang làm việc', value: EmployeeStatus.Active },
    { label: 'Nghỉ phép',     value: EmployeeStatus.Onleave },
    { label: 'Đã nghỉ',       value: EmployeeStatus.Resigned },
    { label: 'Đã chấm dứt',   value: EmployeeStatus.Terminated },
  ];

  genderOptions = [
    { label: 'Nam',  value: Gender.Male },
    { label: 'Nữ',   value: Gender.Female },
    { label: 'Khác', value: Gender.Other },
  ];

  contractTypeOptions = [
    { label: 'Toàn thời gian', value: ContractType.Fulltime },
    { label: 'Bán thời gian',  value: ContractType.Partime },
    { label: 'Hợp đồng',       value: ContractType.Contract },
  ];

  ngOnInit(): void {
    this.employeeId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadEmployee();
  }

  loadEmployee(): void {
    this.isLoading = true;
    this.employeeService.get(this.employeeId).subscribe({
      next: (emp) => {
        this.employee = emp;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; },
    });
  }

  goBack(): void {
    this.router.navigate(['/organization/employee']);
  }

  // ---- Inline Edit ----
  startEdit(): void {
    if (!this.employee) return;
    this.editForm.patchValue({
      fullName:        this.employee.fullName        ?? '',
      email:           this.employee.email           ?? '',
      phone:           this.employee.phone           ?? '',
      gender:          this.employee.gender          ?? null,
      dateOfBirth:     this.parseDateOrNull(this.employee.dateOfBirth),
      nationalId:      this.employee.nationalId      ?? '',
      address:         this.employee.address         ?? '',
      startDate:       this.parseDateOrNull(this.employee.startDate),
      // contractType:    this.employee.contractType    ?? null,
      // contractEndDate: this.parseDateOrNull(this.employee.contractEndDate),
    });
    this.isEditing = true;
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editForm.reset();
  }

  submitEdit(): void {
    if (this.editForm.invalid || !this.employee) {
      this.editForm.markAllAsTouched();
      return;
    }

    const v = this.editForm.value;
    const payload: CreateUpdateEmployeeDto = {
      // Các field không edit — giữ nguyên từ employee hiện tại
      employeeCode: this.employee.employeeCode!,
      departmentId: this.employee.departmentId!,
      positionId:   this.employee.positionId!,
      managerId:    this.employee.managerId ?? null,
      status:       this.employee.status!,
      avatarUrl:    '', // Không edit avatar ở đây, để tránh bị reset khi update

      // Các field có thể edit
      fullName:        v.fullName,
      email:           v.email,
      phone:           v.phone || undefined,
      gender:          v.gender,
      dateOfBirth:     v.dateOfBirth     ? this.toIsoDateString(v.dateOfBirth)     : null,
      nationalId:      v.nationalId      || undefined,
      address:         v.address         || undefined,
      startDate:       this.toIsoDateString(v.startDate),
      contractType:    v.contractType,
      contractEndDate: v.contractEndDate ? this.toIsoDateString(v.contractEndDate) : null,
    };

    this.isSubmitting = true;
    this.employeeService.update(this.employeeId, payload).subscribe({
      next: (result) => {
        this.employee = result;
        this.isSubmitting = false;
        this.isEditing = false;
        this.editForm.reset();
      },
      error: () => { this.isSubmitting = false; },
    });
  }

  // ---- Change Status ----
  openChangeStatus(): void {
    this.changeStatusForm.patchValue({ status: this.employee?.status ?? null });
    this.showChangeStatusModal = true;
  }

  closeChangeStatus(): void {
    this.showChangeStatusModal = false;
    this.changeStatusForm.reset();
  }

  submitChangeStatus(): void {
    if (this.changeStatusForm.invalid || !this.employee) return;
    const { status } = this.changeStatusForm.value;

    this.isSubmitting = true;
    const payload = this.buildPayload({ status });
    this.employeeService.update(this.employeeId, payload).subscribe({
      next: (result) => {
        this.employee = result;
        this.isSubmitting = false;
        this.closeChangeStatus();
      },
      error: () => { this.isSubmitting = false; },
    });
  }

  // ---- Onboard ----
  openOnboard(): void {
    this.onboardForm.reset();
    this.showOnboardModal = true;
  }

  closeOnboard(): void {
    this.showOnboardModal = false;
    this.onboardForm.reset();
  }

  submitOnboard(): void {
    if (this.onboardForm.invalid || !this.employee) return;
    const startDate = this.toIsoDateString(this.onboardForm.value.startDate);

    this.isSubmitting = true;
    const payload = this.buildPayload({ startDate, status: EmployeeStatus.Active });
    this.employeeService.update(this.employeeId, payload).subscribe({
      next: (result) => {
        this.employee = result;
        this.isSubmitting = false;
        this.closeOnboard();
      },
      error: () => { this.isSubmitting = false; },
    });
  }

  // ---- Offboard ----
  openOffboard(): void {
    this.offboardForm.reset();
    this.showOffboardModal = true;
  }

  closeOffboard(): void {
    this.showOffboardModal = false;
    this.offboardForm.reset();
  }

  submitOffboard(): void {
    if (this.offboardForm.invalid) return;
    const terminationDate = this.toIsoDateString(this.offboardForm.value.terminationDate);

    this.isSubmitting = true;
    this.employeeService.offboard(this.employeeId, terminationDate).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.closeOffboard();
        this.loadEmployee();
      },
      error: () => { this.isSubmitting = false; },
    });
  }

  // ---- Helpers ----

  /**
   * Build payload cho update — merge employee hiện tại với override.
   * Dùng cho Change Status và Onboard để không phải lặp mapping.
   */
  private buildPayload(override: Partial<CreateUpdateEmployeeDto>): CreateUpdateEmployeeDto {
    const e = this.employee!;
    return {
      employeeCode:    e.employeeCode!,
      fullName:        e.fullName!,
      email:           e.email!,
      phone:           e.phone,
      gender:          e.gender!,
      dateOfBirth:     e.dateOfBirth ?? null,
      nationalId:      e.nationalId,
      address:         e.address,
      avatarUrl:       e.avatarUrl,
      departmentId:    e.departmentId!,
      positionId:      e.positionId!,
      managerId:       e.managerId ?? null,
      startDate:       e.startDate!,
      contractType:    e.contractType!,
      contractEndDate: e.contractEndDate ?? null,
      status:          e.status!,
      ...override,
    };
  }

  /** Parse ISO date string → Date object cho p-datepicker */
  private parseDateOrNull(value: string | null | undefined): Date | null {
    if (!value) return null;
    const d = new Date(value);
    return isNaN(d.getTime()) ? null : d;
  }

  /** Date object → 'YYYY-MM-DD' string cho API */
  private toIsoDateString(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  getGenderLabel(gender: Gender | undefined): string {
    switch (gender) {
      case Gender.Male:   return 'Nam';
      case Gender.Female: return 'Nữ';
      case Gender.Other:  return 'Khác';
      default:            return '—';
    }
  }

  getStatusLabel(status: EmployeeStatus | undefined): string {
    switch (status) {
      case EmployeeStatus.Active:     return 'Đang làm việc';
      case EmployeeStatus.Onleave:    return 'Nghỉ phép';
      case EmployeeStatus.Resigned:   return 'Đã nghỉ';
      case EmployeeStatus.Terminated: return 'Đã chấm dứt';
      default:                        return '—';
    }
  }

  getStatusSeverity(status: EmployeeStatus | undefined): 'success' | 'warn' | 'secondary' | 'danger' {
    switch (status) {
      case EmployeeStatus.Active:     return 'success';
      case EmployeeStatus.Onleave:    return 'warn';
      case EmployeeStatus.Resigned:   return 'secondary';
      case EmployeeStatus.Terminated: return 'danger';
      default:                        return 'secondary';
    }
  }

  getContractTypeLabel(type: ContractType | undefined): string {
    switch (type) {
      case ContractType.Fulltime: return 'Toàn thời gian';
      case ContractType.Partime:  return 'Bán thời gian';
      case ContractType.Contract: return 'Hợp đồng';
      default:                    return '—';
    }
  }

  canOnboard(): boolean {
    return !this.employee?.startDate || this.employee.status === EmployeeStatus.Resigned;
  }

  canOffboard(): boolean {
    return this.employee?.status === EmployeeStatus.Active
      || this.employee?.status === EmployeeStatus.Onleave;
  }
}