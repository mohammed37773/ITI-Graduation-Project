import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { NurseryListItem } from '../../../core/models/parent-nursery.model';
import { BookingsService } from '../../../core/services/bookings';
import { Booking } from '../../../core/models/booking.model';

@Component({
  selector: 'app-manage-bookings',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './manage-bookings.html',
  styleUrl: './manage-bookings.css'
})
export class ManageBookings implements OnInit {
  private http = inject(HttpClient);
  private bookingService = inject(BookingsService);

  nursery = signal<NurseryListItem | null>(null);
  nurseryId!: number;
  backUrl = environment.backUrl;

  bookings = signal<any[]>([]);
  filteredBookings = signal<any[]>([]);
  isLoading = signal<boolean>(false);
  currentFilter = signal<string>('All');

  // 🆕 Pagination
  currentPage = signal(1);
  pageSize = 6;

  totalPages = computed(() =>
    Math.max(1, Math.ceil(this.filteredBookings().length / this.pageSize))
  );

  pagedBookings = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.filteredBookings().slice(start, start + this.pageSize);
  });

  get pageNumbers(): number[] {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  ngOnInit() {
    this.loadAllBookings();
  }

  loadAllBookings() {
    this.isLoading.set(true);
    this.http.get<any[]>(this.backUrl + '/api/Bookings/owner-bookings').subscribe({
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
    this.currentPage.set(1); // 🆕 رجّعي لأول صفحة عند تغيير الفلتر
    if (status === 'All') {
      this.filteredBookings.set(this.bookings());
    } else {
      this.filteredBookings.set(this.bookings().filter(b => b.status === status));
    }
  }

  updateStatus(bookingId: number, newStatus: string) {
    this.http.put(this.backUrl + `/api/NurseryAdmin/bookings/${bookingId}/status`, { status: newStatus }).subscribe({
      next: () => {
        this.bookings.update(all => all.map(b => b.id === bookingId ? { ...b, status: newStatus } : b));
        this.applyFilter(this.currentFilter());
      },
      error: (err) => console.log()
    });
  }

  handleAction(bookingId: number, actionType: string) {
    if (actionType === "refund") {
      this.bookingService.refund(bookingId).subscribe({
        next: (r) => {
          console.log(r)
          this.updateStatus(bookingId, "Cancelled");
          this.loadAllBookings();
        },
        error: (r) => console.log(r)
      })
    }
    else if (actionType === "complete") {
      this.bookingService.complete(bookingId).subscribe({
        next: (r) => {
          console.log(r)
          this.updateStatus(bookingId, "Completed");
          this.loadAllBookings();
        },
        error: (r) => console.log(r)
      })
    }
  }

  isPastDate(bookingDate: string | Date): boolean {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return new Date(bookingDate) < today;
  }
}