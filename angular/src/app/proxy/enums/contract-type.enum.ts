import { mapEnumToOptions } from '@abp/ng.core';

export enum ContractType {
  Fulltime = 1,
  Partime = 2,
  Contract = 3,
}

export const contractTypeOptions = mapEnumToOptions(ContractType);
