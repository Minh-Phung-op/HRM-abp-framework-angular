import { mapEnumToOptions } from '@abp/ng.core';

export enum AttendanceStatus {
  Present = 1,
  Late = 2,
  Early = 3,
  Absent = 4,
  HalfDay = 5,
  OnLeave = 6,
}

export const attendanceStatusOptions = mapEnumToOptions(AttendanceStatus);
