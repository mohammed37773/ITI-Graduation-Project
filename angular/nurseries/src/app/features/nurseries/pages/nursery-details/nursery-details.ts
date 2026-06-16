import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Nursery } from '../../../../core/services/nursery'; 
import { NurseryModel } from '../../../../core/models/nursery.model';

@Component({
  selector: 'app-nursery-details',
  imports: [CommonModule, RouterLink],
  templateUrl: './nursery-details.html',
  styleUrl: './nursery-details.css',
})
export class NurseryDetails {

  private route = inject(ActivatedRoute);

  // الـ Signals لإدارة حالة الشاشة
  nursery = signal<NurseryModel | null>(null);
  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);

  // 📦 الداتا اللوكال بتاعتك متلقمة هنا بالملي لحد ما الـ API يجهز
  allNurseries: NurseryModel[] = [
    {
      id: 1,
      name: "حضانة الزهور السعيدة",
      description: "رعاية طبية متكاملة وأنشطة ترفيهية يومية متميزة لتنمية مهارات طفلك.",
      dailyPrice: 50,
      ageRangeMin: 1,
      ageRangeMax: 4,
      capacity: 30,
      avgRating: 4.8,
      isVerified: true,
      location: { id: 1, nurseryId: 1, city: "القاهرة", region: "المعادي", address: "شارع 9" },
      images: [{ id: 1, nurseryId: 1, url: "https://images.unsplash.com/photo-1576489922094-27a5521ff34c?q=80&w=500&auto=format&fit=crop", isMain: true }]
    },
    {
      id: 2,
      name: "حضانة المستقبل الذكي",
      description: "تأسيس لغات وتنمية مهارات الذكاء الاصطناعي المبكر ومنهج المنتسوري.",
      dailyPrice: 120,
      ageRangeMin: 2,
      ageRangeMax: 6,
      capacity: 45,
      avgRating: 4.6,
      isVerified: true,
      location: { id: 2, nurseryId: 2, city: "القاهرة", region: "التجمع الخامس", address: "شارع التسعين" },
      images: [{ id: 2, nurseryId: 2, url: "https://images.unsplash.com/photo-1485546246426-74dc88dec4d9?q=80&w=500&auto=format&fit=crop", isMain: true }]
    },
    {
      id: 3,
      name: "حضانة عباقرة الغد",
      description: "تعتمد على مناهج تفاعلية حديثة لتنمية السلوك والاستكشاف الذاتي.",
      dailyPrice: 80,
      ageRangeMin: 3,
      ageRangeMax: 5,
      capacity: 25,
      avgRating: 4.3,
      isVerified: false,
      location: { id: 3, nurseryId: 3, city: "القاهرة", region: "مدينة نصر", address: "عباس العقاد" },
      images: [{ id: 3, nurseryId: 3, url: "https://images.unsplash.com/photo-1603481588273-2f908a9a7a1b?q=80&w=500&auto=format&fit=crop", isMain: true }]
    }
  ];

  ngOnInit() {
    // 1. جلب الـ ID من الـ URL لقراءته
    const id = Number(this.route.snapshot.paramMap.get('id'));
    
    // محاكاة لودينج خفيف (نصف ثانية) عشان الانميشن يظهر والـ UI يبقى واقعي
    setTimeout(() => {
      if (id) {
        // 2. البحث عن الحضانة اللوكال المطابقة للـ ID
        const foundNursery = this.allNurseries.find(n => n.id === id);
        
        if (foundNursery) {
          this.nursery.set(foundNursery);
        } else {
          this.errorMessage.set('عذراً، هذه الحضانة غير موجودة!');
        }
      } else {
        this.errorMessage.set('معرف الحضانة غير صحيح');
      }
      this.isLoading.set(false);
    }, 500);
  }
}
