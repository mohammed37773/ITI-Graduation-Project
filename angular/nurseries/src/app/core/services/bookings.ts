import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';
import { Booking } from '../models/booking.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BookingsService {
  backUrl = environment.backUrl;
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

  refund(id: number): Observable<any> {
    console.log("refunding...");
    return this.api.post(`/api/Payment/${id}/owner-refund`, {});
  }

  complete(id: number): Observable<any> {
    console.log("completing...");
    return this.api.post(`/api/Bookings/${id}/complete-booking`, {});
  }
}
