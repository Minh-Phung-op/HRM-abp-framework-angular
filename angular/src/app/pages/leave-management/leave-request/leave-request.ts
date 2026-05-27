import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { PagedResultDto } from '@abp/ng.core';
import { LeaveRequestService } from 'src/app/proxy/application-services';
import {
  LeaveRequestDto,
  GetAllLeaveRequestsInput,
  CreateLeaveRequestDto,
} from 'src/app/proxy/dtos/models';
import { LeaveRequestStatus } from 'src/app/proxy/enums';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { PaginatorModule } from 'primeng/paginator';
import { TooltipModule } from 'primeng/tooltip';
import { SkeletonModule } from 'primeng/skeleton';
import { TextareaModule } from 'primeng/textarea';

// Validator: endDate >= startDate
function dateRangeValidator(group: AbstractControl): ValidationErrors | null {
  const start = group.get('startDate')?.value;
  const end = group.get('endDate')?.value;
  if (start && end && new Date(end) < new Date(start)) {
    return { dateRange: true };
  }
  return null;
}

@Component({
  selector: 'app-leave-request',
  templateUrl: './leave-request.html',
  styleUrls: ['./leave-request.scss'],
  providers: [MessageService],
  imports: [
    CommonModule, 
    FormsModule, 
    ReactiveFormsModule, 
    TableModule, 
    ToastModule, 
    DialogModule, 
    SelectModule, 
    DatePickerModule, 
    PaginatorModule, 
    TooltipModule, 
    SkeletonModule, 
    TextareaModule
  ],
})
export class LeaveRequest implements OnInit {
  private leaveRequestService = inject(LeaveRequestService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);

  // ---- Tab state ----
  activeTab: 'mine' | 'pending' = 'mine';

  // ---- My requests ----
  myRequests: LeaveRequestDto[] = [];
  myTotalCount = 0;
  myPageIndex = 0;
  myPageSize = 10;

  // ---- Pending approvals ----
  pendingRequests: LeaveRequestDto[] = [];
  pendingTotalCount = 0;

  isLoading = false;
  isSubmitting = false;

  // ---- Create modal ----
  showCreateModal = false;

  // ---- Leave types (mock — replace with real service) ----
  leaveTypes = [
    { id: 1, name: 'Nghỉ phép năm' },
    { id: 2, name: 'Nghỉ ốm' },
    { id: 3, name: 'Nghỉ thai sản' },
    { id: 4, name: 'Nghỉ không lương' },
    { id: 5, name: 'Nghỉ công việc cá nhân' },
  ];

  // ---- Create form ----
  createForm: FormGroup = this.fb.group({
    leaveTypeId: [null, Validators.required],
    startDate: [null, Validators.required],
    endDate: [null, Validators.required],
    reason: [''],
  }, { validators: dateRangeValidator });

  // ---- Filter form ----
  filterForm: FormGroup = this.fb.group({
    status: [null],
    month: [null],
    year: [null],
  });

  statusOptions = [
    { label: 'Tất cả trạng thái', value: null },
    { label: 'Chờ duyệt', value: LeaveRequestStatus.Pending },
    { label: 'Đã duyệt', value: LeaveRequestStatus.Approved },
    { label: 'Từ chối', value: LeaveRequestStatus.Rejected },
    { label: 'Đã hủy', value: LeaveRequestStatus.Cancelled },
  ];

  monthOptions = Array.from({ length: 12 }, (_, i) => ({
    label: `Tháng ${i + 1}`,
    value: i + 1,
  }));

  yearOptions = Array.from({ length: 5 }, (_, i) => {
    const y = new Date().getFullYear() - 2 + i;
    return { label: `${y}`, value: y };
  });

  readonly LeaveRequestStatus = LeaveRequestStatus;

  ngOnInit(): void {
    this.loadMyRequests();
    this.loadPendingApprovals();

    this.filterForm.valueChanges.subscribe(() => {
      this.myPageIndex = 0;
      this.loadMyRequests();
    });
  }

