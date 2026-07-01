import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  public backUrl = environment.backUrl;
  private apiUrl = this.backUrl + '/api/nurseries';

  featuredNurseries = signal<any[]>([]);
  reviewsList = signal<any[]>([]); // السيجنال الخاص بالتقييمات الديناميكية
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.loadFeaturedNurseries();
    this.loadHomeReviews();
  }

  loadFeaturedNurseries() {
    this.isLoading.set(true);
    this.http.get<any>(this.apiUrl).subscribe({
      next: (res) => {
        let actualList: any[] = [];
        
        if (Array.isArray(res)) {
          actualList = res;
        } else if (res && Array.isArray(res.data)) {
          actualList = res.data;
        } else if (res && Array.isArray(res.nurseries)) {
          actualList = res.nurseries;
        } else if (res && typeof res === 'object') {
          actualList = Object.values(res).find(val => Array.isArray(val)) as any[] || [];
        }

        // أخذ أول 3 حضانات فقط للعرض في الهوم
        this.featuredNurseries.set(actualList.slice(0, 3));
        console.log('الحضانات المميزة:', this.featuredNurseries());
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('خطأ أثناء تحميل الحضانات في الرئيسية:', err);
        this.featuredNurseries.set([]);
        this.isLoading.set(false);
      }
    });
  }

  // جلب التقييمات ديناميكياً
  loadHomeReviews() {
    // بيتم جلب التقييمات العامة أو محاكاة الرد من الباك إند بناءً على الداتا المتاحة
    this.http.get<any>(this.backUrl + '/api/nurseries').subscribe({
      next: (res) => {
        // بنستخرج الريفيوهات المتاحة من الداتا لو موجودة
        let extractedReviews: any[] = [];
        if (Array.isArray(res)) {
          res.forEach(n => {
            if (n.reviews && Array.isArray(n.reviews)) {
              extractedReviews.push(...n.reviews);
            }
          });
        }
        this.reviewsList.set(extractedReviews.slice(0, 3));
      },
      error: (err) => {
        console.error('خطأ أثناء جلب الريفيوهات:', err);
        this.reviewsList.set([]); // سيعرض الاحتياطي المنسق في الـ HTML مباشرة
      }
    });
  }

  onSearch(searchTerm: string) {
    const term = searchTerm.trim();
    if (!term) {
      this.router.navigate(['/nurseries']);
      return;
    }
    this.router.navigate(['/nurseries'], { queryParams: { search: term } });
  }
}