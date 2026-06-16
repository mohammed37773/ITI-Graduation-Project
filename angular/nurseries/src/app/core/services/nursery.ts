import { inject, Injectable } from '@angular/core';
import { NurseryModel } from '../models/nursery.model';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class Nursery {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:xxxx/api/nurseries'; 

  // جلب تفاصيل حضانة معينة بناءً على الـ ID
  getNurseryById(id: number): Observable<NurseryModel> {
    return this.http.get<NurseryModel>(`${this.apiUrl}/${id}`);
  }
}