  // ---- Data loading ----
  loadMyRequests(): void {
    this.isLoading = true;
    const fv = this.filterForm.value;
    const input: GetAllLeaveRequestsInput = {
      status: fv.status,
      month: fv.month,
      year: fv.year,
      skipCount: this.myPageIndex * this.myPageSize,
      maxResultCount: this.myPageSize,
    };

    this.leaveRequestService.getList(input).subscribe({
      next: (result: PagedResultDto<LeaveRequestDto>) => {
        this.myRequests = result.items || [];
        this.myTotalCount = result.totalCount || 0;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; },
    });
  }

  loadPendingApprovals(): void {
    this.leaveRequestService.getPendingApprovals().subscribe({
      next: (result) => {
        this.pendingRequests = result.items || [];
        this.pendingTotalCount = result.totalCount || 0;
      },
    });
  }

  // ---- Tabs ----
  switchTab(tab: 'mine' | 'pending'): void {
    this.activeTab = tab;
  }

  // ---- Create modal ----
  openCreate(): void {
    this.createForm.reset();
    this.showCreateModal = true;
  }

  closeCreate(): void {
    this.showCreateModal = false;
    this.createForm.reset();
  }

  submitCreate(): void {
    this.createForm.markAllAsTouched();
    if (this.createForm.invalid) return;

    const fv = this.createForm.value;
    const payload: CreateLeaveRequestDto = {
      leaveTypeId: fv.leaveTypeId,
      startDate: this.toDateString(fv.startDate),
      endDate: this.toDateString(fv.endDate),
      reason: fv.reason,
    };

    this.isSubmitting = true;
    this.leaveRequestService.create(payload).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.closeCreate();
        this.loadMyRequests();
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đơn nghỉ phép đã được tạo',
        });
      },
      error: () => {
        this.isSubmitting = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tạo đơn. Vui lòng thử lại.',
        });
      },
    });
  }

  // ---- Navigation ----
  goToDetail(id: number): void {
    this.router.navigate(['leave-management', 'leave-request', 'detail', id]);
  }

  // ---- Pagination ----
  onMyPageChange(event: { page: number; rows: number }): void {
    this.myPageIndex = event.page;
    this.myPageSize = event.rows;
    this.loadMyRequests();
  }

  // ---- Computed ----
  get calculatedDays(): number {
    const start = this.createForm.get('startDate')?.value;
    const end = this.createForm.get('endDate')?.value;
    if (!start || !end) return 0;
    const diff = new Date(end).getTime() - new Date(start).getTime();
    return diff >= 0 ? Math.ceil(diff / (1000 * 60 * 60 * 24)) + 1 : 0;
  }

  get hasDateRangeError(): boolean {
    return !!this.createForm.errors?.['dateRange'] &&
      !!this.createForm.get('startDate')?.value &&
      !!this.createForm.get('endDate')?.value;
  }

  // ---- Helpers ----
  private toDateString(date: Date | string): string {
    if (!date) return '';
    const d = date instanceof Date ? date : new Date(date);
    return d.toISOString().split('T')[0];
  }

  getStatusLabel(status: LeaveRequestStatus | undefined): string {
    switch (status) {
      case LeaveRequestStatus.Pending: return 'Chờ duyệt';
      case LeaveRequestStatus.Approved: return 'Đã duyệt';
      case LeaveRequestStatus.Rejected: return 'Từ chối';
      case LeaveRequestStatus.Cancelled: return 'Đã hủy';
      default: return '—';
    }
  }

  getStatusSeverity(status: LeaveRequestStatus | undefined): string {
    switch (status) {
      case LeaveRequestStatus.Pending: return 'warning';
      case LeaveRequestStatus.Approved: return 'success';
      case LeaveRequestStatus.Rejected: return 'danger';
      case LeaveRequestStatus.Cancelled: return 'secondary';
      default: return 'info';
    }
  }

  getStatusClass(status: LeaveRequestStatus | undefined): string {
    switch (status) {
      case LeaveRequestStatus.Pending: return 'status-pending';
      case LeaveRequestStatus.Approved: return 'status-approved';
      case LeaveRequestStatus.Rejected: return 'status-rejected';
      case LeaveRequestStatus.Cancelled: return 'status-cancelled';
      default: return '';
    }
  }

  resetFilter(): void {
    this.filterForm.reset({ status: null, month: null, year: null });
  }

  isFieldInvalid(controlName: string): boolean {
    const ctrl = this.createForm.get(controlName);
    return !!(ctrl?.invalid && ctrl?.touched);
  }
}