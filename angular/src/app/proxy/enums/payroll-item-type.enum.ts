import { mapEnumToOptions } from '@abp/ng.core';

export enum PayrollItemType {
  Allowance = 1,
  Bonus = 2,
  Deduction = 3,
  Advance = 4,
}

export const payrollItemTypeOptions = mapEnumToOptions(PayrollItemType);
