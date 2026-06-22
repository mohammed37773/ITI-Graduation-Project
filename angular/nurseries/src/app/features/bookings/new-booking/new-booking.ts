import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-new-booking',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './new-booking.html',
  styleUrl: './new-booking.css',
})
export class NewBooking implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);

  // الـ Base URL المعتمد
  private baseUrl = 'http://localhost:5104/api';

  nurseryId = signal<number | null>(null);
  nurseryName = signal<string>('جاري تحميل اسم الحضانة...');
  myChildren = signal<any[]>([]);
  
  isLoading = signal<boolean>(false);
  isSubmitted = signal<boolean>(false);
  isWaitingForPayment = signal<boolean>(false); 
  errorMessage = signal<string | null>(null);

  bookingForm!: FormGroup;
  pollingInterval: any; 
  createdBookingId: number | null = null; 

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('nurseryId'));
    if (id) {
      this.nurseryId.set(id);
      this.loadNurseryDetails(id);
    }
    this.initForm();
    this.loadMyChildren();
  }

  private initForm() {
    this.bookingForm = this.fb.group({
      childId: ['', [Validators.required]],
      startDate: ['', [Validators.required]]
    });
  }

  private loadNurseryDetails(id: number) {
    // 🟢 تعديل الحرف لـ nurseries صغيرة عشان يطابق الـ Swagger
    this.http.get<any>(`${this.baseUrl}/nurseries/${id}`).subscribe({
      next: (nursery) => this.nurseryName.set(nursery?.name || 'الحضانة المختارة'),
      error: () => this.nurseryName.set('حضانة مسجلة بالنظام')
    });
  }

  private loadMyChildren() {
    // 🟢 تعديل الحرف لـ children صغيرة
    this.http.get<any[]>(`${this.baseUrl}/children`).subscribe({
      next: (data) => this.myChildren.set(data || []),
      error: () => this.errorMessage.set('فشل في تحميل قائمة أطفالك.')
    });
  }

  onSubmitBooking() {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const bookingPayload = {
      nurseryId: this.nurseryId(),
      childId: parseInt(this.bookingForm.value.childId),
      startDate: this.bookingForm.value.startDate
    };

    // 1️⃣ الخطوة الأولى: إنشاء الحجز (/api/bookings) بحروف صغيرة بالملي
    this.http.post<any>(`${this.baseUrl}/bookings`, bookingPayload).subscribe({
      next: (bookingRes) => {
        // لقط الـ ID الراجع من السيرفر
        this.createdBookingId = bookingRes.id || bookingRes.bookingId;
        
        // 2️⃣ الخطوة الثانية: طلب الدفع من الـ Endpoint الصح (/api/payment/initiate)
        const paymentPayload = {
          bookingId: this.createdBookingId,
          paymentMethod: 0 // 0 للـ Card كـ افتراضي
        };

        this.http.post<any>(`${this.baseUrl}/payment/initiate`, paymentPayload).subscribe({
          next: (paymentRes) => {
            this.isLoading.set(false);
            
            if (paymentRes?.paymentUrl) {
              window.open(paymentRes.paymentUrl, '_blank');
              this.isWaitingForPayment.set(true);
              this.startPaymentPolling(paymentRes.paymentId || paymentRes.id); 
            } else {
              this.isSubmitted.set(true);
              setTimeout(() => this.router.navigate(['/parent/bookings']), 2000);
            }
          },
          error: (paymentErr) => {
            this.isLoading.set(false);
            this.errorMessage.set(paymentErr.error?.message || 'فشل في تهيئة بوابة دفع Paymob.');
          }
        });
      },
      error: (bookingErr) => {
        this.isLoading.set(false);
        this.errorMessage.set(bookingErr.error?.message || 'حدث خطأ أثناء إرسال طلب الحجز، يرجى التحقق من السيرفر.');
      }
    });
  }

  private startPaymentPolling(paymentId: number) {
    this.pollingInterval = setInterval(() => {
      // 🟢 فحص حالة الدفع على المسار الصغير المعتمد بالـ Guide
      this.http.get<any>(`${this.baseUrl}/payment/${paymentId}/status`).subscribe({
        next: (paymentStatusRes) => {
          if (paymentStatusRes.status === 'Completed' || paymentStatusRes.status === 1) {
            clearInterval(this.pollingInterval);
            this.isWaitingForPayment.set(false);
            this.isSubmitted.set(true);
            setTimeout(() => this.router.navigate(['/parent/bookings']), 2500);
          } else if (paymentStatusRes.status === 'Failed') {
            clearInterval(this.pollingInterval);
            this.isWaitingForPayment.set(false);
            this.errorMessage.set('فشلت عملية الدفع عبر Paymob، يرجى إعادة المحاولة.');
          }
        },
        error: (err) => console.error('خطأ أثناء فحص الـ Webhook المالي:', err)
      });
    }, 2000);
  }

  ngOnDestroy(): void {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  }
}