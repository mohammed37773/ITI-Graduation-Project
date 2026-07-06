import { Component, computed, inject, OnInit, signal, effect, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { Chart, registerables } from 'chart.js';
import { Booking } from '../../../core/models/booking.model';
import { environment } from '../../../../environments/environment';
import { BookingsService } from '../../../core/services/bookings';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private http = inject(HttpClient);
  private bookingService = inject(BookingsService);

  @ViewChild('statusChart') statusChartRef!: ElementRef;
  @ViewChild('earningsChart') earningsChartRef!: ElementRef;

  private statusChartInstance: Chart | null = null;
  private earningsChartInstance: Chart | null = null;

  private apiUrl = environment.backUrl + '/api/Bookings/owner-bookings';

  bookings = signal<Booking[]>([]);
  isLoading = signal<boolean>(true);

  // 1. حساب الحجوزات المعلقة
  pendingRequestsCount = computed(() => {
    return this.bookings().filter((b) => b.status === 'Pending').length;
  });

  // 2. حساب الأطفال النشطين حالياً (المؤكدة والمقبولة)
  activeBookingsCount = computed(() => {
    return this.bookings().filter((b) => b.status === 'Confirmed').length;
  });

  // 3. إجمالي الإيرادات المسحوبة من الحجوزات المؤكدة والمكتملة
  totalEarnings = computed(() => {
    return this.bookings()
      .filter((b) => b.status === 'Confirmed' || b.status === 'Completed')
      .reduce((sum, current) => sum + (current.totalPrice || 0), 0);
  });

  constructor() {
    // مراقبة التغيرات في الـ bookings وإعادة رسم الـ Charts تلقائياً بداتا حية
    effect(() => {
      const data = this.bookings();
      if (data.length > 0) {
        // تأخير بسيط للتأكد من أن الـ DOM تم رندمته والـ Canvas متاح
        setTimeout(() => this.updateCharts(), 50);
      }
    });
  }

  ngOnInit(): void {
    this.fetchDashboardData();
  }

  fetchDashboardData() {
    this.isLoading.set(true);
    this.http.get<Booking[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.bookings.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('خطأ أثناء جلب بيانات لوحة التحكم:', err);
        this.isLoading.set(false);
        this.bookings.set([]);
      },
    });
  }

  updateCharts() {
    const totalPending = this.pendingRequestsCount();
    const totalConfirmed = this.activeBookingsCount();
    const totalCompleted = this.bookings().filter(b => b.status === 'Completed').length;
    const totalCancelled = this.bookings().filter(b => b.status === 'Cancelled').length;

    // 1. رسم بياني لحالات الحجوزات (Pie Chart)
    if (this.statusChartRef) {
      if (this.statusChartInstance) this.statusChartInstance.destroy();
      this.statusChartInstance = new Chart(this.statusChartRef.nativeElement, {
        type: 'doughnut',
        data: {
          labels: ['معلق', 'مؤكد/نشط', 'مكتمل', 'ملغي'],
          datasets: [{
            data: [totalPending, totalConfirmed, totalCompleted, totalCancelled],
            backgroundColor: ['#ffc107', '#198754', '#0d6efd', '#dc3545'],
          }]
        },
        options: { responsive: true, plugins: { legend: { position: 'bottom', rtl: true } } }
      });
    }

    // 2. رسم بياني مقارنة الأرباح (Bar Chart)
    if (this.earningsChartRef) {
      if (this.earningsChartInstance) this.earningsChartInstance.destroy();
      this.earningsChartInstance = new Chart(this.earningsChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: ['إجمالي الأرباح المكتسبة (ج.م)'],
          datasets: [{
            label: 'الإيرادات',
            data: [this.totalEarnings()],
            backgroundColor: ['#20c997'],
            borderRadius: 8
          }]
        },
        options: { responsive: true, scales: { y: { beginAtZero: true } } }
      });
    }
  }

  // سيتم ربط هذه الأزرار بالإجراءات الفورية من السيرفر وعمل تحديث تلقائي للداتا
  handleAction(id: number, actionType: string) {
    // مثال للتعامل مع الإجراءات مباشرة من لوحة التحكم لتحديث الأرقام لايف
    let request;
    if (actionType === 'cancel') request = this.bookingService.cancel(id);
    if (actionType === "refund") { request = this.bookingService.refund(id)}
    if (actionType === "complete") {request = this.bookingService.complete(id)}
    
    if (request) {
      request.subscribe({
        next: () => this.fetchDashboardData(), // تحديث الجدول والـ Charts لايف فوراً
        error: (err) => console.error(err)
      });
    }
  }
}