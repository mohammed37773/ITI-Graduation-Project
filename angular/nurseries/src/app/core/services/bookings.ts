import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';


@Injectable({ providedIn: 'root' })
export class BookingsService {
  constructor(private api: ApiService) {}

  getAll(): Observable<any> {
    return this.api.get('/api/admin/bookings');
  }

  getStats(): Observable<any> {
    return this.api.get('/api/admin/bookings/stats');
  }
}
