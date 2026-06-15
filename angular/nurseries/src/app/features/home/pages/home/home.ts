import { Component, OnInit } from '@angular/core';
import { Nursery } from '../../../../core/models/nursery.model';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit {
  // مصفوفة الحضانات المميزة اللي هتربط بعدين بالـ API
  featuredNurseries: Nursery[] = [];

  ngOnInit(): void {
    // تحديث الداتا لتطابق الـ Properties الجديدة بالملي
    this.featuredNurseries = [
      {
        id: 1,
        name: "حضانة الزهور السعيدة",
        description: "بيئة تعليمية وترفيهية آمنة لأطفالكم مع رعاية طبية متكاملة وأنشطة يومية متميزة.",
        dailyPrice: 50,
        ageRangeMin: 1,
        ageRangeMax: 4,
        capacity: 30,
        avgRating: 4.8,
        isVerified: true,
        images: [{ id: 1, nurseryId: 1, url: "https://images.unsplash.com/photo-1576489922094-27a5521ff34c?q=80&w=500&auto=format&fit=crop", isMain: true }]
      },
      {
        id: 2,
        name: "حضانة المستقبل الذكي",
        description: "نهتم بتأسيس اللغات وتنمية مهارات الطفل الإبداعية والذكاء الاصطناعي المبكر للأطفال.",
        dailyPrice: 75,
        ageRangeMin: 2,
        ageRangeMax: 6,
        capacity: 45,
        avgRating: 4.6,
        isVerified: true,
        images: [{ id: 2, nurseryId: 2, url: "https://images.unsplash.com/photo-1485546246426-74dc88dec4d9?q=80&w=500&auto=format&fit=crop", isMain: true }]
      },
      {
        id: 3,
        name: "حضانة عباقرة الغد",
        description: "برامج تعليمية متطورة تعتمد على منتسوري لتنمية مهارات الاعتماد على النفس والاستكشاف.",
        dailyPrice: 60,
        ageRangeMin: 1,
        ageRangeMax: 5,
        capacity: 25,
        avgRating: 4.3,
        isVerified: false,
        images: [{ id: 3, nurseryId: 3,  url: "https://images.unsplash.com/photo-1603481588273-2f908a9a7a1b?q=80&w=500&auto=format&fit=crop", isMain: true }]
      }
    ];
  }}
