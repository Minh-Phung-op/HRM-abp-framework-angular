import { mapEnumToOptions } from '@abp/ng.core';

export enum ApprovalStep {
  TeamLead = 1,
  HR = 2,
}

export const approvalStepOptions = mapEnumToOptions(ApprovalStep);
