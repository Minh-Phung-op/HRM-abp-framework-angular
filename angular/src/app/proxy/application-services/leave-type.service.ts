import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { CreateUpdateLeaveTypeDto, GetAllLeaveTypesInput, LeaveTypeDto } from '../dtos/models';

@Injectable({
  providedIn: 'root',
})
export class LeaveTypeService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateLeaveTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveTypeDto>({
      method: 'POST',
      url: '/api/app/leave-type',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/leave-type/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveTypeDto>({
      method: 'GET',
      url: `/api/app/leave-type/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAllLeaveTypesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LeaveTypeDto>>({
      method: 'GET',
      url: '/api/app/leave-type',
      params: { keyword: input.keyword, paid: input.paid, carryOver: input.carryOver, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: number, input: CreateUpdateLeaveTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveTypeDto>({
      method: 'PUT',
      url: `/api/app/leave-type/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}