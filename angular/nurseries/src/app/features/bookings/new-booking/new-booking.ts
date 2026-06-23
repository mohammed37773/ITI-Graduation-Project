import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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

  // الـ Base URL المعتمد للباك إند
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

  // 🔐 دالة مساعدة لإنشاء الـ Headers بالتوكن الصحيح فريش
  private getAuthHeaders(): HttpHeaders {
    const sessionData = localStorage.getItem('user_session');
    let token = '';
    if (sessionData) {
      try {
        token = JSON.parse(sessionData).token || '';
      } catch (e) {
        console.error('خطأ في استخراج التوكن جوه الحجز:', e);
      }
    }
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  private loadNurseryDetails(id: number) {
    const headers = this.getAuthHeaders();
    this.http.get<any>(`${this.baseUrl}/nurseries/${id}`, { headers }).subscribe({
      next: (nursery) => this.nurseryName.set(nursery?.name || 'الحضانة المختارة'),
      error: () => this.nurseryName.set('حضانة مسجلة بالنظام')
    });
  }

  private loadMyChildren() {
    const headers = this.getAuthHeaders();
    this.http.get<any[]>(`${this.baseUrl}/children`, { headers }).subscribe({
      next: (data) => this.myChildren.set(data || []),
      error: () => this.errorMessage.set('فشل في تحميل قائمة أطفالك، يرجى إعادة تسجيل الدخول.')
    });
  }

 onSubmitBooking() {
  if (this.bookingForm.invalid) {
    this.bookingForm.markAllAsTouched();
    return;
  }
  this.isLoading.set(true);
  this.errorMessage.set(null);

  const headers = this.getAuthHeaders();
  const formValue = this.bookingForm.value;

  // 1️⃣ الطريقة الأضمن لقطع التاريخ للنص الصافي (YYYY-MM-DD) المتوافق مع الـ DateOnly
  // بنضمن إننا بنتعامل مع التاريخ كـ نص نقي عشان الـ .NET binder ميحصلش فيه لغبطة
  const dateInput = formValue.startDate; // الصيغة الجاية من الـ HTML Input بتكون "2026-06-23" مثلاً
  let dateOnlyString = '';

  if (dateInput instanceof Date) {
    const year = dateInput.getFullYear();
    const month = String(dateInput.getMonth() + 1).padStart(2, '0');
    const day = String(dateInput.getDate()).padStart(2, '0');
    dateOnlyString = `${year}-${month}-${day}`;
  } else {
    // لو جاي كـ string من الـ input (وده الغالب في Angular الكلاسيكي) بنقصه لحد أول 10 حروف بس
    dateOnlyString = String(dateInput).substring(0, 10);
  }

  // 2️⃣ بناء الـ Payload مع التأكيد التام على الـ CamelCase المطلوبة في الـ Swagger
  const bookingPayload = {
    nurseryId: Number(this.nurseryId()!),
    childId: Number(formValue.childId),
    startDate: dateOnlyString // النص الصافي تماماً "YYYY-MM-DD"
  };

  console.log('🚀 Sending Pure Payload:', bookingPayload);

  // 3️⃣ إرسال الطلب
  this.http.post<any>(`${this.baseUrl}/bookings`, bookingPayload, { headers }).subscribe({
    next: (bookingRes) => {
      console.log('✅ تم إنشاء الحجز بنجاح في السيرفر:', bookingRes);
      
      // استقبال الـ ID (السيرفر بيرجع الـ الحجز بالكامل فبنلقط الـ id الصغير)
      this.createdBookingId = bookingRes.id;
      this.proceedToPayment(this.createdBookingId!);
    },
    error: (bookingErr) => {
      this.isLoading.set(false);
      console.error('❌ تفاصيل خطأ الـ الباك إند بالكامل:', bookingErr);
      
      // سحب رسالة الخطأ الموحدة اللي الـ Middleware بيرجعها بالعربي
      if (bookingErr.error && bookingErr.error.message) {
        this.errorMessage.set(bookingErr.error.message);
      } else if (typeof bookingErr.error === 'string') {
        this.errorMessage.set(bookingErr.error);
      } else if (bookingErr.error && bookingErr.error.errors) {
        // لو الـ خطأ جاي من الـ Validation بتاعة الـ .NET نفسه (بدون ما يوصل للـ Controller)
        const validationErrors = bookingErr.error.errors;
        console.error('🚫 .NET Validation Details:', validationErrors);
        this.errorMessage.set('تأكد من اختيار طفل وتاريخ حجز صحيح متوافق مع سن الطفل.');
      } else {
        this.errorMessage.set('حدث خطأ أثناء إرسال طلب الحجز، يرجى مراجعة البيانات.');
      }
    }
  });
}

// 💳 دالة الدفع المنفصلة والمؤمنة
private proceedToPayment(bookingId: number) {
  const headers = this.getAuthHeaders();
  const paymentPayload = {
    bookingId: Number(bookingId),
    paymentMethod: 0 // 0 للكارت كـ افتراضي حسب الـ Swagger
  };

  this.http.post<any>(`${this.baseUrl}/payment/initiate`, paymentPayload, { headers }).subscribe({
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
}

  private startPaymentPolling(paymentId: number) {
    const headers = this.getAuthHeaders();
    
    this.pollingInterval = setInterval(() => {
      // فحص حالة الدفع كل ثانيتين بمد الـ Headers المطلوبة بالتوكن
      this.http.get<any>(`${this.baseUrl}/payment/${paymentId}/status`, { headers }).subscribe({
        next: (paymentStatusRes) => {
          // التعامل مع الـ Status سواء كانت String (Completed) أو Enum رقمي (1) لتأمين الرد
          if (paymentStatusRes.status === 'Completed' || paymentStatusRes.status === 1) {
            clearInterval(this.pollingInterval);
            this.isWaitingForPayment.set(false);
            this.isSubmitted.set(true);
            setTimeout(() => this.router.navigate(['/parent/bookings']), 2500);
          } else if (paymentStatusRes.status === 'Failed' || paymentStatusRes.status === 3) {
            clearInterval(this.pollingInterval);
            this.isWaitingForPayment.set(false);
            this.errorMessage.set('فشلت عملية الدفع عبر Paymob، يرجى إعادة المحاولة أو مراجعة الرصيد.');
          }
        },
        error: (err) => console.error('جاري انتظار الـ Webhook المالي لتأكيد الحجز من السيرفر...', err)
      });
    }, 2000);
  }

  ngOnDestroy(): void {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  }
}