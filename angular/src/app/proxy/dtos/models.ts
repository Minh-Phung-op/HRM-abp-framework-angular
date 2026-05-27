import type { CreationAuditedEntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { AttendanceStatus } from '../enums/attendance-status.enum';
import type { AttendanceExplainStatus } from '../enums/attendance-explain-status.enum';
import type { AttendanceSource } from '../enums/attendance-source.enum';
import type { ContractType } from '../enums/contract-type.enum';
import type { ContractStatus } from '../enums/contract-status.enum';
import type { ApprovalStep } from '../enums/approval-step.enum';
import type { ApprovalAction } from '../enums/approval-action.enum';
import type { Gender } from '../enums/gender.enum';
import type { EmployeeStatus } from '../enums/employee-status.enum';
import type { PayrollItemType } from '../enums/payroll-item-type.enum';
import type { WorkingDayFlags } from '../enums/working-day-flags.enum';
import type { LeaveRequestStatus } from '../enums/leave-request-status.enum';
import type { PayrollStatus } from '../enums/payroll-status.enum';

export interface AdjustLeaveBalanceDto {
  adjustmentDays: number;
}

export interface AttendanceDto extends FullAuditedEntityDto<number> {
  employeeId?: number;
  employeeName?: string;
  employeeCode?: string;
  workDate?: string;
  scheduleId?: number;
  scheduleName?: string;
  checkInAt?: string | null;
  checkOutAt?: string | null;
  status?: AttendanceStatus;
  lateMinutes?: number | null;
  earlyLeaveMinutes?: number | null;
  explainNote?: string | null;
  explainStatus?: AttendanceExplainStatus | null;
  explainApprovedBy?: number | null;
  otMinutes?: number;
  note?: string;
  source?: AttendanceSource;
  isLocked?: boolean;
}

export interface BulkInitializeLeaveBalanceDto {
  leaveTypeId: number;
  year: number;
  defaultDays?: number | null;
}

export interface ContractDto extends FullAuditedEntityDto<number> {
  employeeId?: number;
  employeeName?: string;
  employeeCode?: string;
  contractNumber?: string;
  contractType?: ContractType;
  signDate?: string;
  startDate?: string;
  endDate?: string | null;
  basicSalary?: number;
  insuranceSalary?: number;
  status?: ContractStatus;
}

export interface CreateAttendanceDto {
  employeeId: number;
  workDate: string;
  scheduleId: number;
  checkInAt?: string | null;
  checkOutAt?: string | null;
  status: AttendanceStatus;
  lateMinutes?: number | null;
  earlyLeaveMinutes?: number | null;
  otMinutes?: number;
  note?: string;
  source: AttendanceSource; 
}

export interface CreateContractDto {
  employeeId: number;
  contractNumber: string;
  contractType: ContractType;
  signDate: string;
  startDate: string;
  endDate?: string | null;
  basicSalary?: number;
  insuranceSalary?: number;
  status: ContractStatus;
}

export interface CreateLeaveRequestApprovalLogDto {
  actionStep: ApprovalStep;
  action: ApprovalAction;
  comment?: string;
}

export interface CreateLeaveRequestDto {
  leaveTypeId: number;
  startDate: string;
  endDate: string;
  reason?: string;
}

export interface CreatePayrollDto {
  employeeId: number;
  year: number;
  month: number;
  baseSalary?: number;
  items?: CreateUpdatePayrollItemDto[];
}

export interface CreateUpdateDepartmentDto {
  name: string;
  code: string;
  parentId?: number | null;
  managerId?: number | null;
  isActive?: boolean;
}

export interface CreateUpdateEmployeeDto {
  employeeCode: string;
  fullName: string;
  email: string;
  phone?: string;
  dateOfBirth?: string | null;
  gender: Gender;
  nationalId?: string;
  address?: string;
  avatarUrl?: string;
  departmentId: number;
  positionId: number;
  managerId?: number | null;
  startDate: string;
  contractType: ContractType;
  contractEndDate?: string | null;
  status: EmployeeStatus;
}

export interface CreateUpdateLeaveBalanceDto {
  employeeId: number;
  leaveTypeId: number;
  year: number;
  allocatedDays?: number;
  carriedOverDays?: number;
}

export interface CreateUpdateLeaveTypeDto {
  name: string;
  code: string;
  defaultDaysPerYear?: number;
  carryOver?: boolean;
  maxCarryDays?: number | null;
  paid?: boolean;
}

export interface CreateUpdatePayrollItemDto {
  type: PayrollItemType;
  label: string;
  amount?: number;
  note?: string;
}

export interface CreateUpdatePositionDto {
  title: string;
  level?: string;
  departmentId: number;
  isActive?: boolean;
}

export interface CreateUpdateWorkScheduleDto {
  name: string;
  checkInTime: string;
  checkOutTime: string;
  lateThresholdMinutes?: number;
  workingDays: WorkingDayFlags;
}

export interface DepartmentDto extends FullAuditedEntityDto<number> {
  name?: string;
  code?: string;
  parentId?: number | null;
  parentName?: string;
  managerId?: number | null;
  managerName?: string;
  isActive?: boolean;
}

export interface EmployeeDto extends FullAuditedEntityDto<number> {
  userId?: string | null;
  employeeCode?: string;
  fullName?: string;
  email?: string;
  phone?: string;
  dateOfBirth?: string | null;
  gender?: Gender;
  roles?: string[];
  nationalId?: string;
  address?: string;
  avatarUrl?: string;
  departmentId?: number;
  departmentName?: string;
  positionId?: number;
  positionTitle?: string;
  managerId?: number | null;
  managerName?: string;
  startDate?: string;
  contractType?: ContractType;
  contractEndDate?: string | null;
  status?: EmployeeStatus;
  contracts?: ContractDto[];
}

export interface GeneratePayrollInput {
  year: number;
  month: number;
  departmentId?: number | null;
}

export interface GetAllAttendancesInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
  employeeId?: number | null;
  departmentId?: number | null;
  scheduleId?: number | null;
  status?: AttendanceStatus | null;
  source?: AttendanceSource | null;
  explainStatus?: AttendanceExplainStatus | null;
  isLocked?: boolean | null;
  workDateFrom?: string | null;
  workDateTo?: string | null;
  month?: number | null;
  year?: number | null;
}

