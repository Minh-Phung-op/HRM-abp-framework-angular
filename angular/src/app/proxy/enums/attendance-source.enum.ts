import { mapEnumToOptions } from '@abp/ng.core';

export enum AttendanceSource {
  Manual = 1,
  Device = 2,
  Mobile = 3,
}

export const attendanceSourceOptions = mapEnumToOptions(AttendanceSource);
