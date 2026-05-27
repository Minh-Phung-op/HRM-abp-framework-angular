import { Routes } from '@angular/router';

export const LEAVE_MANAGEMENT_ROUTES: Routes = [
  {
    path: 'leave-request',
    loadComponent: () =>
      import('./leave-request/leave-request')
        .then(c => c.LeaveRequest)
  },
  {
    path: 'leave-request/detail/:id',
    loadComponent: () =>
      import('./leave-request/leave-request-detail/leave-request-detail')
        .then(c => c.LeaveRequestDetail)
  },
  {
    path: 'leave-balance',
    loadComponent: () =>
      import('./leave-balance/leave-balance')
        .then(c => c.LeaveBalanceComponent)
  },
  {
    path: 'leave-type',
    loadComponent: () =>
      import('./leave-type/leave-type')
        .then(c => c.LeaveTypeComponent)
  },
];