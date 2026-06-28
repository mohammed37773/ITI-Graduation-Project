import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthResponseDto, LoginDto } from '../../../../core/models/authModel';
import {AuthService} from '../../../../core/services/auth'
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

  // الـ Signals المستخدمة في الـ HTML بتاعك لايف
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  // بناء الفورم مع شروط التحقق المتطابقة مع رسائل الخطأ في الـ HTML
  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

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
        
        const userRole = res.role; // الـ Role الراجع من السيرفر (Parent / NurseryAdmin / Admin)

        
        if (userRole === 'NurseryAdmin') {
          this.router.navigate(['/owner/dashboard']); // التوجيه للمسار الجديد اللي لسه باعته
        } else if (userRole === 'Parent') {
          this.router.navigate(['/parent/nursery-list']);
        } else if (userRole === 'Admin') {
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err: any) => {
        this.isLoading.set(false);
        console.error('خطأ في تسجيل الدخول:', err);
        
        
        if (err.error && err.error.message) {
          this.errorMessage.set(err.error.message);
        } else {
          this.errorMessage.set('البريد الإلكتروني أو كلمة المرور غير صحيحة.');
        }
      }
    });
  }
}