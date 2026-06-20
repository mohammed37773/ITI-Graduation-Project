import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth'; // تأكد من اسم الملف صح (.service)
import { CommonModule } from '@angular/common';
import { AuthResponseDto, LoginDto } from '../../../../core/models/authModel';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
 private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  // Signals لإدارة حالة الشاشة والـ الأخطاء لايف
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  loginForm!: FormGroup;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

onLogin() {
  if (this.loginForm.invalid) {
    this.loginForm.markAllAsTouched();
    return;
  }

  this.isLoading.set(true);
  this.errorMessage.set(null);

  const payload: LoginDto = this.loginForm.value;

  this.authService.login(payload).subscribe({
    next: (res: AuthResponseDto) => { 
      this.isLoading.set(false);
      
      // 🔥 الـ Role-Based Routing الذكي هنا يا ريس:
      const userRole = res.role; // القيمة اللي راجعة لايف من الـ API

      if (userRole === 'Parent') {
        this.router.navigate(['/parent/nursery-list']); // يروح لصفحة الحضانات المخصصة للـ Parent
      } else if (userRole === 'NurseryOwner') {
        this.router.navigate(['/owner/dashboard']);   // يروح للوحة تحكم صاحب الحضانة
      } else if (userRole === 'Admin') {
        this.router.navigate(['/admin/dashboard']);   // لوحة تحكم الأدمن العام
      } else {
        this.router.navigate(['/']); // مسار احتياطي عام
      }
    },
    error: (err: any) => { 
      this.isLoading.set(false);
      this.errorMessage.set(err.error?.message || 'بريد إلكتروني أو كلمة مرور غير صحيحة.');
    }
  })};
}
