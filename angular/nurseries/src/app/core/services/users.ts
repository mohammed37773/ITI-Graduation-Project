import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';

@Injectable({ providedIn: 'root' })
export class UsersService {
  constructor(private api: ApiService) {}

  getAll(): Observable<any> {
    return this.api.get('/api/admin/users');
  }

  getById(id: string): Observable<any> {
    return this.api.get(`/api/admin/users/${id}`);
  }

  getStats(): Observable<any> {
    return this.api.get('/api/admin/users/stats');
  }

  ban(id: string): Observable<any> {
    return this.api.put(`/api/admin/users/${id}/ban`);
  }

  unban(id: string): Observable<any> {
    return this.api.put(`/api/admin/users/${id}/unban`);
  }
}
