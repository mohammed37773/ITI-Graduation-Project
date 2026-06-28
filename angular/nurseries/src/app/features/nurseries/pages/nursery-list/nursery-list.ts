import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router'; // 🎯 إضافة ActivatedRoute لقط الفلتر من الهوم
import { CommonModule } from '@angular/common';
import { NurseryListItem } from '../../../../core/models/parent-nursery.model';
import { NurseryCard } from '../../../../shared/components/nursery-card/nursery-card';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';


@Component({
  selector: 'app-nursery-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, NurseryCard],
  templateUrl: './nursery-list.html',
  styleUrl: './nursery-list.css',
})
export class NurseryList implements OnInit {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute); // 🎯 حقن الـ Route لربط الهوم بالقائمة

  private apiUrl = environment.backUrl + '/api/nurseries';

  nurseries = signal<NurseryListItem[]>([]);
  isLoading = signal<boolean>(true);

  searchCity = signal<string>('');
  maxPrice = signal<number | null>(null);
  minRating = signal<number | null>(null);
  currentPage = signal<number>(1);
  pageSize = 6; 

  ngOnInit(): void {
    // 🔍 لقط كلمة البحث القادمة من الهوم أوتوماتيكياً عند فتح الصفحة
    this.route.queryParams.subscribe(params => {
      const homeSearch = params['search'] || '';
      if (homeSearch) {
        this.searchCity.set(homeSearch); // وضع الكلمة داخل فلتر المدينة/المنطقة
      }
      this.fetchNurseries();
    });
  }

  fetchNurseries() {
    this.isLoading.set(true);
    
    let params = new HttpParams()
      .set('PageNumber', this.currentPage().toString())
      .set('PageSize', this.pageSize.toString());

    // تأكد من توافق مفاتيح الباك إند (لو الباك إند مستني سمال أو كابيتال)
    if (this.searchCity()) params = params.set('City', this.searchCity());
    if (this.maxPrice()) params = params.set('MaxPrice', this.maxPrice()!.toString());
    if (this.minRating()) params = params.set('MinRating', this.minRating()!.toString());

    this.http.get<any>(this.apiUrl, { params }).subscribe({
      next: (res) => {
        let actualArray: NurseryListItem[] = [];

        if (Array.isArray(res)) {
          actualArray = res;
        } else if (res && Array.isArray(res.data)) {
          actualArray = res.data;
        } else if (res && Array.isArray(res.nurseries)) {
          actualArray = res.nurseries;
        } else if (res && typeof res === 'object') {
          actualArray = Object.values(res).find(val => Array.isArray(val)) as NurseryListItem[] || [];
        }

        this.nurseries.set(actualArray);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('حدث خطأ أثناء سحب الحضانات:', err);
        this.nurseries.set([]);
        this.isLoading.set(false);
      }
    });
  }

  applyFilters() {
    this.currentPage.set(1); 
    this.fetchNurseries();
  }

  changePage(newPage: number) {
    this.currentPage.set(newPage);
    this.fetchNurseries();
  }
}