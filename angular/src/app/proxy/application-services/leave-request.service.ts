import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { CreateLeaveRequestApprovalLogDto, CreateLeaveRequestDto, GetAllLeaveRequestsInput, LeaveRequestApprovalLogDto, LeaveRequestDto, UpdateLeaveRequestDto } from '../dtos/models';

@Injectable({
  providedIn: 'root',
})
export class LeaveRequestService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  approve = (id: number, input: CreateLeaveRequestApprovalLogDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveRequestDto>({
      method: 'POST',
      url: `/api/app/leave-request/${id}/approve`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  cancel = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveRequestDto>({
      method: 'POST',
      url: `/api/app/leave-request/${id}/cancel`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateLeaveRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveRequestDto>({
      method: 'POST',
      url: '/api/app/leave-request',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveRequestDto>({
      method: 'GET',
      url: `/api/app/leave-request/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getApprovalHistory = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveRequestApprovalLogDto[]>({
      method: 'GET',
      url: `/api/app/leave-request/${id}/approval-history`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAllLeaveRequestsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LeaveRequestDto>>({
      method: 'GET',
      url: '/api/app/leave-request',
      params: { keyword: input.keyword, employeeId: input.employeeId, departmentId: input.departmentId, leaveTypeId: input.leaveTypeId, status: input.status, month: input.month, year: input.year, startDateFrom: input.startDateFrom, startDateTo: input.startDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getPendingApprovals = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LeaveRequestDto>>({
      method: 'GET',
      url: '/api/app/leave-request/pending-approvals',
    },
    { apiName: this.apiName,...config });
  

  reject = (id: number, input: CreateLeaveRequestApprovalLogDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveRequestDto>({
      method: 'POST',
      url: `/api/app/leave-request/${id}/reject`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  update = (id: number, input: UpdateLeaveRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveRequestDto>({
      method: 'PUT',
      url: `/api/app/leave-request/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}