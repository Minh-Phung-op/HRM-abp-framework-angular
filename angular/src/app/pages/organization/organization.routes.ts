import { Routes } from '@angular/router';

export const ORGANIZATION_ROUTES: Routes = [
  {
    path: 'department',
    loadComponent: () =>
      import('./department/department')
        .then(c => c.Department)
  },
  {
    path: 'employee',
    loadComponent: () =>
      import('./employee/employee')
        .then(c => c.Employee)
  },
  {
    path: 'employee/detail/:id',
    loadComponent: () =>
      import('./employee/employee-detail/employee-detail')
        .then(c => c.EmployeeDetail)
  },
  {
    path: 'position',
    loadComponent: () =>
      import('./position/position')
        .then(c => c.Position)
  }
];