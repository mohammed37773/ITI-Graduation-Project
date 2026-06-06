import { Routes } from '@angular/router';
import { guestGuard } from './core/guards/guest-guard';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/home/pages/home/home')
        .then(c => c.Home)
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/pages/login/login')
        .then(c => c.Login),
    canActivate: [guestGuard]
  },

  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/pages/register/register')
        .then(c => c.Register),
    canActivate: [guestGuard]
  },

  {
    path: 'nurseries',
    loadComponent: () =>
      import('./features/nurseries/pages/nursery-list/nursery-list')
        .then(c => c.NurseryList)
  },

  {
    path: 'nurseries/:id',
    loadComponent: () =>
      import('./features/nurseries/pages/nursery-details/nursery-details')
        .then(c => c.NurseryDetails)
  },

  {
    path: 'recommendation',
    loadComponent: () =>
      import('./features/recommendations/pages/ai-recommendation/ai-recommendation')
        .then(c => c.AiRecommendation)
  },

  {
    path: 'profile',
    loadComponent: () =>
      import('./features/profile/pages/profile/profile')
        .then(c => c.Profile),
    canActivate: [authGuard]
  },

  {
    path: 'admin',
    loadComponent: () =>
      import('./features/admin/pages/dashboard/dashboard')
        .then(c => c.Dashboard)
  }
];
