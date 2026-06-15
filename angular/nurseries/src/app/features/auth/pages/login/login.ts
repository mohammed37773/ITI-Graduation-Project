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
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    rememberMe: [false]
  });

  onSubmit() {
    if (this.loginForm.valid) {
      // نداء واحد فقط للـ Service
      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          console.log('Login Successful', response);
          
          // جلب الـ Role المقروء من الـ Token بعد حفظه في الـ Service
          const role = this.authService.getRole();
          
          // التوجيه الذكي حسب الدور (Role-Based Routing)
          if (role === 'Admin') {
            this.router.navigate(['/admin/dashboard']);
          } else if (role === 'NurseryOwner') {
            this.router.navigate(['/owner/dashboard']);
          } else {
            this.router.navigate(['/']); // الـ Parent يروح للرئيسية يتصفح الحضانات
          }
        },
        error: (err) => {
          console.error('Login Failed', err);
          // هنا تقدر تعرض رسالة خطأ للمستخدم في الـ UI لو حابب
        }
      });
    }
  }
}