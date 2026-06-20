import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  // لو التوكن موجود، بنعمل Clone للـ Request ونضيف الـ Header
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}` // الصياغة المطلوبة من الباك
      }
    });
  }

  return next(req);
};