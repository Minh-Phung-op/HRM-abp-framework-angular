import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'tenant-management',
    loadChildren: () => import('@abp/ng.tenant-management').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  {
    path: 'organization',
    loadChildren: () =>
      import('./pages/organization/organization.routes')
        .then(r => r.ORGANIZATION_ROUTES)
  },
  {
    path: 'leave-management',
    loadChildren: () =>
      import('./pages/leave-management/leave-management.routes')
        .then(r => r.LEAVE_MANAGEMENT_ROUTES)
  },
  {
    path: 'payroll',
    loadChildren: () =>
      import('./pages/payroll/payroll.routes')
        .then(r => r.PAYROLL_ROUTES)
  },
  {
    path: 'attendance',
    loadChildren: () =>
      import('./pages/attendance/attendance.routes')
        .then(r => r.ATTENDANCE_ROUTES)
  }
];
