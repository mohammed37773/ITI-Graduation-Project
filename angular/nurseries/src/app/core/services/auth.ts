import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponseDto, LoginDto, RegisterDto } from '../models/authModel';
@Injectable({
  providedIn: 'root'
})
export class AuthService {
private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5104/api/Auth';

  currentUser = signal<AuthResponseDto | null>(null);
  isAuthenticated = signal<boolean>(false);
  
  // حل مشكلة الـ guest-guard
  isLoggedIn = computed(() => this.isAuthenticated());

  constructor() {
    this.loadSavedUser();
  }

  register(registerData: RegisterDto): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/register`, registerData);
  }

  confirmEmail(email: string, token: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/confirm-email`, {
      params: { email, token }
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
    return this.currentUser()?.role || null;
  }
};
