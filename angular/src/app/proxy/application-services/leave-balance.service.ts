import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { AdjustLeaveBalanceDto, BulkInitializeLeaveBalanceDto, CreateUpdateLeaveBalanceDto, GetAllLeaveBalancesInput, LeaveBalanceDto } from '../dtos/models';

@Injectable({
  providedIn: 'root',
})
export class LeaveBalanceService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  adjustBalance = (id: number, input: AdjustLeaveBalanceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/leave-balance/${id}/adjust-balance`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  bulkInitializeYearly = (input: BulkInitializeLeaveBalanceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/leave-balance/bulk-initialize-yearly',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateLeaveBalanceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveBalanceDto>({
      method: 'POST',
      url: '/api/app/leave-balance',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/leave-balance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveBalanceDto>({
      method: 'GET',
      url: `/api/app/leave-balance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAllLeaveBalancesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LeaveBalanceDto>>({
      method: 'GET',
      url: '/api/app/leave-balance',
      params: { keyword: input.keyword, employeeId: input.employeeId, departmentId: input.departmentId, leaveTypeId: input.leaveTypeId, year: input.year, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getMyCurrentBalances = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveBalanceDto[]>({
      method: 'GET',
      url: '/api/app/leave-balance/my-current-balances',
    },
    { apiName: this.apiName,...config });
  

  recalculateBalance = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/leave-balance/${id}/recalculate-balance`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: number, input: CreateUpdateLeaveBalanceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveBalanceDto>({
      method: 'PUT',
      url: `/api/app/leave-balance/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}