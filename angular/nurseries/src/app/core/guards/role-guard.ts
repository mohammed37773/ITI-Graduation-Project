import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // جلب الـ Role المتوقع للصفحة دي من ملف الـ routes
  const expectedRole = route.data['expectedRole'];
  // جلب دور المستخدم الحالي المسجل
  const userRole = authService.getRole();

  if (authService.isAuthenticated() && userRole === expectedRole) {
    return true; // مسموح له بالدخول
  }

  // لو مش دوره، ودديه لصفحة الـ Login أو صفحة غير مصرح له
  router.navigate(['/auth/login']);
  return false;
};
