import { mapEnumToOptions } from '@abp/ng.core';

export enum EmployeeStatus {
  Active = 1,
  Onleave = 2,
  Resigned = 3,
  Terminated = 4,
}

export const employeeStatusOptions = mapEnumToOptions(EmployeeStatus);
