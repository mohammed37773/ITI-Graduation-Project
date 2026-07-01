import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  constructor(private api: ApiService) {}

  getSummary(): Observable<any> {
    return this.api.get('/api/admin/reports/summary');
  }
}
