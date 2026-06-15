import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

export const authGuard : CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // لو المستخدم عامل تسجيل دخول ومعاه Token ساري
  if (authService.isAuthenticated()) {
    return true; 
  }

  // لو مش مسجل دخول، خده على صفحة الـ Login ورجعه لنفس الصفحة اللي كان رايحها بعد ما يسجل
  router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  return false;
};