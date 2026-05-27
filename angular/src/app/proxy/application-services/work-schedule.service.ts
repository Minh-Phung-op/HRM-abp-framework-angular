import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { CreateUpdateWorkScheduleDto, GetAllWorkSchedulesInput, WorkScheduleDto } from '../dtos/models';

@Injectable({
  providedIn: 'root',
})
export class WorkScheduleService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateWorkScheduleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkScheduleDto>({
      method: 'POST',
      url: '/api/app/work-schedule',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/work-schedule/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkScheduleDto>({
      method: 'GET',
      url: `/api/app/work-schedule/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAllWorkSchedulesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WorkScheduleDto>>({
      method: 'GET',
      url: '/api/app/work-schedule',
      params: { keyword: input.keyword, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: number, input: CreateUpdateWorkScheduleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkScheduleDto>({
      method: 'PUT',
      url: `/api/app/work-schedule/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}