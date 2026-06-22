import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NurseryListItem } from '../../../../core/models/parent-nursery.model';
import { NurseryCard } from '../../../../shared/components/nursery-card/nursery-card';
import { HttpClient, HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-nursery-list',
  standalone: true, // تأكد إنها standalone لو بتستخدمها كدة
  imports: [CommonModule, RouterLink, FormsModule, NurseryCard],
  templateUrl: './nursery-list.html',
  styleUrl: './nursery-list.css',
})
export class NurseryList implements OnInit {
  private http = inject(HttpClient);
  // 1️⃣ تعديل الحروف لـ lowercase بالملي تماشياً مع الـ Swagger
  private apiUrl = 'http://localhost:5104/api/nurseries';

  // الـ Signals الخاصة بالبيانات والتحميل
  nurseries = signal<NurseryListItem[]>([]);
  isLoading = signal<boolean>(true);

  // فلاتر البحث المسؤولة عن الـ Query Params لايف
  searchCity = signal<string>('');
  maxPrice = signal<number | null>(null);
  minRating = signal<number | null>(null);
  currentPage = signal<number>(1);
  pageSize = 6; // عرض 6 حضانات في الصفحة

  ngOnInit(): void {
    this.fetchNurseries();
  }

  fetchNurseries() {
    this.isLoading.set(true);
    
    let params = new HttpParams()
      .set('PageNumber', this.currentPage().toString())
      .set('PageSize', this.pageSize.toString());

    if (this.searchCity()) params = params.set('City', this.searchCity());
    if (this.maxPrice()) params = params.set('MaxPrice', this.maxPrice()!.toString());
    if (this.minRating()) params = params.set('MinRating', this.minRating()!.toString());

    // 2️⃣ استلام الـ Response كـ any لتجنب ضرب الـ Type والتشييك على بنية البيانات الراجعة
    this.http.get<any>(this.apiUrl, { params }).subscribe({
      next: (res) => {
        let actualArray: NurseryListItem[] = [];

        if (Array.isArray(res)) {
          // لو الـ API باعت الـ Array علطول
          actualArray = res;
        } else if (res && Array.isArray(res.data)) {
          // لو الباك إند لاففها جوه أوبجكت اسمه data
          actualArray = res.data;
        } else if (res && Array.isArray(res.nurseries)) {
          // لو الباك إند لاففها جوه أوبجكت اسمه nurseries
          actualArray = res.nurseries;
        } else if (res && typeof res === 'object') {
          // حل وقائي للـ Pagination: تدوير على أول خاصية جواه نوعها مصفوفة
          actualArray = Object.values(res).find(val => Array.isArray(val)) as NurseryListItem[] || [];
        }

        // إرسال المصفوفة السليمة والنظيفة للـ Signal
        this.nurseries.set(actualArray);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('حدث خطأ أثناء سحب الحضانات:', err);
        this.nurseries.set([]); // حماية الكود بمصفوفة فارغة منعاً لتعليق الصفحة
        this.isLoading.set(false);
      }
    });
  }


  applyFilters() {
    this.currentPage.set(1); // إعادة التصفير للصفحة الأولى عند البحث
    this.fetchNurseries();
  }

  changePage(newPage: number) {
    this.currentPage.set(newPage);
    this.fetchNurseries();
  }
}