import { mapEnumToOptions } from '@abp/ng.core';

export enum LeaveRequestStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Cancelled = 4,
}

export const leaveRequestStatusOptions = mapEnumToOptions(LeaveRequestStatus);
