import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'services',
    loadComponent: () => import('./features/services/services-list/services-list.component').then(m => m.ServicesListComponent)
  },
  {
    path: 'services/:id',
    loadComponent: () => import('./features/services/service-detail/service-detail.component').then(m => m.ServiceDetailComponent)
  },
  {
    path: 'bookings',
    loadComponent: () => import('./features/bookings/bookings-list/bookings-list.component').then(m => m.BookingsListComponent),
    canActivate: [authGuard]
  },
  {
    path: 'bookings/:id',
    loadComponent: () => import('./features/bookings/booking-detail/booking-detail.component').then(m => m.BookingDetailComponent),
    canActivate: [authGuard]
  },
  {
    path: 'profile',
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard]
  },
  {
    path: 'provider/dashboard',
    loadComponent: () => import('./features/provider/dashboard/provider-dashboard.component').then(m => m.ProviderDashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/dashboard',
    loadComponent: () => import('./features/admin/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    canActivate: [adminGuard]
  },
  {
    path: 'admin/users',
    loadComponent: () => import('./features/admin/users/users-management.component').then(m => m.UsersManagementComponent),
    canActivate: [adminGuard]
  },
  {
    path: 'admin/services',
    loadComponent: () => import('./features/admin/services/services-management.component').then(m => m.ServicesManagementComponent),
    canActivate: [adminGuard]
  },
  {
    path: 'admin/bookings',
    loadComponent: () => import('./features/admin/bookings/bookings-management.component').then(m => m.BookingsManagementComponent),
    canActivate: [adminGuard]
  },
  {
    path: 'admin/reports',
    loadComponent: () => import('./features/admin/reports/reports-analytics.component').then(m => m.ReportsAnalyticsComponent),
    canActivate: [adminGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
