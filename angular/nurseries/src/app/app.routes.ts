import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { roleGuard } from './core/guards/role-guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./shared/components/main-layout/main-layout').then(m => m.MainLayout),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/home/pages/home/home').then(m => m.Home)
      },
      {
        path: 'nurseries',
        loadComponent: () => import('./features/nurseries/pages/nursery-list/nursery-list').then(m => m.NurseryList)
      },
      {
        path: 'nurseries/:id',
        loadComponent: () => import('./features/nurseries/pages/nursery-details/nursery-details').then(m => m.NurseryDetails)
      }
    ]
  },

  {
    path: 'auth',
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/pages/login/login').then(m => m.Login)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/pages/register/register').then(m => m.Register)
      }
    ]
  },

  // --- مسارات ولي الأمر (Parent Role) ---
  {
    path: 'parent',
    loadComponent: () => import('./shared/components/main-layout/main-layout').then(m => m.MainLayout),
    canActivate: [authGuard, roleGuard],
    data: { expectedRole: 'Parent' }, // مطابق تماماً للـ DB
    children: [
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/pages/profile/profile').then(m => m.Profile)
      },
      {
        path: 'chatbot',
        loadComponent: () => import('./features/chatbot/components/chatbot/chatbot').then(m => m.Chatbot)
      },
      {
        path: 'ai-questionnaire',
        loadComponent: () => import('./features/recommendations/pages/ai-recommendation/ai-recommendation').then(m => m.AiRecommendation)
      },
      {
        path: 'nursery-list',
        loadComponent: () => import('./features/nurseries/pages/nursery-list/nursery-list').then(m => m.NurseryList)
      },
      {
        path: 'bookings',        
        loadComponent: () => import('./features/bookings/bookings/bookings').then(m => m.Bookings)
      },
      {
        path: 'bookings/new/:nurseryId',
        loadComponent: () => import('./features/bookings/new-booking/new-booking').then(m => m.NewBooking)
      },
      {
  path: 'payment/:bookingId',
  loadComponent: () => import('./features/payment/payment').then(m => m.Payment)
}
    ]
  },

  // --- مسارات صاحب الحضانة (NurseryAdmin) ---
  {
    path: 'owner',
    loadComponent: () => import('./layouts/nursery-owner/nursery-owner').then(m => m.NurseryOwner),
    canActivate: [authGuard, roleGuard],
    data: { expectedRole: 'NurseryAdmin' }, 
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/nursery-owner/dashboard/dashboard').then(m => m.Dashboard)
      },
      {
        path: 'my-nursery',
        loadComponent: () => import('./features/nursery-owner/manage-nursery/manage-nursery').then(m => m.ManageNursery)
      },
      {
        path: 'bookings',
        loadComponent: () => import('./features/nursery-owner/manage-bookings/manage-bookings').then(m => m.ManageBookings)
      }
    ]
  },

  // --- صفحات الـ Admin العام ---
  {
    path: '',
    loadComponent: () => import('./layouts/admin-layout/admin-layout').then(m => m.AdminLayout),
    // canActivate: [authGuard, roleGuard],
    data: { expectedRole: 'Admin' },
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/admin/pages/dashboard/dashboard').then(m => m.Dashboard)
      },
      {
        path: 'manage-nurseries',
        loadComponent: () => import('./features/admin/pages/manage-nurseries/manage-nurseries').then(m => m.ManageNurseries)
      },
      {
        path: 'users',
        loadComponent: () => import('./features/admin/pages/manage-users/manage-users').then(m => m.ManageUsers)
      },
      {
        path: 'payments',
        loadComponent: () => import('./features/admin/pages/payments/payments').then(m => m.Payments)
      },
        {
        path: 'reports',
        loadComponent: () => import('./features/admin/pages/reports/reports').then(m => m.Reports)
      },
         {
        path: 'bookings',
        loadComponent: () => import('./features/admin/pages/bookings/bookings').then(m => m.Bookings)
      },
    ]
  },

  {
    path: '**',
    loadComponent: () => import('./shared/components/not-found/not-found').then(m => m.NotFound)
  }
];