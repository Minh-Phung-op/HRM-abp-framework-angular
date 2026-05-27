import { mapEnumToOptions } from '@abp/ng.core';

export enum WorkingDayFlags {
  None = 0,
  Monday = 1,
  Tuesday = 2,
  Wednesday = 4,
  Thursday = 8,
  Friday = 16,
  Weekdays = 31,
  Saturday = 32,
  Sunday = 64,
}

export const workingDayFlagsOptions = mapEnumToOptions(WorkingDayFlags);
