
import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsService } from '../../../../core/services/reports';

interface ReportSummary {
  totalRevenue: number;
  totalBookings: number;
  totalNurseries: number;
  verifiedNurseries: number;
  thisMonth: {
    revenue: number;
    bookings: number;
  };
}

@Component({
  selector: 'app-reports',
  imports: [CommonModule],
  templateUrl: './reports.html',
  styleUrl: './reports.css',
})
export class Reports implements OnInit {
  loading = signal(true);
  error = signal<string | null>(null);
  summary = signal<ReportSummary | null>(null);

  constructor(private reportsService: ReportsService) {}

  ngOnInit() {
    this.reportsService.getSummary().subscribe({
      next: (data: any) => {
        this.summary.set({
          totalRevenue: data?.totalRevenue ?? 0,
          totalBookings: data?.totalBookings ?? 0,
          totalNurseries: data?.totalNurseries ?? 0,
          verifiedNurseries: data?.verifiedNurseries ?? 0,
          thisMonth: {
            revenue: data?.thisMonth?.revenue ?? 0,
            bookings: data?.thisMonth?.bookings ?? 0
          }
        });
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading report summary:', err);
        this.error.set('تعذر تحميل ملخص التقارير');
        this.loading.set(false);
      }
    });
  }
}

