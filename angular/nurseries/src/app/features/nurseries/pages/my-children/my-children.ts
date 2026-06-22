import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Child } from '../../../../core/models/child.model';

@Component({
  selector: 'app-my-children',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './my-children.html',
  styleUrl: './my-children.css',
})
export class MyChildren implements OnInit {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);

  private apiUrl = 'http://localhost:5104/api/Children';

  children = signal<Child[]>([]);
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  showAddForm = signal<boolean>(false); // لإظهار/إخفاء فورم الإضافة سرياً

  childForm!: FormGroup;

  constructor() {
    this.childForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      age: [2, [Validators.required, Validators.min(0), Validators.max(12)]],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.loadChildren();
  }

  // 1. جلب قائمة الأطفال
  loadChildren() {
    this.isLoading.set(true);
    this.http.get<Child[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.children.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('خطأ أثناء جلب قائمة الأطفال:', err);
        this.isLoading.set(false);
      }
    });
  }

  // 2. إضافة طفل جديد
  onSubmit() {
    if (this.childForm.invalid) {
      this.childForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const body: Child = this.childForm.value;

    this.http.post(this.apiUrl, body).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.childForm.reset({ age: 2 });
        this.showAddForm.set(false); // إغلاق الفورم بعد النجاح
        alert('تم إضافة بيانات طفلك بنجاح! 👶❤️');
        this.loadChildren(); // تحديث القائمة لايف
      },
      error: (err) => {
        this.isSubmitting.set(false);
        alert(err.error?.message || 'حدث خطأ أثناء إضافة الطفل.');
      }
    });
  }}
