import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';

@Injectable({ providedIn: 'root' })
export class PaymentsService {
  constructor(private api: ApiService) {}

  getAll(): Observable<any> {
    return this.api.get('/api/admin/payments');
  }

  getStats(): Observable<any> {
    return this.api.get('/api/admin/payments/stats');
  }

  refund(id: number): Observable<any> {
    return this.api.post(`/api/admin/payments/${id}/refund`);
  }
}