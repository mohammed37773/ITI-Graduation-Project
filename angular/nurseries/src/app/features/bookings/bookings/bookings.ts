import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Booking } from '../../../core/models/booking.model';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-bookings',
  imports:  [CommonModule, RouterLink ],
  templateUrl: './bookings.html',
  styleUrl: './bookings.css',
})
export class Bookings implements OnInit {
  
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5104/api/Bookings/my';

  allBookings = signal<any[]>([]);
  selectedFilter = signal<'all' | 'Pending' | 'Confirmed' | 'Cancelled'>('all');
  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);

  filteredBookings = computed(() => {
    const filter = this.selectedFilter();
    const bookings = this.allBookings();
    if (filter === 'all') return bookings;
    return bookings.filter(b => b.status === filter);
  });

  ngOnInit() {
    this.loadUserBookings();
  }

  loadUserBookings() {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.http.get<any[]>(this.apiUrl).subscribe({
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
