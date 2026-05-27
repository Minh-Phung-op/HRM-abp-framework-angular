import { Routes } from '@angular/router';

export const ATTENDANCE_ROUTES: Routes = [
  {
    path: 'attendance',
    loadComponent: () =>
      import('./attendance')
        .then(c => c.AttendanceComponent)
  },
//   {
//     path: 'employee',
//     loadComponent: () =>
//       import('./employee/employee')
//         .then(c => c.Employee)
//   },
];