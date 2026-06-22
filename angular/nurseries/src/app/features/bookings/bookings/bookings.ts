import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Booking } from '../../../core/models/booking.model';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './bookings.html',
  styleUrl: './bookings.css',
})
export class Bookings implements OnInit {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5104/api/Bookings';

  allBookings = signal<Booking[]>([]);
  selectedFilter = signal<'all' | 'Pending' | 'Confirmed' | 'Cancelled'>('all');
  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);

  // 🟢 متغير ذكي عشان نخزن فيه ID أول حضانة للحجز الجديد بدون ما يضرب الـ HTML
  firstNurseryId = computed(() => {
    const bookings = this.allBookings();
    return bookings.length > 0 ? bookings[0].nurseryId : 1; // لو مفيش حجوزات هيفترض 1 كـ default أو يفتح الفورم
  });

  filteredBookings = computed(() => {
    const filter = this.selectedFilter();
    const bookings = this.allBookings();
    
    if (filter === 'all') return bookings;
    
    return bookings.filter(b => {
      if (typeof b.status === 'number') {
        const statusMap: { [key: number]: string } = { 0: 'Pending', 1: 'Confirmed', 2: 'Cancelled' };
        return statusMap[b.status] === filter;
      }
      return b.status?.toString().toLowerCase() === filter.toLowerCase();
    });
  });

  ngOnInit() {
    this.loadUserBookings();
  }

  loadUserBookings() {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.http.get<Booking[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.allBookings.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'حدث خطأ أثناء تحميل الحجوزات.');
        this.isLoading.set(false);
      }
    });
  }

  changeFilter(filterType: 'all' | 'Pending' | 'Confirmed' | 'Cancelled') {
    this.selectedFilter.set(filterType);
  }
}