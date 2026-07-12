import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Booking } from '../../../core/models/booking.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './bookings.html',
  styleUrl: './bookings.css',
})
export class Bookings implements OnInit {
  private http = inject(HttpClient);
  private apiUrl = environment.backUrl + '/api/Bookings';

  allBookings = signal<Booking[]>([]);
  selectedFilter = signal<'all' | 'Pending' | 'Confirmed' | 'Cancelled'>('all');
  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);

  // 🆕 Pagination state
  currentPage = signal(1);
  pageSize = 6;

  firstNurseryId = computed(() => {
    const bookings = this.allBookings();
    return bookings.length > 0 ? bookings[0].nurseryId : 1;
  });

  // ده المصدر الكامل بعد الفلترة (من غير تقسيم صفحات) - نفس منطقك الأصلي
  allFilteredBookings = computed(() => {
    const filter = this.selectedFilter();
    const bookings = this.allBookings();

    if (filter === 'all') return bookings;

    return bookings.filter((b) => {
      if (typeof b.status === 'number') {
        const statusMap: { [key: number]: string } = {
          0: 'Pending',
          1: 'Confirmed',
          2: 'Cancelled',
        };
        return statusMap[b.status] === filter;
      }
      return b.status?.toString().toLowerCase() === filter.toLowerCase();
    });
  });

  // 🆕 عدد الصفحات بناءً على النتائج المفلترة
  totalPages = computed(() =>
    Math.max(1, Math.ceil(this.allFilteredBookings().length / this.pageSize))
  );

  // 🆕 ده اللي بيتعرض فعليًا في الجدول (صفحة واحدة بس)
  filteredBookings = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.allFilteredBookings().slice(start, start + this.pageSize);
  });

  // 🆕 قائمة أرقام الصفحات لعرضها في الـ pagination
  get pageNumbers(): number[] {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  }

  ngOnInit() {
    this.loadUserBookings();
  }

  loadUserBookings() {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    // 1️⃣ سحب التوكن من الـ user_session لتأمين الطلب
    const sessionData = localStorage.getItem('user_session');
    let token = '';
    if (sessionData) {
      try {
        token = JSON.parse(sessionData).token || '';
      } catch (e) {
        console.error('خطأ في قراءة الـ session', e);
      }
    }

    // 2️⃣ إعداد الـ Headers
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });

    // 3️⃣ تزويد الـ URL بـ /my (حل مشكلة 405) وتمرير الـ headers (حل مشكلة 401)
    this.http.get<Booking[]>(`${this.apiUrl}/my`, { headers }).subscribe({
      next: (data) => {
        console.log('بيانات الحجوزات:', data);
        this.allBookings.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'حدث خطأ أثناء تحميل الحجوزات.');
        this.isLoading.set(false);
      },
    });
  }

  // 🆕 مدمجة: فلترة + رجوع لأول صفحة تلقائيًا
  changeFilter(filterType: 'all' | 'Pending' | 'Confirmed' | 'Cancelled') {
    this.selectedFilter.set(filterType);
    this.currentPage.set(1);
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }
}