import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Nursery } from '../../../../core/services/nursery'; 
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NurseryListItem } from '../../../../core/models/parent-nursery.model';
import { Review } from '../../../../core/models/parent-nursery.model';

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

  private apiUrl = 'http://localhost:5104/api/Nurseries';

  nurseryId!: number;
  nursery = signal<NurseryListItem | null>(null);
  reviews = signal<Review[]>([]);
  isLoading = signal<boolean>(true);
  isSubmittingReview = signal<boolean>(false);

  reviewForm!: FormGroup;

  constructor() {
    this.reviewForm = this.fb.group({
      rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
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
    if (this.reviewForm.invalid) return;

    this.isSubmittingReview.set(true);
    const body = this.reviewForm.value;

    this.http.post(`${this.apiUrl}/${this.nurseryId}/reviews`, body).subscribe({
      next: () => {
        this.isSubmittingReview.set(false);
        this.reviewForm.patchValue({ rating: 5, comment: '' }); // تصفير الفورم
        alert('تم إضافة تقييمك بنجاح! ⭐');
        this.loadNurseryReviews(); // إعادة تحميل التقييمات لعرض التقييم الجديد فوراُ
      },
      error: (err) => {
        this.isSubmittingReview.set(false);
        alert(err.error?.message || 'حدث خطأ أثناء إرسال التقييم.');
      }
    });
  }
}
