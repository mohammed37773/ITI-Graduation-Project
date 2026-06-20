import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth';
import { Injectable, inject } from '@angular/core'; 
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { RegisterDto } from '../../../../core/models/authModel';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class Register {
 private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  // Signals للتحكم في حالة الشاشة
  isLoading = signal<boolean>(false);
  showOtpModal = signal<boolean>(false); // بيتحكم في ظهور صندوق الـ OTP
  errorMessage = signal<string | null>(null);
  registeredEmail = signal<string>(''); // لحفظ الإيميل عشان نبعته مع الـ OTP

  registerForm!: FormGroup;
  otpForm!: FormGroup;

  constructor() {
    this.initForms();
  }

  private initForms() {
  this.registerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Parent', [Validators.required]] // القيمة الافتراضية Parent وتتغير حسب الاختيار
  });

  this.otpForm = this.fb.group({
    otpCode: ['', [Validators.required, Validators.minLength(4)]]
  });
}

  // دالة إرسال بيانات التسجيل للباك إند
 onRegister() {
  if (this.registerForm.invalid) {
    this.registerForm.markAllAsTouched();
    return;
  }

  this.isLoading.set(true);
  this.errorMessage.set(null);

  const payload: RegisterDto = this.registerForm.value;
  
  // حفظ الإيميل لاستخدامه في خطوة الـ OTP
  this.registeredEmail.set(payload.email || '');

  this.authService.register(payload).subscribe({
    next: (res: any) => {
      this.isLoading.set(false);
      this.showOtpModal.set(true); // تحويل الشاشة فوراً لصندوق الـ OTP 🎉
    },
    error: (err: any) => {
      this.isLoading.set(false);
      this.errorMessage.set(err.error?.message || 'حدث خطأ أثناء التسجيل.');
    }
  });
}

// 2. دالة تأكيد الـ OTP (الخطوة 3 + 4)
onVerifyOtp() {
  if (this.otpForm.invalid) return;

  this.isLoading.set(true);
  const code = this.otpForm.value.otpCode; // الكود اللي المستخدم كتبه

  // نرسل الإيميل والكود للباك إند
  this.authService.confirmEmail(this.registeredEmail(), code).subscribe({
    next: (res: any) => {
      this.isLoading.set(false);
      this.showOtpModal.set(false);
      
      alert('تم تفعيل حسابك بنجاح! يمكنك الآن تسجيل الدخول باستخدام حسابك الفعلي.');
      
      // التوجيه الذكي لصفحة الـ Login
      this.router.navigate(['/auth/login']);
    },
    error: (err: any) => {
      this.isLoading.set(false);
      alert(err.error?.message || 'كود الـ OTP غير صحيح أو منتهي الصلاحية.');
    }
  });
}
}
