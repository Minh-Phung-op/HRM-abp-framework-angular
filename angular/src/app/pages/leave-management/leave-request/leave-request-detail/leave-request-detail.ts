import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { EmployeeService, LeaveRequestService } from 'src/app/proxy/application-services';
import {
  LeaveRequestDto,
  LeaveRequestApprovalLogDto,
  CreateLeaveRequestApprovalLogDto,
  EmployeeDto, DepartmentDto, PositionDto,
} from 'src/app/proxy/dtos/models';
import {CommonModule} from '@angular/common';
import {
  LeaveRequestStatus,
  ApprovalAction,
  ApprovalStep,
} from 'src/app/proxy/enums';
import { ReactiveFormsModule } from '@angular/forms';

import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-leave-request-detail',
  templateUrl: './leave-request-detail.html',
  styleUrls: ['./leave-request-detail.scss'],
  providers: [MessageService, ConfirmationService],
  imports: [CommonModule, ReactiveFormsModule, ToastModule, ConfirmDialogModule, DialogModule, TextareaModule],
})
export class LeaveRequestDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private leaveRequestService = inject(LeaveRequestService);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private employeeService = inject(EmployeeService);

  request: LeaveRequestDto | null = null;
  employeeInfo: EmployeeDto | null = null;
  approvalHistory: LeaveRequestApprovalLogDto[] = [];
  isLoading = false;
  isSubmitting = false;
  requestId!: number;

  // Approve / reject dialog
  showApprovalDialog = false;
  approvalAction: 'approve' | 'reject' = 'approve';

  approvalForm: FormGroup = this.fb.group({
    comment: [''],
  });

  // Reject requires comment
  get commentRequired(): boolean { return this.approvalAction === 'reject'; }

  readonly LeaveRequestStatus = LeaveRequestStatus;
  readonly ApprovalAction = ApprovalAction;
  readonly ApprovalStep = ApprovalStep;

  ngOnInit(): void {
    this.requestId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadRequest();
    this.loadHistory();
  }

  loadEmployeeInfo(): void{
    if (this.request?.employeeId) {
      this.employeeService.get(this.request.employeeId).subscribe({
        next: (employee) => {
          this.employeeInfo = employee;
          console.log('Employee info loaded:', employee);
        }
      });
    }
  }

  loadRequest(): void {
    this.isLoading = true;
    this.leaveRequestService.get(this.requestId).subscribe({
      next: (req) => {
        this.request = req;
        this.isLoading = false;
        this.loadEmployeeInfo();
      },
      error: () => { this.isLoading = false; },
    });
  }

  loadHistory(): void {
    this.leaveRequestService.getApprovalHistory(this.requestId).subscribe({
      next: (logs) => { this.approvalHistory = logs; },
    });
  }

  goBack(): void {
    this.router.navigate(['/leave-requests']);
  }

  // ---- Approve ----
  openApprove(): void {
    this.approvalAction = 'approve';
    this.approvalForm.reset();
    this.approvalForm.get('comment')?.clearValidators();
    this.approvalForm.get('comment')?.updateValueAndValidity();
    this.showApprovalDialog = true;
  }

  // ---- Reject ----
  openReject(): void {
    this.approvalAction = 'reject';
    this.approvalForm.reset();
    this.approvalForm.get('comment')?.setValidators(Validators.required);
    this.approvalForm.get('comment')?.updateValueAndValidity();
    this.showApprovalDialog = true;
  }

  closeApprovalDialog(): void {
    this.showApprovalDialog = false;
    this.approvalForm.reset();
  }

  submitApproval(): void {
    this.approvalForm.markAllAsTouched();
    if (this.approvalForm.invalid) return;

    const payload: CreateLeaveRequestApprovalLogDto = {
      actionStep: ApprovalStep.HR,
      action: this.approvalAction === 'approve' ? ApprovalAction.Approve : ApprovalAction.Reject,
      comment: this.approvalForm.value.comment,
    };

    this.isSubmitting = true;
    const request$ = this.approvalAction === 'approve'
      ? this.leaveRequestService.approve(this.requestId, payload)
      : this.leaveRequestService.reject(this.requestId, payload);

    request$.subscribe({
      next: (result) => {
        this.request = result;
        this.isSubmitting = false;
        this.closeApprovalDialog();
        this.loadHistory();
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: this.approvalAction === 'approve' ? 'Đã duyệt đơn' : 'Đã từ chối đơn',
        });
      },
      error: () => {
        this.isSubmitting = false;
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể xử lý yêu cầu' });
      },
    });
  }

  // ---- Cancel ----
  confirmCancel(): void {
    this.confirmationService.confirm({
      message: 'Bạn có chắc muốn hủy đơn nghỉ phép này?',
      header: 'Xác nhận hủy đơn',
      icon: 'fas fa-exclamation-triangle',
      acceptLabel: 'Hủy đơn',
      rejectLabel: 'Quay lại',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.isSubmitting = true;
        this.leaveRequestService.cancel(this.requestId).subscribe({
          next: (result) => {
            this.request = result;
            this.isSubmitting = false;
            this.loadHistory();
            this.messageService.add({ severity: 'info', summary: 'Đã hủy', detail: 'Đơn nghỉ phép đã được hủy' });
          },
          error: () => { this.isSubmitting = false; },
        });
      },
    });
  }

  // ---- Helpers ----
  getStatusLabel(status: LeaveRequestStatus | undefined): string {
    switch (status) {
      case LeaveRequestStatus.Pending: return 'Chờ duyệt';
      case LeaveRequestStatus.Approved: return 'Đã duyệt';
      case LeaveRequestStatus.Rejected: return 'Từ chối';
      case LeaveRequestStatus.Cancelled: return 'Đã hủy';
      default: return '—';
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

  getActionLabel(action: ApprovalAction | undefined): string {
    switch (action) {
      case ApprovalAction.Approve: return 'Đã duyệt';
      case ApprovalAction.Reject: return 'Từ chối';
      case ApprovalAction.Cancel: return 'Đã hủy';
      default: return '—';
    }
  }

  getActionClass(action: ApprovalAction | undefined): string {
    switch (action) {
      case ApprovalAction.Approve: return 'action-approved';
      case ApprovalAction.Reject: return 'action-rejected';
      case ApprovalAction.Cancel: return 'action-cancelled';
      default: return '';
    }
  }

  getStepLabel(step: ApprovalStep | undefined): string {
    switch (step) {
      case ApprovalStep.TeamLead: return 'Team Lead';
      case ApprovalStep.HR: return 'HR';
      default: return '—';
    }
  }

  canApproveOrReject(): boolean {
    return this.request?.status === LeaveRequestStatus.Pending;
  }

  canCancel(): boolean {
    return this.request?.status === LeaveRequestStatus.Pending;
  }

  isCommentInvalid(): boolean {
    const ctrl = this.approvalForm.get('comment');
    return !!(ctrl?.invalid && ctrl?.touched);
  }
}