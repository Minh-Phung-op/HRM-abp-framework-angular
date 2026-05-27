import { Routes } from '@angular/router';

export const PAYROLL_ROUTES: Routes = [
  {
    path: 'payroll-period-list',
    loadComponent: () =>
      import('./payroll-period-list/payroll-period-list')
        .then(c => c.PayrollPeriodListComponent)
  },
  {
    path: 'worksheet',
    loadComponent: () =>
      import('./payroll-worksheet/payroll-worksheet')
        .then(c => c.PayrollWorksheetComponent)
  },
];