import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { NurseryListItem } from '../../core/models/parent-nursery.model';

@Component({
  selector: 'app-nursery-owner',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './nursery-owner.html',
  styleUrl: './nursery-owner.css',
})
export class NurseryOwner {
  private authService = inject(AuthService);
  private router = inject(Router);
  public backUrl = environment.backUrl + "/api/Nurseries/owner/";
  private http = inject(HttpClient);


  isLoading = signal<boolean>(true);
  nursery = signal<NurseryListItem | null>(null);
  user = this.authService.currentUser();
  userName = this.user?.fullName || 'صاحب الحضانة';

  //   get (ownerId: string): Observable<any>{
  //   return this.api.get(`/api/Nurseries/owner/${ownerId}`)
  // }

    ngOnInit(): void {
        this.loadNurseryDetails();
      }
  
  
    // 1. جلب بيانات الحضانة الأساسية
    loadNurseryDetails() {
      this.isLoading.set(true);
      this.http.get<NurseryListItem>(this.backUrl + `${this.user?.id}`).subscribe({
        next: (data) => {
          this.nursery.set(data);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('خطأ في جلب تفاصيل الحضانة:', err);
          this.isLoading.set(false);
        },
      });
    }

  logout() {
    this.authService.logout(); // دالة مسح التوكن من الـ LocalStorage
    this.router.navigate(['/auth/login']);
}
}
