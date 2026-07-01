import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.backUrl;

  constructor(private http: HttpClient) {}

  get<T>(path: string): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${path}`, { headers: this.headers() });
  }

  post<T>(path: string, body?: unknown): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${path}`, body, { headers: this.headers() });
  }

  put<T>(path: string, body?: unknown): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${path}`, body, { headers: this.headers() });
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${path}`, { headers: this.headers() });
  }

  private headers(): HttpHeaders {
    const token = localStorage.getItem('token');
    return token
      ? new HttpHeaders({ Authorization: `Bearer ${token}` })
      : new HttpHeaders();
  }
}
