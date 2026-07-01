import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class Nursery {
  private http = inject(HttpClient);
  private apiUrl = environment.backUrl + '/api/nurseries'; 

  // جلب تفاصيل حضانة معينة بناءً على الـ ID
  getNurseryById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  getNurseryByOwnerId(ownerId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${ownerId}`);
  }
}
