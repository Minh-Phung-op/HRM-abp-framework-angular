import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { AttendanceDto, CreateAttendanceDto, GetAllAttendancesInput, UpdateAttendanceDto } from '../dtos/models';

@Injectable({
  providedIn: 'root',
})
export class AttendanceService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  approveExplain = (id: number, note: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/attendance/${id}/approve-explain`,
      params: { note },
    },
    { apiName: this.apiName,...config });
  

  bulkLock = (filter: GetAllAttendancesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/attendance/bulk-lock',
      body: filter,
    },
    { apiName: this.apiName,...config });
  

  checkIn = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceDto>({
      method: 'POST',
      url: '/api/app/attendance/check-in',
    },
    { apiName: this.apiName,...config });
  

  checkOut = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceDto>({
      method: 'POST',
      url: '/api/app/attendance/check-out',
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateAttendanceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceDto>({
      method: 'POST',
      url: '/api/app/attendance',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/attendance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceDto>({
      method: 'GET',
      url: `/api/app/attendance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetAllAttendancesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AttendanceDto>>({
      method: 'GET',
      url: '/api/app/attendance',
      params: { keyword: input.keyword, employeeId: input.employeeId, departmentId: input.departmentId, scheduleId: input.scheduleId, status: input.status, source: input.source, explainStatus: input.explainStatus, isLocked: input.isLocked, workDateFrom: input.workDateFrom, workDateTo: input.workDateTo, month: input.month, year: input.year, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  lock = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceDto>({
      method: 'POST',
      url: `/api/app/attendance/${id}/lock`,
    },
    { apiName: this.apiName,...config });
  

  requestExplain = (id: number, explainNote: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/attendance/${id}/request-explain`,
      params: { explainNote },
    },
    { apiName: this.apiName,...config });
  

  unlock = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceDto>({
      method: 'POST',
      url: `/api/app/attendance/${id}/unlock`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: number, input: UpdateAttendanceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceDto>({
      method: 'PUT',
      url: `/api/app/attendance/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}