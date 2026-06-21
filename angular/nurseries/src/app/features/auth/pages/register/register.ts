import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import {AuthService} from '../../../../core/services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient); // استخدام HttpClient مباشرة لضمان الربط المباشر بالسيرفر
  private router = inject(Router);
  private authService = inject(AuthService);
  private apiUrl = 'http://localhost:5104/api/Auth/register'; // الـ Endpoint الرسمي للتسجيل

  // Signals لإدارة حالة الشاشة لايف بالبرانش الجديد
  isLoading = signal<boolean>(false);
  isSuccess = signal<boolean>(false); // بديل للـ OtpModal لمطابقة نظام تأكيد الرابط
  errorMessage = signal<string | null>(null);
  registeredEmail = signal<string>(''); 

  registerForm!: FormGroup;

  constructor() {
    this.initForm();
  }

  private initForm() {
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['Parent', [Validators.required]] // القيمة الافتراضية المعتمدة بالسيرفر (Parent / NurseryAdmin)
    });
  }

onRegister() {
  if (this.registerForm.invalid) {
    this.registerForm.markAllAsTouched();
    return;
  }

  this.isLoading.set(true);
  this.errorMessage.set(null);

  const payload = this.registerForm.value;
  this.registeredEmail.set(payload.email || '');

  // استدعاء الـ register من الـ AuthService اللي متظبطة على text
  this.authService.register(payload).subscribe({
    next: (res: string) => {
      // الـ res هنا هتوصل النص الصريح اللي السيرفر بعته: "تم التسجيل بنجاح..."
      console.log('رد السيرفر الناجح:', res); 
      
      this.isLoading.set(false);
      this.isSuccess.set(true); // هتفتح الـ Alert الأخضر في الـ HTML فوراً
      this.registerForm.reset({ role: 'Parent' }); // تصفير الفورم بأمان
    },
    error: (err: any) => {
      this.isLoading.set(false);
      console.error('تفاصيل الخطأ الفعلي:', err);
      
      // التقاط رسائل الخطأ الموحدة لو الإيميل مكرر مثلاً
      if (err.error && err.error.message) {
        this.errorMessage.set(err.error.message);
      } else if (typeof err.error === 'string') {
        this.errorMessage.set(err.error);
      } else {
        this.errorMessage.set('حدث خطأ أثناء التسجيل، يرجى إعادة المحاولة.');
      }
    }
  });
}
}