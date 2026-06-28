
import { Component, OnInit, signal } from '@angular/core';
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
  imports: [CommonModule,FormsModule],
  templateUrl: './bookings.html',
  styleUrl: './bookings.css',
})
export class Bookings implements OnInit {
  loading = signal(true);
  error = signal<string | null>(null);
  searchQuery = signal('');
  selectedStatus = signal('all');

  bookings: Booking[] = [];
  statsData = { total: 0, confirmed: 0, pending: 0, cancelled: 0, totalRevenue: 0 };

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
        this.bookings = Array.isArray(data) ? data : (data?.data ?? data?.bookings ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading bookings:', err);
        this.error.set('تعذر تحميل الحجوزات');
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

  get filteredBookings(): Booking[] {
    return this.bookings.filter(b => {
      const q = this.searchQuery().toLowerCase();
      const matchesSearch = String(b.id).includes(q) ||
        String(b.parentId).includes(q) ||
        String(b.nurseryId).includes(q);
      const matchesStatus = this.selectedStatus() === 'all' || b.status === this.selectedStatus();
      return matchesSearch && matchesStatus;
    });
  }

  get stats() {
    return {
      total: this.statsData.total || this.bookings.length,
      confirmed: this.statsData.confirmed || this.bookings.filter(b => b.status === 'Confirmed').length,
      pending: this.statsData.pending || this.bookings.filter(b => b.status === 'Pending').length,
      cancelled: this.statsData.cancelled || this.bookings.filter(b => b.status === 'Cancelled').length,
      totalRevenue: this.statsData.totalRevenue
    };
  }

  getStatusClass(status: string): string {
    if (status === 'Confirmed' || status === 'مؤكد') return 'badge-success';
    if (status === 'Pending' || status === 'معلق') return 'badge-warning';
    return 'badge-error';
  }

  getStatusLabel(status: string): string {
    if (status === 'Confirmed') return 'مؤكد';
    if (status === 'Pending') return 'معلق';
    if (status === 'Cancelled') return 'ملغى';
    return status;
  }

  setStatus(s: string) { this.selectedStatus.set(s); }

  formatDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('ar-EG', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
