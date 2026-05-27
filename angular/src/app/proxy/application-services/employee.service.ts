import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { CreateUpdateEmployeeDto, EmployeeDto, GetAllEmployeesInput } from '../dtos/models';

@Injectable({
  providedIn: 'root',
})
export class EmployeeService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateEmployeeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeDto>({
      method: 'POST',
      url: '/api/app/employee',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createAccountForEmployee = (employeeId: number, email: string, password: string, roleName: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/employee/account-for-employee/${employeeId}`,
      params: { email, password, roleName },
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/employee/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeDto>({
      method: 'GET',
      url: `/api/app/employee/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getAssignableRoles = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, string[]>({
      method: 'GET',
      url: '/api/app/employee/assignable-roles',
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAllEmployeesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<EmployeeDto>>({
      method: 'GET',
      url: '/api/app/employee',
      params: { keyword: input.keyword, departmentId: input.departmentId, positionId: input.positionId, managerId: input.managerId, contractType: input.contractType, status: input.status, gender: input.gender, startDateFrom: input.startDateFrom, startDateTo: input.startDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getMyProfile = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeDto>({
      method: 'GET',
      url: '/api/app/employee/my-profile',
    },
    { apiName: this.apiName,...config });
  

  offboard = (id: number, terminationDate: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/employee/${id}/offboard`,
      params: { terminationDate },
    },
    { apiName: this.apiName,...config });
  

  update = (id: number, input: CreateUpdateEmployeeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeDto>({
      method: 'PUT',
      url: `/api/app/employee/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}