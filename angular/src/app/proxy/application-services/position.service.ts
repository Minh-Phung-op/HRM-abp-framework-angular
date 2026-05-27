import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { CreateUpdatePositionDto, GetAllPositionsInput, PositionDto } from '../dtos/models';

@Injectable({
  providedIn: 'root',
})
export class PositionService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdatePositionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PositionDto>({
      method: 'POST',
      url: '/api/app/position',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/position/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PositionDto>({
      method: 'GET',
      url: `/api/app/position/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAllPositionsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PositionDto>>({
      method: 'GET',
      url: '/api/app/position',
      params: { keyword: input.keyword, departmentId: input.departmentId, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: number, input: CreateUpdatePositionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PositionDto>({
      method: 'PUT',
      url: `/api/app/position/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}