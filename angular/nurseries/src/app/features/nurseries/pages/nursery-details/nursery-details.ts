import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Nursery } from '../../../../core/services/nursery'; 
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { NurseryListItem } from '../../../../core/models/parent-nursery.model';
import { Review } from '../../../../core/models/parent-nursery.model';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-nursery-details',
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './nursery-details.html',
  styleUrl: './nursery-details.css',
})
export class NurseryDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);

  private apiUrl = environment.backUrl + '/api/Nurseries';

  nurseryId!: number;
  nursery = signal<NurseryListItem | null>(null);
  reviews = signal<Review[]>([]);
  isLoading = signal<boolean>(true);
  isSubmittingReview = signal<boolean>(false);

  reviewForm!: FormGroup;

  constructor() {
    this.reviewForm = this.fb.group({
  rating: [5, [Validators.required]],
  comment: ['', [Validators.required]]
});
  }

  ngOnInit(): void {
    // سحب الـ id من الـ Route URL
    this.nurseryId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.nurseryId) {
      this.loadNurseryDetails();
      this.loadNurseryReviews();
    }
  }

  // 1. جلب بيانات الحضانة الأساسية
  loadNurseryDetails() {
    this.isLoading.set(true);
    this.http.get<NurseryListItem>(`${this.apiUrl}/${this.nurseryId}`).subscribe({
      next: (data) => {
        this.nursery.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('خطأ في جلب تفاصيل الحضانة:', err);
        this.isLoading.set(false);
      }
    });
  }

  // 2. جلب التقييمات الخاصة بالحضانة
  loadNurseryReviews() {
    this.http.get<Review[]>(`${this.apiUrl}/${this.nurseryId}/reviews`).subscribe({
      next: (data) => {
        this.reviews.set(data || []);
      },
      error: (err) => console.error('خطأ في جلب التقييمات:', err)
    });
  }

  // 3. إضافة تقييم جديد
  submitReview() {
  if (this.reviewForm.invalid) {
    this.reviewForm.markAllAsTouched();
    return;
  }

  this.isSubmittingReview.set(true);

  // 1️⃣ جلب التوكن من الـ Session وتأمينه في الـ Headers
  const sessionData = localStorage.getItem('user_session');
  let savedToken = '';
  if (sessionData) {
    try {
      savedToken = JSON.parse(sessionData).token || '';
    } catch (e) {
      console.error('خطأ في قراءة التوكن:', e);
    }
  }

  const headers = new HttpHeaders({
    'Authorization': `Bearer ${savedToken}`,
    'Content-Type': 'application/json'
  });

  // 2️⃣ بناء الـ Payload النظيف والمطابق للـ Prototype المتوقع في الباك إند
  const reviewPayload = {
    rating: Number(this.reviewForm.value.rating), // تحويل إجباري لـ Number لمنع الـ 400
    comment: this.reviewForm.value.comment.trim()
  };

  // جلب الـ ID بتاع الحضانة الحالية
  const nurseryId = this.nursery()?.id; 
  const url = `http://localhost:5104/api/Nurseries/${nurseryId}/reviews`;

  console.log('🚀 Sending Clean Review Payload:', reviewPayload);

  this.http.post<any>(url, reviewPayload, { headers }).subscribe({
    next: (newReview) => {
      this.isSubmittingReview.set(false);
      
      // تحديث قائمة التقييمات فوراً على الشاشة
      this.reviews.set([...this.reviews(), newReview]);
      
      // تصفير الفورم بنجاح
      this.reviewForm.reset({ rating: 5, comment: '' });
      
      alert('🟢 تم نشر تقييمك الطبي بنجاح يا رئيس!');
    },
    error: (err) => {
      this.isSubmittingReview.set(false);
      console.error('❌ خطأ الـ Review API:', err);
      
      // 3️⃣ لقط الرسالة الذكية القادمة من الباك إند مباشرة وعرضها للمستخدم
      let errorMessage = 'حدث خطأ أثناء إرسال التقييم.';
      
      if (err.error) {
        if (typeof err.error === 'string') {
          errorMessage = err.error; // هيلقط هنا: "مش هتقدر تعمل Review غير لو حجزت في الحضانة دي"
        } else if (err.error.message) {
          errorMessage = err.error.message;
        }
      }
      
      alert(`⚠️ ${errorMessage}`);
    }
  });
}
}
