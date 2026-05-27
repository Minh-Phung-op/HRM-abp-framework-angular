import { mapEnumToOptions } from '@abp/ng.core';

export enum AttendanceExplainStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
}

export const attendanceExplainStatusOptions = mapEnumToOptions(AttendanceExplainStatus);
