import { CommonModule } from '@angular/common';
import { Component, inject,OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-new-booking',
  imports: [CommonModule, ReactiveFormsModule , RouterLink],
  templateUrl: './new-booking.html',
  styleUrl: './new-booking.css',
})
export class NewBooking {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  // Signals لإدارة حالة الصفحة
  nurseryId = signal<number | null>(null);
  nurseryName = signal<string>('');
  isLoading = signal<boolean>(false);
  isSubmitted = signal<boolean>(false);

  bookingForm!: FormGroup;

  // داتا لوكال سريعة عشان نعرض اسم الحضانة المحددة فوق الفورم كـ UX ممتاز
  private mockNurseries = [
    { id: 1, name: "حضانة الزهور السعيدة" },
    { id: 2, name: "حضانة المستقبل الذكي" },
    { id: 3, name: "حضانة عباقرة الغد" }
  ];

  ngOnInit() {
    // 1. قراءة الـ ID بتاع الحضانة من الـ URL
    const id = Number(this.route.snapshot.paramMap.get('nurseryId'));
    if (id) {
      this.nurseryId.set(id);
      const found = this.mockNurseries.find(n => n.id === id);
      this.nurseryName.set(found ? found.name : 'الحضانة المختارة');
    }

    // 2. بناء وتجهيز الـ Reactive Form مع الـ Validation
    this.initForm();
  }

  private initForm() {
    this.bookingForm = this.fb.group({
      childName: ['', [Validators.required, Validators.minLength(3)]],
      childAge: ['', [Validators.required, Validators.min(1), Validators.max(6)]],
      startDate: ['', [Validators.required]],
      notes: ['']
    });
  }

  // دالة الإرسال وفحص المدخلات
  onSubmitBooking() {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);

    // تجميع البيانات لإرسالها للـ API لاحقاً
    const bookingData = {
      nurseryId: this.nurseryId(),
      ...this.bookingForm.value,
      bookingDate: new Date()
    };

    console.log('بيانات الحجز الجاهزة للإرسال:', bookingData);

    // محاكاة إرسال للسيرفر لمدة ثانية
    setTimeout(() => {
      this.isLoading.set(false);
      this.isSubmitted.set(true);
      
      // تحويل الأب لصفحة "حجوزاتي" بعد نجاح الحجز (يمكنك تعديل المسار حسب مشروعك)
      setTimeout(() => {
  this.router.navigate(['/parent/bookings']); 
}, 2000);
    }, 1200);
  }
}
