import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Booking } from '../../../core/models/booking.model';

@Component({
  selector: 'app-bookings',
  imports:  [CommonModule, RouterLink ],
  templateUrl: './bookings.html',
  styleUrl: './bookings.css',
})
export class Bookings implements OnInit {
  
  // الـ Signals الأساسية لإدارة حالة الصفحة والتبويب الحالي
  allBookings = signal<Booking[]>([]);
  selectedFilter = signal<'all' | 'pending' | 'approved' | 'rejected'>('all');
  isLoading = signal<boolean>(true);

  // 🔥 Computed Signal: بيعمل فیلتر للداتا تلقائياً أول ما الـ selectedFilter يتغير لايف
  filteredBookings = computed(() => {
    const filter = this.selectedFilter();
    const bookings = this.allBookings();
    
    if (filter === 'all') return bookings;
    return bookings.filter(b => b.status === filter);
  });

  ngOnInit() {
    this.loadUserBookings();
  }

  // محاكاة جلب الحجوزات الخاصة بولي الأمر من السيرفر
  loadUserBookings() {
    this.isLoading.set(true);
    
    setTimeout(() => {
      this.allBookings.set([
        {
          id: 101,
          nurseryId: 1,
          nurseryName: "حضانة الزهور السعيدة",
          childName: "يوسف محمد",
          childAge: 3,
          startDate: new Date('2026-07-01'),
          status: 'approved',
          dailyPrice: 50
        },
        {
          id: 102,
          nurseryId: 2,
          nurseryName: "حضانة المستقبل الذكي",
          childName: "سجده محمد",
          childAge: 5,
          startDate: new Date('2026-07-15'),
          status: 'pending',
          dailyPrice: 120
        },
        {
          id: 103,
          nurseryId: 3,
          nurseryName: "حضانة عباقرة الغد",
          childName: "يوسف محمد",
          childAge: 3,
          startDate: new Date('2026-06-10'),
          status: 'rejected',
          dailyPrice: 80
        }
      ]);
      this.isLoading.set(false);
    }, 600); // لودنج خفيف نص ثانية
  }

  // دالة لتغيير التبويب الحالي
  changeFilter(filterType: 'all' | 'pending' | 'approved' | 'rejected') {
    this.selectedFilter.set(filterType);
  }
}
