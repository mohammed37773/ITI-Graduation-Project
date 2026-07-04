import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentsService } from '../../../../core/services/payments';

interface Payment {
  id: number;
  bookingId: number;
  parentId: number;
  amount: number;
  method: string;
  status: string;
  transactionId: string;
  gatewayOrderId: string;
  paymentUrl: string;
  paidAt: string;
  createdAt: string;
}

interface PaymentStats {
  total: number;
  completed: number;
  failed: number;
  refunded: number;
  totalRevenue: number;
  byMethod: {
    vodafoneCash: number;
    meeza: number;
    card: number;
    payPal: number;
  };
}

@Component({
  selector: 'app-payments',
  imports: [CommonModule, FormsModule],
  templateUrl: './payments.html',
  styleUrl: './payments.css',
})
export class Payments implements OnInit {
  loading = signal(true);
  error = signal<string | null>(null);
  searchQuery = signal('');
  selectedStatus = signal('all');
  refundLoading = signal<boolean>(false);
  refundLoadingPaymentId = signal<number | null>(null);

  payments = signal<Payment[]>([]);
  statsData: PaymentStats = {
    total: 0,
    completed: 0,
    failed: 0,
    refunded: 0,
    totalRevenue: 0,
    byMethod: { vodafoneCash: 0, meeza: 0, card: 0, payPal: 0 },
  };

  constructor(private paymentsService: PaymentsService) {}

  ngOnInit() {
    this.loadPayments();
    this.loadStats();
  }

  loadPayments() {
    this.loading.set(true);
    this.error.set(null);
    this.paymentsService.getAll().subscribe({
      next: (data: any) => {
        this.payments.set(Array.isArray(data) ? data : (data?.data ?? data?.payments ?? []));
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading payments:', err);
        this.error.set('تعذر تحميل المدفوعات');
        this.loading.set(false);
      },
    });
  }

  loadStats() {
    this.paymentsService.getStats().subscribe({
      next: (data: any) => {
        this.statsData = {
          total: data?.total ?? 0,
          completed: data?.completed ?? 0,
          failed: data?.failed ?? 0,
          refunded: data?.refunded ?? 0,
          totalRevenue: data?.totalRevenue ?? 0,
          byMethod: {
            vodafoneCash: data?.byMethod?.vodafoneCash ?? 0,
            meeza: data?.byMethod?.meeza ?? 0,
            card: data?.byMethod?.card ?? 0,
            payPal: data?.byMethod?.payPal ?? 0,
          },
        };
      },
      error: (err) => console.error('Error loading payment stats:', err),
    });
  }

  get filteredPayments(): Payment[] {
    return this.payments().filter((p) => {
      const q = this.searchQuery().toLowerCase();
      const matchesSearch =
        String(p.id).includes(q) ||
        String(p.bookingId).includes(q) ||
        String(p.parentId).includes(q) ||
        p.transactionId?.toLowerCase().includes(q);
      const matchesStatus =
        this.selectedStatus() === 'all' || p.status?.toLowerCase() === this.selectedStatus();
      return matchesSearch && matchesStatus;
    });
  }

  get stats() {
    return {
      total: this.statsData.total || this.payments().length,
      completed:
        this.statsData.completed || this.payments().filter((p) => p.status === 'Completed').length,
      failed: this.statsData.failed || this.payments().filter((p) => p.status === 'Failed').length,
      refunded:
        this.statsData.refunded || this.payments().filter((p) => p.status === 'Refunded').length,
      totalRevenue: this.statsData.totalRevenue,
      byMethod: this.statsData.byMethod,
    };
  }

  refund(id: number) {
    this.refundLoading.set(true);
    this.refundLoadingPaymentId.set(id);
    this.paymentsService.refund(id).subscribe({
      next: () => {
        const p = this.payments().find((x) => x.id === id);
        if (p) p.status = 'Refunded';
        this.payments.set([...this.payments()]); // Trigger change detection
        this.refundLoading.set(false);
        this.refundLoadingPaymentId.set(null);
      },
      error: (err) => {
        console.error('Error refunding payment:', err);
        this.refundLoading.set(false);
        this.refundLoadingPaymentId.set(null);
      },
    });
  }

  getStatusClass(status: string): string {
    if (status === 'Completed') return 'badge-success';
    if (status === 'Pending') return 'badge-warning';
    if (status === 'Refunded') return 'badge-neutral';
    return 'badge-error';
  }

  getStatusLabel(status: string): string {
    if (status === 'Completed') return 'مكتمل';
    if (status === 'Pending') return 'معلق';
    if (status === 'Refunded') return 'مسترد';
    if (status === 'Failed') return 'فشل';
    return status;
  }

  getMethodLabel(method: string): string {
    if (method === 'VodafoneCash' || method === 'vodafoneCash') return 'فودافون كاش';
    if (method === 'Meeza' || method === 'meeza') return 'ميزة';
    if (method === 'Card' || method === 'card') return 'بطاقة';
    if (method === 'PayPal' || method === 'payPal') return 'باي بال';
    return method;
  }

  setStatus(s: string) {
    this.selectedStatus.set(s);
  }

  formatDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('ar-EG', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }
}
