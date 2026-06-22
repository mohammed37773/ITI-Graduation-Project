import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})

export class Profile implements OnInit {
  private http = inject(HttpClient);
  
  // الـ Base URL الفعلي من التوثيق الخاص بك
  private baseUrl = 'http://localhost:5104/api'; 

  // بيانات ولي الأمر (يتم جلبها ديناميكياً مما تم حفظه أثناء الـ Login)
  parentName = signal<string>(localStorage.getItem('fullName') || 'ولي أمر IncuCare');
  parentEmail = signal<string>(localStorage.getItem('email') || '');
  parentRole = signal<string>(localStorage.getItem('role') || 'Parent');

  // قائمة الأطفال (Signals)
  children = signal<any[]>([]);
  
  // فورم إضافة/تعديل طفل
  childForm = signal({
    id: null as number | null,
    fullName: '',
    dateOfBirth: '',
    specialNeeds: ''
  });

  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  successMessage = signal<string>('');
  errorMessage = signal<string>('');

  ngOnInit() {
    if (this.parentRole() === 'Parent') {
      this.loadChildren();
    }
  }

  // 1️⃣ جلب قائمة الأطفال: GET /api/Children
  loadChildren() {
    this.isLoading.set(true);
    this.http.get<any[]>(`${this.baseUrl}/Children`).subscribe({
      next: (data) => {
        this.children.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'فشل في تحميل قائمة الأطفال.');
        this.isLoading.set(false);
      }
    });
  }

  // 2️⃣ حفظ أو تحديث بيانات طفل: POST or PUT /api/Children
  saveChild() {
    this.isSaving.set(true);
    this.successMessage.set('');
    this.errorMessage.set('');

    const payload = {
      fullName: this.childForm().fullName,
      dateOfBirth: this.childForm().dateOfBirth,
      specialNeeds: this.childForm().specialNeeds
    };

    if (this.isEditMode() && this.childForm().id) {
      // تعديل طفل قائم: PUT /api/Children/{id}
      this.http.put(`${this.baseUrl}/Children/${this.childForm().id}`, payload).subscribe({
        next: () => {
          this.successMessage.set('تم تحديث بيانات الطفل بنجاح! 🎉');
          this.resetForm();
          this.loadChildren();
        },
        error: (err) => this.handleError(err)
      });
    } else {
      // إضافة طفل جديد: POST /api/Children
      this.http.post(`${this.baseUrl}/Children`, payload).subscribe({
        next: () => {
          this.successMessage.set('تم إضافة الطفل بنجاح وإدراجه بالنظام! 👶');
          this.resetForm();
          this.loadChildren();
        },
        error: (err) => this.handleError(err)
      });
    }
  }

  // 3️⃣ حذف طفل: DELETE /api/Children/{id}
  deleteChild(id: number) {
    if (confirm('هل أنت متأكد من حذف هذا الطفل؟ لا يمكن التراجع عن هذا الإجراء.')) {
      this.http.delete(`${this.baseUrl}/Children/${id}`).subscribe({
        next: () => {
          this.successMessage.set('تم حذف الطفل من الحساب بنجاح.');
          this.loadChildren();
        },
        error: (err) => this.handleError(err)
      });
    }
  }

  // وضع التعديل
  editChild(child: any) {
    this.isEditMode.set(true);
    this.childForm.set({
      id: child.id,
      fullName: child.fullName,
      // تحويل التاريخ لـ YYYY-MM-DD ليناسب الـ Input HTML
      dateOfBirth: child.dateOfBirth ? child.dateOfBirth.split('T')[0] : '',
      specialNeeds: child.specialNeeds || ''
    });
  }

  resetForm() {
    this.isEditMode.set(false);
    this.childForm.set({ id: null, fullName: '', dateOfBirth: '', specialNeeds: '' });
    this.isSaving.set(false);
  }

  private handleError(err: any) {
    this.errorMessage.set(err.error?.message || 'حدث خطأ ما، يرجى المحاولة لاحقاً.');
    this.isSaving.set(false);
  }
}