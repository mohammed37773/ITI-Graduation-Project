import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-manage-bookings',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './manage-bookings.html',
  styleUrl: './manage-bookings.css'
})
export class ManageBookings implements OnInit {
  private http = inject(HttpClient);
  backUrl = environment.backUrl;

  bookings = signal<any[]>([]);
  filteredBookings = signal<any[]>([]);
  isLoading = signal<boolean>(false);
  currentFilter = signal<string>('All'); // 'All' | 'Pending' | 'Confirmed' | 'Canceled'

  ngOnInit() {
    this.loadAllBookings();
  }

  loadAllBookings() {
    this.isLoading.set(true);
    // استبدل الـ URL بـ Endpoint الحجوزات الفعلي بتاعك في الـ .NET
    this.http.get<any[]>(this.backUrl + '/api/NurseryAdmin/bookings').subscribe({
      next: (data) => {
        this.bookings.set(data);
        this.applyFilter(this.currentFilter());
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('خطأ في جلب الحجوزات:', err);
        this.isLoading.set(false);
      }
    });
  }

  applyFilter(status: string) {
    this.currentFilter.set(status);
    if (status === 'All') {
      this.filteredBookings.set(this.bookings());
    } else {
      this.filteredBookings.set(this.bookings().filter(b => b.status === status));
    }
  }

  updateStatus(bookingId: number, newStatus: string) {
    // إرسال التحديث للباك إند لايف
    this.http.put(this.backUrl + `/api/NurseryAdmin/bookings/${bookingId}/status`, { status: newStatus }).subscribe({
      next: () => {
        // تحديث الداتا محلياً في الـ Signals فوراً لتوفير تجربة مستخدم سريعة لايف
        this.bookings.update(all => all.map(b => b.id === bookingId ? { ...b, status: newStatus } : b));
        this.applyFilter(this.currentFilter());
      },
      error: (err) => console.error('خطأ في تحديث الحالة:', err)
    });
  }
}