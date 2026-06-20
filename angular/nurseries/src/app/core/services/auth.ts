import { inject, Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { RegisterDto, LoginDto, AuthResponseDto } from '../models/authModel';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5104/api/Auth';

  currentUser = signal<AuthResponseDto | null>(null);
  isAuthenticated = signal<boolean>(false);
  
  // لخدمة الـ guest-guard والـ auth-guard لايف
  isLoggedIn = computed(() => this.isAuthenticated());
  userRole = computed(() => this.currentUser()?.role || null);

  constructor() {
    this.loadSavedUser();
  }

  register(registerData: any): Observable<any> {
    // الباك إند بيرجع نص صريح (String) عند النجاح، عشان كدة بنخليه expect 'text'
    return this.http.post(`${this.baseUrl}/register`, registerData, { responseType: 'text' });
  }

  confirmEmail(email: string, token: string): Observable<any> {
    // إرسال الـ email والـ token كـ Query Parameters في الـ GET Request بالملي زي الـ API
    return this.http.get(`${this.baseUrl}/confirm-email`, {
      params: { email, token },
      responseType: 'text'
    });
  }

  login(loginData: LoginDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/login`, loginData).pipe(
      tap((response) => {
        localStorage.setItem('user_session', JSON.stringify(response));
        this.currentUser.set(response);
        this.isAuthenticated.set(true);
      })
    );
  }

  logout() {
    localStorage.removeItem('user_session');
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }

  private loadSavedUser() {
    const savedSession = localStorage.getItem('user_session');
    if (savedSession) {
      const user: AuthResponseDto = JSON.parse(savedSession);
      this.currentUser.set(user);
      this.isAuthenticated.set(true);
    }
  }

  getToken(): string | null {
    return this.currentUser()?.token || null;
  }

  getRole(): string | null {
    return this.userRole();
  }
}