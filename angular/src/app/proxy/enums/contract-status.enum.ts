import { mapEnumToOptions } from '@abp/ng.core';

export enum ContractStatus {
  Active = 1,
  Expired = 2,
  Terminated = 3,
}

export const contractStatusOptions = mapEnumToOptions(ContractStatus);
