import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Nursery } from '../../../../core/services/nursery';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import{NurseryModel} from '../../../../core/models/nursery.model'

@Component({
  selector: 'app-nursery-list',
  imports: [CommonModule, ReactiveFormsModule , RouterLink],
  templateUrl: './nursery-list.html',
  styleUrl: './nursery-list.css',
})
export class NurseryList implements OnInit {
  private fb = inject(FormBuilder);

  filterForm!: FormGroup;
  allNurseries: NurseryModel[] = [];
  filteredNurseries: NurseryModel[] = [];
  
  // قائمة المناطق المتاحة لعرضها في الـ Dropdown
  locationsList: string[] = ['المعادي', 'التجمع الخامس', 'مدينة نصر', 'مصر الجديدة', 'الدقي'];

  ngOnInit(): void {
    // 1. إضافة الـ location في فورم الفلاتر
    this.filterForm = this.fb.group({
      searchQuery: [''],
      location: [''], // الفلتر الجديد
      maxPrice: [500],
      age: [''],
      verifiedOnly: [false]
    });

    // 2. تحديث الداتا لتشمل الـ location Object
    this.allNurseries = [
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
        images: [{ id: 3,nurseryId: 3, url: "https://images.unsplash.com/photo-1603481588273-2f908a9a7a1b?q=80&w=500&auto=format&fit=crop", isMain: true }]
      }
    ];

    this.filteredNurseries = [...this.allNurseries];

    this.filterForm.valueChanges.subscribe(() => {
      this.applyFilters();
    });
  }

  applyFilters(): void {
    const { searchQuery, location, maxPrice, age, verifiedOnly } = this.filterForm.value;

    this.filteredNurseries = this.allNurseries.filter(nursery => {
      const matchesSearch = !searchQuery || nursery.name.includes(searchQuery);
      
      // منطق فلترة الموقع الجديد (بيطابق الـ region جوه الـ location object)
      const matchesLocation = !location || (nursery.location?.region === location);
      
      const matchesPrice = nursery.dailyPrice <= maxPrice;
      const matchesAge = !age || (Number(age) >= nursery.ageRangeMin && Number(age) <= nursery.ageRangeMax);
      const matchesVerified = !verifiedOnly || nursery.isVerified;

      return matchesSearch && matchesLocation && matchesPrice && matchesAge && matchesVerified;
    })
  }
}

