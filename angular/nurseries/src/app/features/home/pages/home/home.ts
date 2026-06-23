import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router); // 🎯 حقن الـ Router للتوجيه عند البحث

  private apiUrl = 'http://localhost:5104/api/nurseries';

  featuredNurseries = signal<any[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.loadFeaturedNurseries();
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

        // أخذ أول 3 حواضن طبية مميزة فقط للعرض في الهوم
        this.featuredNurseries.set(actualList.slice(0, 3));
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('خطأ أثناء تحميل الحضانات في الرئيسية:', err);
        this.featuredNurseries.set([]);
        this.isLoading.set(false);
      }
    });
  }

  // 🔍 2️⃣ دالة تشغيل البحث السريع والانتقال لصفحة التصفح الشاملة
  onSearch(searchTerm: string) {
    const term = searchTerm.trim();
    if (!term) {
      // لو ضغط بحث وهو فاضي، يوجهه لصفحة التصفح العامة لكل الحواضن
      this.router.navigate(['/nurseries']);
      return;
    }
    
    // التوجيه لصفحة الـ nurseries مع باصي كلمة البحث في الـ Query Params ليتم تصفيتها هناك لايف
    this.router.navigate(['/nurseries'], { queryParams: { search: term } });
  }
}