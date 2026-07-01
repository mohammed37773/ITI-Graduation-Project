import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';

@Injectable({ providedIn: 'root' })
export class NurseriesService {
  constructor(private api: ApiService) {}

  getAll(): Observable<any> {
    return this.api.get('/api/admin/nurseries');
  }

  getStats(): Observable<any> {
    return this.api.get('/api/admin/nurseries/stats');
  }

  verify(id: number): Observable<any> {
    return this.api.put(`/api/admin/nurseries/${id}/verify`);
  }

  delete(id: number): Observable<any> {
    return this.api.delete(`/api/admin/nurseries/${id}`);
  }

}
