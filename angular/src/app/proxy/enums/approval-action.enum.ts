import { mapEnumToOptions } from '@abp/ng.core';

export enum ApprovalAction {
  Approve = 1,
  Reject = 2,
  Cancel = 3,
}

export const approvalActionOptions = mapEnumToOptions(ApprovalAction);
