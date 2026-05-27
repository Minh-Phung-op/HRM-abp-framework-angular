import { mapEnumToOptions } from '@abp/ng.core';

export enum PayrollStatus {
  Draft = 1,
  Processing = 2,
  Calculated = 3,
  Approved = 4,
  Paid = 5,
}

export const payrollStatusOptions = mapEnumToOptions(PayrollStatus);
