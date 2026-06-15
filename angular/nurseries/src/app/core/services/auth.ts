import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment'; // تأكد من المسار الصحيح للبيئة
@Injectable({
  providedIn: 'root'
})
export class AuthService {

 private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/auth`; 

  currentUserRole = signal<string | null>(null);

  constructor() {
    this.loadUserFromStorage();
  }

  // دالة الـ Register وترسل الـ Data للـ Backend
  register(userData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, userData);
  }

  // دالة الـ Login وبتخزن الـ Token وتحدد الـ Role
  login(credentials: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);
          this.decodeAndSaveUser(response.token);
        }
      })
    );
  }

  // دالة لفك تشفير الـ Token واستخراج الـ Role
  private decodeAndSaveUser(token: string) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.currentUserRole.set(payload.role); // حفظ الدور في الـ Signal
    } catch (error) {
      console.error('Invalid token', error);
      this.currentUserRole.set(null);
    }
  }

  // دالة لجلب الدور الحالي
  getRole(): string | null {
    return this.currentUserRole();
  }

  // دالة للتحقق إذا المستخدم مسجل دخول
  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  }

  // دالة لتسجيل الخروج ومسح البيانات
  logout() {
    localStorage.removeItem('token');
    this.currentUserRole.set(null);
  }

  // تحميل بيانات المستخدم من التخزين عند بدء التطبيق
  private loadUserFromStorage() {
    const token = localStorage.getItem('token');
    if (token) {
      this.decodeAndSaveUser(token);
    }
  }
  isLoggedIn(): boolean {
    return this.isAuthenticated();
  }
};
