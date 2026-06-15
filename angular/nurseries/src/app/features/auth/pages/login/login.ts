import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth'; // تأكد من اسم الملف صح (.service)
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
 private fb = inject(FormBuilder);
  private router = inject(Router);

  loginForm!: FormGroup;

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      // الـ Email مطلوب ولازم يكون فورمت إيميل صحيح
      email: ['', [Validators.required, Validators.email]],
      // الـ Password مطلوب ومش أقل من 6 حقول
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  // ميثود مساعدة (Getter) عشان نسهل قراءة الأخطاء في الـ HTML
  get f() { return this.loginForm.controls; }

  onSubmit(): void {
    if (this.loginForm.valid) {
      console.log('بيانات تسجيل الدخول صالحة:', this.loginForm.value);
      // هنا هتربط بعدين بالـ AuthService
      this.router.navigate(['/']);
    } else {
      this.loginForm.markAllAsTouched(); // لو داس زرار والبيانات غلط، نور الحقول أحمر
    }
  }
  }
