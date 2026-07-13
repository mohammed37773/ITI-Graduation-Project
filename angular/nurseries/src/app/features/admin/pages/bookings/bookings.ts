import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BookingsService } from '../../../../core/services/bookings';

interface Booking {
  id: number;
  parentId: number;
  nurseryId: number;
  childId: number;
  startDate: string;
  status: string;
  totalPrice: number;
  createdAt: string;
}

@Component({
  selector: 'app-bookings',
  imports: [CommonModule, FormsModule],
  templateUrl: './bookings.html',
  styleUrl: './bookings.css',
})
export class Bookings implements OnInit {
  loading = signal(true);
  error = signal<string | null>(null);
  searchQuery = signal('');
  selectedStatus = signal('all');

  bookings = signal<Booking[]>([]);
  statsData = { total: 0, confirmed: 0, pending: 0, cancelled: 0, totalRevenue: 0 };

  // 🔢 محرك الـ Pagination المطور عبر الـ Signals والـ Computed
  currentPage = signal(1);
  pageSize = 10; // عدد الحجوزات المعروضة في كل صفحة

  constructor(private bookingsService: BookingsService) {}

  ngOnInit() {
    this.loadBookings();
    this.loadStats();
  }

  loadBookings() {
    this.loading.set(true);
    this.error.set(null);
    this.bookingsService.getAll().subscribe({
      next: (data: any) => {
        const rawList = Array.isArray(data) ? data : (data?.data ?? data?.bookings ?? []);
        this.bookings.set(rawList);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading bookings:', err);
        this.error.set('تعذر تحميل بيانات الحجوزات من السيرفر.');
        this.loading.set(false);
      }
    });
  }

  loadStats() {
    this.bookingsService.getStats().subscribe({
      next: (data: any) => {
        this.statsData = {
          total: data?.total ?? 0,
          confirmed: data?.confirmed ?? 0,
          pending: data?.pending ?? 0,
          cancelled: data?.cancelled ?? 0,
          totalRevenue: data?.totalRevenue ?? 0
        };
      },
      error: (err) => console.error('Error loading booking stats:', err)
    });
  }

  // مصفوفة الحجوزات المفلترة بالكامل (دون تجزئة أو قص)
  get filteredBookings(): Booking[] {
    return this.bookings().filter(b => {
      const q = this.searchQuery().toLowerCase().trim();
      const matchesSearch = !q || 
        String(b.id).includes(q) ||
        String(b.parentId).includes(q) ||
        String(b.nurseryId).includes(q);
        
      const status = this.selectedStatus();
      const matchesStatus = status === 'all' || b.status === status;
      
      return matchesSearch && matchesStatus;
    });
  }

  // ✂️ استخراج صفوف البيانات المخصصة للصفحة الحالية (Slicing Process)
  get pagedBookings(): Booking[] {
    const startIndex = (this.currentPage() - 1) * this.pageSize;
    return this.filteredBookings.slice(startIndex, startIndex + this.pageSize);
  }

  // حساب إجمالي عدد الصفحات ديناميكياً استناداً للبيانات المفلترة
  totalPages = computed(() => {
    const count = this.filteredBookings.length;
    return Math.ceil(count / this.pageSize) || 1;
  });

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  // إنشاء مصفوفة أرقام الصفحات لعرضها في الـ HTML
  getPageArray(): number[] {
    const total = this.totalPages();
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  onSearchChange(value: string) {
    this.searchQuery.set(value);
    this.currentPage.set(1); // العودة للصفحة الأولى عند كتابة بحث جديد
  }

  setStatus(s: string) {
    this.selectedStatus.set(s);
    this.currentPage.set(1); // العودة للصفحة الأولى عند تبديل حالة الفلترة
  }

  get stats() {
    return {
      total: this.statsData.total || this.bookings().length,
      confirmed: this.statsData.confirmed || this.bookings().filter(b => b.status === 'Confirmed').length,
      pending: this.statsData.pending || this.bookings().filter(b => b.status === 'Pending').length,
      cancelled: this.statsData.cancelled || this.bookings().filter(b => b.status === 'Cancelled').length,
      totalRevenue: this.statsData.totalRevenue
    };
  }

  getStatusClass(status: string): string {
    if (status === 'Confirmed' || status === 'مؤكد') return 'badge-success';
    if (status === 'Pending' || status === 'معلق') return 'badge-warning';
    if (status === 'Completed' || status === 'مكتمل') return 'badge-info';
    return 'badge-error';
  }

  getStatusLabel(status: string): string {
    if (status === 'Confirmed') return 'مؤكد';
    if (status === 'Pending') return 'معلق';
    if (status === 'Cancelled') return 'ملغى';
    if (status === 'Completed') return 'مكتمل';
    return status;
  }

  formatDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('ar-EG', { year: 'numeric', month: 'short', day: 'numeric' });
  }

  // دالة مساعدة لاستخدام Math.min داخل قالب الـ HTML بأمان
  mathMin(a: number, b: number): number {
    return Math.min(a, b);
  }
}