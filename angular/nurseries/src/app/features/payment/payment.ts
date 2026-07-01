import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { switchMap, takeWhile } from 'rxjs/operators';
import {
  InitiatePaymentDto,
  PaymentResponse,
  PaymentStatusResponse,
} from '../../core/models/payment.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class Payment implements OnInit {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private baseUrl = environment.backUrl + '/api/payment';

  bookingId = signal<number>(0);
  paymentId = signal<number | null>(null);
  paymentUrl = signal<string | null>(null);

  isProcessing = signal<boolean>(false);
  paymentStatus = signal<
    'NotStarted' | 'Redirecting' | 'WaitingForCallback' | 'Completed' | 'Failed'
  >('NotStarted');
  errorMessage = signal<string | null>(null);

  private pollingSub?: Subscription;

  ngOnInit() {
    // لقط الـ bookingId من الـ URL
    const id = this.route.snapshot.paramMap.get('bookingId');
    if (id) {
      this.bookingId.set(+id);
    } else {
      this.errorMessage.set('رقم الحجز غير صحيح.');
    }
  }

  // الخطوة الأولى: إنشاء عملية الدفع وجلب رابط Paymob
  payWithMethod(methodType: number) {
    this.isProcessing.set(true);
    this.errorMessage.set(null);
    this.paymentStatus.set('Redirecting');

    const payload = {
      bookingId: this.bookingId(),
      amount: 100, // مثال على المبلغ، يجب تغييره
      method: methodType === 0 ? 'Card' : 'Wallet',
    };

    console.log(payload);

    const headers = this.getAuthHeaders();

    this.http.post<PaymentResponse>(`${this.baseUrl}/initiate`, payload, { headers }).subscribe({
      next: (res) => {
        this.paymentId.set(res.paymentId);
        this.paymentUrl.set(res.paymentUrl);

        window.open(res.paymentUrl, '_blank');

        this.paymentStatus.set('WaitingForCallback');

        this.startPollingStatus(res.paymentId);
      },
      error: (err) => {
        console.log(err.error);

        this.errorMessage.set(err.error?.message);

        this.paymentStatus.set('Failed');

        this.isProcessing.set(false);
      },
    });
  }

  // الخطوة الثانية: عمل Polling كل ثانيتين للتأكد من نجاح الدفع عبر الـ Webhook الخاص بالباك إند
  startPollingStatus(paymentId: number) {
    const headers = this.getAuthHeaders();

    this.pollingSub = interval(2000)
      .pipe(
        switchMap(() =>
          this.http.get<PaymentStatusResponse>(`${this.baseUrl}/${paymentId}/status`, { headers }),
        ),
        takeWhile((res) => res.status === 'Pending', true),
      )
      .subscribe({
        next: (res) => {
          if (res.status === 'Completed') {
            this.paymentStatus.set('Completed');
            this.isProcessing.set(false);
            this.pollingSub?.unsubscribe();
          } else if (res.status === 'Failed') {
            this.paymentStatus.set('Failed');
            this.isProcessing.set(false);
            this.pollingSub?.unsubscribe();
          }
        },
        error: (err) => {
          console.error('خطأ أثناء فحص حالة الدفع:', err);
        },
      });
  }

  ngOnDestroy() {
    if (this.pollingSub) {
      this.pollingSub.unsubscribe();
    }
  }

  goToBookings() {
    this.router.navigate(['/parent/bookings']);
  }
  private getAuthHeaders(): HttpHeaders {
    const sessionData = localStorage.getItem('user_session');
    let token = '';

    if (sessionData) {
      try {
        token = JSON.parse(sessionData).token || '';
      } catch (e) {
        console.error('خطأ في استخراج التوكن:', e);
      }
    }

    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
  }
}
