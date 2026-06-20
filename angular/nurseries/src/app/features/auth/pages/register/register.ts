import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoading = signal<boolean>(false);
  showOtpModal = signal<boolean>(false); 
  errorMessage = signal<string | null>(null);
  registeredEmail = signal<string>(''); 

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
      role: ['Parent', [Validators.required]] // القيمة الافتراضية
    });

    this.otpForm = this.fb.group({
      otpCode: ['', [Validators.required, Validators.minLength(4)]]
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

    this.authService.register(payload).subscribe({
      next: (res: string) => {
        this.isLoading.set(false);
        this.showOtpModal.set(true); // تحويل لايف لصندوق الـ OTP
      },
      error: (err: any) => {
        this.isLoading.set(false);
        console.error('تفاصيل كراش السيرفر بالكامل:', err); // هيطبع لك تفاصيل الـ 500 في الـ Inspect
        
        if (err.error && typeof err.error === 'string') {
          this.errorMessage.set(err.error);
        } else {
          this.errorMessage.set('السيرفر واجه خطأ داخلي (500). يرجى مراجعة شاشة الـ .NET Console السوداء.');
        }
      }
    });
  }

  onVerifyOtp() {
    if (this.otpForm.invalid) return;

    this.isLoading.set(true);
    const code = this.otpForm.value.otpCode;

    this.authService.confirmEmail(this.registeredEmail(), code).subscribe({
      next: (res: string) => {
        this.isLoading.set(false);
        this.showOtpModal.set(false);
        alert('تم تفعيل حسابك بنجاح في قاعدة البيانات! يمكنك الآن تسجيل الدخول.');
        this.router.navigate(['/auth/login']);
      },
      error: (err: any) => {
        this.isLoading.set(false);
        alert(err.error || 'كود الـ OTP غير صحيح أو انتهت صلاحيته.');
      }
    });
  }
}