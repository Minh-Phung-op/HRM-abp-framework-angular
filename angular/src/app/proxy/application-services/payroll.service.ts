import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { CreatePayrollDto, GeneratePayrollInput, GetAllPayrollsInput, PayrollDto, UpdatePayrollDto } from '../dtos/models';

@Injectable({
  providedIn: 'root',
})
export class PayrollService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  approve = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDto>({
      method: 'POST',
      url: `/api/app/payroll/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreatePayrollDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDto>({
      method: 'POST',
      url: '/api/app/payroll',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/payroll/${id}`,
    },
    { apiName: this.apiName,...config });
  

  generate = (input: GeneratePayrollInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/payroll/generate',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDto>({
      method: 'GET',
      url: `/api/app/payroll/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAllPayrollsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PayrollDto>>({
      method: 'GET',
      url: '/api/app/payroll',
      params: { keyword: input.keyword, employeeId: input.employeeId, departmentId: input.departmentId, year: input.year, month: input.month, status: input.status, netSalaryFrom: input.netSalaryFrom, netSalaryTo: input.netSalaryTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  lock = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDto>({
      method: 'POST',
      url: `/api/app/payroll/${id}/lock`,
    },
    { apiName: this.apiName,...config });
  

  markAsPaid = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDto>({
      method: 'POST',
      url: `/api/app/payroll/${id}/mark-as-paid`,
    },
    { apiName: this.apiName,...config });
  

  submit = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDto>({
      method: 'POST',
      url: `/api/app/payroll/${id}/submit`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: number, input: UpdatePayrollDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDto>({
      method: 'PUT',
      url: `/api/app/payroll/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}