
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Booking } from '../../../core/models/booking.model';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';


@Component({
  selector: 'app-dashboard',
  standalone: true, 
  imports: [CommonModule , RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private http = inject(HttpClient);
  
  // الـ Base URL الموحد عندك في السيرفر
  private apiUrl = 'http://localhost:5104/api/Bookings/my';

  // Signal رئيسي لتخزين قائمة الحجوزات الخام القادمة من السيرفر
  bookings = signal<Booking[]>([]);
  isLoading = signal<boolean>(true);

  // 🔥 Computed Signals ذكية ومحسوبة تلقائياً فور تغير قائمة الحجوزات:
  
  // 1. حساب الطلبات المعلقة
  pendingRequestsCount = computed(() => {
    return this.bookings().filter(b => b.status === 'Pending').length;
  });

  // 2. حساب الأطفال المقبولين/النشطين بالحضانة
  activeBookingsCount = computed(() => {
    return this.bookings().filter(b => b.status === 'Confirmed' || b.status === 'Completed').length;
  });

  // 3. حساب إجمالي الإيرادات لايف من الحجوزات المؤكدة
  totalEarnings = computed(() => {
    return this.bookings()
      .filter(b => b.status === 'Confirmed' || b.status === 'Completed')
      .reduce((sum, current) => sum + (current.amount || 0), 0);
  });

  // 🔥 تصليح الـ Return Type هنا لـ void
  ngOnInit(): void {
    this.fetchDashboardData();
  }

  // دالة جلب البيانات لايف من قاعدة البيانات
  fetchDashboardData() {
    this.isLoading.set(true);
    
    // الـ Interceptor بتاعك هيرفق التوكن تلقائياً هنا في الخلفية
    this.http.get<Booking[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.bookings.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('خطأ أثناء جلب بيانات لوحة التحكم:', err);
        this.isLoading.set(false);
        // تعيين مصفوفة فارغة لتجنب ضرب الشاشة لو مفيش بيانات
        this.bookings.set([]); 
      }
    });
  }
}