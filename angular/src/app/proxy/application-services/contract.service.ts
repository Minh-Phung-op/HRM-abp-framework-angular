import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { ContractDto, CreateContractDto, GetAllContractsInput, UpdateContractDto } from '../dtos/models';

@Injectable({
  providedIn: 'root',
})
export class ContractService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateContractDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ContractDto>({
      method: 'POST',
      url: '/api/app/contract',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/contract/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ContractDto>({
      method: 'GET',
      url: `/api/app/contract/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAllContractsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ContractDto>>({
      method: 'GET',
      url: '/api/app/contract',
      params: { keyword: input.keyword, employeeId: input.employeeId, contractType: input.contractType, status: input.status, startDateFrom: input.startDateFrom, startDateTo: input.startDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: number, input: UpdateContractDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ContractDto>({
      method: 'PUT',
      url: `/api/app/contract/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}