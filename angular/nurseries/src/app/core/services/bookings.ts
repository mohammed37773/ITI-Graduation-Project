import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';
import { Booking } from '../models/booking.model';


@Injectable({ providedIn: 'root' })
export class BookingsService {
  constructor(private api: ApiService) {}

  getAll(): Observable<any> {
    return this.api.get('/api/admin/bookings');
  }

  getStats(): Observable<any> {
    return this.api.get('/api/admin/bookings/stats');
  }

  cancel(id: number): Observable<any> {
    return this.api.put(`/api/Bookings/${id}/cancel`, {});
  }
}