export interface GetAllContractsInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
  employeeId?: number | null;
  contractType?: ContractType | null;
  status?: ContractStatus | null;
  startDateFrom?: string | null;
  startDateTo?: string | null;
}

export interface GetAllDepartmentsInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
  parentId?: number | null;
  isActive?: boolean | null;
}

export interface GetAllEmployeesInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
  departmentId?: number | null;
  positionId?: number | null;
  managerId?: number | null;
  contractType?: ContractType | null;
  status?: EmployeeStatus | null;
  gender?: Gender | null;
  startDateFrom?: string | null;
  startDateTo?: string | null;
}

export interface GetAllLeaveBalancesInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
  employeeId?: number | null;
  departmentId?: number | null;
  leaveTypeId?: number | null;
  year?: number | null;
}

export interface GetAllLeaveRequestsInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
  employeeId?: number | null;
  departmentId?: number | null;
  leaveTypeId?: number | null;
  status?: LeaveRequestStatus | null;
  month?: number | null;
  year?: number | null;
  startDateFrom?: string | null;
  startDateTo?: string | null;
}

export interface GetAllLeaveTypesInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
  paid?: boolean | null;
  carryOver?: boolean | null;
}

export interface GetAllPayrollsInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
  employeeId?: number | null;
  departmentId?: number | null;
  year?: number | null;
  month?: number | null;
  status?: PayrollStatus | null;
  netSalaryFrom?: number | null;
  netSalaryTo?: number | null;
}

export interface GetAllPositionsInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
  departmentId?: number | null;
  isActive?: boolean | null;
}

export interface GetAllWorkSchedulesInput extends PagedAndSortedResultRequestDto {
  keyword?: string | null;
}

export interface LeaveBalanceDto extends FullAuditedEntityDto<number> {
  employeeId?: number;
  employeeName?: string;
  employeeCode?: string;
  leaveTypeId?: number;
  leaveTypeName?: string;
  year?: number;
  allocatedDays?: number;
  carriedOverDays?: number;
  totalDays?: number;
  usedDays?: number;
  pendingDays?: number;
  remainingDays?: number;
}

export interface LeaveRequestApprovalLogDto extends CreationAuditedEntityDto<number> {
  leaveRequestId?: number;
  userId?: string;
  userName?: string;
  userFullName?: string;
  actionStep?: ApprovalStep;
  action?: ApprovalAction;
  comment?: string;
}

export interface LeaveRequestDto extends FullAuditedEntityDto<number> {
  employeeId?: number;
  employeeName?: string;
  employeeCode?: string;
  leaveTypeId?: number;
  leaveTypeName?: string;
  startDate?: string;
  endDate?: string;
  totalDays?: number;
  reason?: string;
  status?: LeaveRequestStatus;
  approvalLogs?: LeaveRequestApprovalLogDto[];
}

export interface LeaveTypeDto extends FullAuditedEntityDto<number> {
  name?: string;
  code?: string;
  defaultDaysPerYear?: number;
  carryOver?: boolean;
  maxCarryDays?: number;
  paid?: boolean;
}

export interface PayrollDto extends FullAuditedEntityDto<number> {
  employeeId?: number;
  employeeName?: string;
  employeeCode?: string;
  departmentName?: string;
  positionTitle?: string;
  year?: number;
  month?: number;
  baseSalary?: number;
  grossSalary?: number;
  netSalary?: number;
  totalDeduction?: number;
  bhxhEmployee?: number;
  bhytEmployee?: number;
  bhtnEmployee?: number;
  pit?: number;
  status?: PayrollStatus;
  lockedAt?: string | null;
  paidAt?: string | null;
  items?: PayrollItemDto[];
}

export interface PayrollItemDto extends FullAuditedEntityDto<number> {
  payrollId?: number;
  type?: PayrollItemType;
  label?: string;
  amount?: number;
  note?: string;
}

export interface PositionDto extends FullAuditedEntityDto<number> {
  title?: string;
  level?: string;
  departmentId?: number;
  departmentName?: string;
  isActive?: boolean;
}

export interface UpdateAttendanceDto {
  checkInAt?: string | null;
  checkOutAt?: string | null;
  status?: AttendanceStatus | null;
  lateMinutes?: number | null;
  earlyLeaveMinutes?: number | null;
  otMinutes?: number;
  note?: string;
  explainNote?: string;
}

export interface UpdateContractDto {
  contractNumber: string;
  contractType: ContractType;
  signDate: string;
  startDate: string;
  endDate?: string | null;
  basicSalary?: number;
  insuranceSalary?: number;
  status: ContractStatus;
}

export interface UpdateLeaveRequestDto {
  leaveTypeId: number;
  startDate: string;
  endDate: string;
  reason?: string;
}

export interface UpdatePayrollDto {
  baseSalary?: number;
  items?: CreateUpdatePayrollItemDto[];
}

export interface WorkScheduleDto extends FullAuditedEntityDto<number> {
  name?: string;
  checkInTime?: string;
  checkOutTime?: string;
  lateThresholdMinutes?: number;
  workingDays?: WorkingDayFlags;
}
