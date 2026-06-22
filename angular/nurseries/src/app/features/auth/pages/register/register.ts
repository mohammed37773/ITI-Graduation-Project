import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
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
  private http = inject(HttpClient); 
  private router = inject(Router);
  private authService = inject(AuthService);
  private apiUrl = 'http://localhost:5104/api/Auth/register'; 

  // Signals لإدارة حالة الشاشة لايف بالبرانش الجديد
  isLoading = signal<boolean>(false);
  isSuccess = signal<boolean>(false); 
  errorMessage = signal<string | null>(null);
  registeredEmail = signal<string>(''); 

  registerForm!: FormGroup;

  constructor() {
    this.initForm();
  }

  private initForm() {
    // الـ Regex يشترط: حرف كبير، حرف صغير، رقم، ورمز خاص مع طول 6 خانات على الأقل
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z]\d@$!%*?&]{6,}$/;

    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [
        Validators.required, 
        Validators.minLength(6),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/)
      ]],
      role: ['Parent', [Validators.required]] 
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
        console.log('رد السيرفر الناجح:', res); 
        this.isLoading.set(false);
        this.isSuccess.set(true); 
        this.registerForm.reset({ role: 'Parent' }); 
      },
      error: (err: any) => {
        this.isLoading.set(false);
        console.error('تفاصيل الخطأ الفعلي:', err);
        
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