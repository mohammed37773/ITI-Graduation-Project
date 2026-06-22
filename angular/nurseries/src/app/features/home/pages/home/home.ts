import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true, // تأكيد الـ standalone
  imports: [CommonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  private http = inject(HttpClient);
  // 1️⃣ تعديل الـ URL ليكون بحروف صغيرة تماماً لتفادي الـ 404
  private apiUrl = 'http://localhost:5104/api/nurseries';

  featuredNurseries = signal<any[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.loadFeaturedNurseries();
  }

  loadFeaturedNurseries() {
    this.isLoading.set(true);
    
    // هنستقبلها كـ <any> لأنها أوبجكت مغلف وليس Array مباشرة
    this.http.get<any>(this.apiUrl).subscribe({
      next: (res) => {
        // 2️⃣ التشييك الذكي: هل الـ res نفسه Array؟ ولا جواه خاصية اسمها data أو nurseries؟
        let actualList: any[] = [];
        
        if (Array.isArray(res)) {
          actualList = res;
        } else if (res && Array.isArray(res.data)) {
          actualList = res.data;
        } else if (res && Array.isArray(res.nurseries)) {
          actualList = res.nurseries;
        } else if (res && typeof res === 'object') {
          // لو الباك إند باعت أوبجكت فيه باجينيشن، بنحاول نلقط أي مصفوفة جواه
          actualList = Object.values(res).find(val => Array.isArray(val)) as any[] || [];
        }

        // كدة ضمنّا 100% إن الـ actualList عبارة عن Array ومستحيل تضرب تيب ايرور
        this.featuredNurseries.set(actualList.slice(0, 3));
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('خطأ أثناء تحميل الحضانات في الرئيسية:', err);
        this.featuredNurseries.set([]); // تأمين الشاشة بمصفوفة فاضية عند الخطأ
        this.isLoading.set(false);
      }
    });
  }
}