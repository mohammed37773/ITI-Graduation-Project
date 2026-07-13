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

  // 🎨 هوية Owner (كحلي + ذهبي)
  private colors = {
    navy: '#1B2A41',
    navyLight: '#2E4160',
    gold: '#D9A441',
    terracotta: '#C1694F',
    danger: '#D9756B',
    grid: '#E7EAF0',
  };

  bookings = signal<Booking[]>([]);
  isLoading = signal<boolean>(true);

  // 🆕 Pagination
  currentPage = signal(1);
  pageSize = 5;

  totalPages = computed(() =>
    Math.max(1, Math.ceil(this.bookings().length / this.pageSize))
  );

  pagedBookings = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.bookings().slice(start, start + this.pageSize);
  });

  get pageNumbers(): number[] {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  // إحصائيات
  pendingRequestsCount = computed(() => {
    return this.bookings().filter((b) => b.status === 'Pending').length;
  });

  activeBookingsCount = computed(() => {
    return this.bookings().filter((b) => b.status === 'Confirmed').length;
  });

  totalEarnings = computed(() => {
    return this.bookings()
      .filter((b) => b.status === 'Confirmed' || b.status === 'Completed')
      .reduce((sum, current) => sum + (current.totalPrice || 0), 0);
  });

  constructor() {
    effect(() => {
      const data = this.bookings();
      if (data.length > 0) {
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
        this.currentPage.set(1); // رجّعي لأول صفحة عند كل تحديث
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

    // 1️⃣ توزيع حالات الحجوزات (Doughnut) - بهوية كحلي/ذهبي
    if (this.statusChartRef) {
      if (this.statusChartInstance) this.statusChartInstance.destroy();
      this.statusChartInstance = new Chart(this.statusChartRef.nativeElement, {
        type: 'doughnut',
        data: {
          labels: ['معلق', 'مؤكد/نشط', 'مكتمل', 'ملغي'],
          datasets: [{
            data: [totalPending, totalConfirmed, totalCompleted, totalCancelled],
            backgroundColor: [this.colors.terracotta, this.colors.navy, this.colors.gold, this.colors.danger],
            borderWidth: 3,
            borderColor: '#ffffff',
            hoverOffset: 8,
          }]
        },
        options: {
          responsive: true,
          cutout: '68%',
          animation: { animateRotate: true, duration: 800, easing: 'easeOutQuart' },
          plugins: {
            legend: {
              position: 'bottom',
              rtl: true,
              labels: {
                font: { family: 'Cairo', size: 12, weight: 600 },
                color: this.colors.navy,
                padding: 16,
                usePointStyle: true,
                pointStyle: 'circle',
              }
            },
            tooltip: {
              rtl: true,
              backgroundColor: this.colors.navy,
              titleFont: { family: 'Cairo' },
              bodyFont: { family: 'Cairo' },
              padding: 12,
              cornerRadius: 8,
            }
          }
        }
      });
    }

    // 2️⃣ مؤشر الأرباح (Bar) - ذهبي متدرج
    if (this.earningsChartRef) {
      if (this.earningsChartInstance) this.earningsChartInstance.destroy();

      const ctx = this.earningsChartRef.nativeElement.getContext('2d');
      const gradient = ctx.createLinearGradient(0, 0, 0, 250);
      gradient.addColorStop(0, this.colors.gold);
      gradient.addColorStop(1, '#F0CE8E');

      this.earningsChartInstance = new Chart(this.earningsChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: ['إجمالي الأرباح المكتسبة (ج.م)'],
          datasets: [{
            label: 'الإيرادات',
            data: [this.totalEarnings()],
            backgroundColor: gradient,
            borderRadius: 10,
            maxBarThickness: 70,
          }]
        },
        options: {
          responsive: true,
          animation: { duration: 900, easing: 'easeOutQuart' },
          plugins: {
            legend: { display: false },
            tooltip: {
              rtl: true,
              backgroundColor: this.colors.navy,
              titleFont: { family: 'Cairo' },
              bodyFont: { family: 'Cairo' },
              padding: 12,
              cornerRadius: 8,
            }
          },
          scales: {
            y: {
              beginAtZero: true,
              grid: { color: this.colors.grid },
              ticks: { font: { family: 'Cairo' }, color: this.colors.navy }
            },
            x: {
              grid: { display: false },
              ticks: { font: { family: 'Cairo' }, color: this.colors.navy }
            }
          }
        }
      });
    }
  }

  handleAction(id: number, actionType: string) {
    let request;
    if (actionType === 'cancel') request = this.bookingService.cancel(id);
    if (actionType === "refund") { request = this.bookingService.refund(id)}
    if (actionType === "complete") {request = this.bookingService.complete(id)}

    if (request) {
      request.subscribe({
        next: () => this.fetchDashboardData(),
        error: (err) => console.error(err)
      });
    }
  }
}