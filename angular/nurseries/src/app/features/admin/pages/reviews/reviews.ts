
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Review {
  id: string;
  reviewer: string;
  nursery: string;
  date: string;
  rating: number;
  text: string;
  status: 'معلقة' | 'موافق عليها' | 'مرفوضة';
  issues: number;
  avatar: string;
}

@Component({
  selector: 'app-reviews',
  imports: [CommonModule],
  templateUrl: './reviews.html',
  styleUrl: './reviews.css',
})
export class Reviews {
  activeTab = signal<'معلقة' | 'موافق عليها' | 'مرفوضة'>('معلقة');
  selectedReview = signal<Review | null>(null);

  reviews: Review[] = [
    { id: 'rev-001', reviewer: 'سارة جنكينز', nursery: 'حضانة ليتل أنجلز', date: '2024-05-15', rating: 5, text: 'الموظفون هنا رائعون تماماً. ازدهر ابني منذ انضمامه. المرافق نظيفة ومشرقة وآمنة جداً. أنصح بها لكل أهل يبحثون عن بيئة حاضنة', status: 'معلقة', issues: 0, avatar: 'https://images.pexels.com/photos/1065084/pexels-photo-1065084.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' },
    { id: 'rev-002', reviewer: 'ديفيد تشن', nursery: 'روضة غرين فالي', date: '2024-05-14', rating: 2, text: 'خُبت آمالي بسبب ضعف التواصل بشأن الأنشطة اليومية لابنتي. المرفق جيد لكن الجانب الإداري يحتاج تحسينات جوهرية.', status: 'معلقة', issues: 1, avatar: 'https://images.pexels.com/photos/220453/pexels-photo-220453.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' },
    { id: 'rev-003', reviewer: 'جيمس ويلسون', nursery: 'مركز أطفال سانشاين', date: '2024-05-16', rating: 1, text: 'هذا المكان فوضى تامة. [محتوى تم حذفه لمخالفته المعايير] ولا يراقبون الأطفال بشكل صحيح. ابتعدوا!', status: 'معلقة', issues: 3, avatar: 'https://images.pexels.com/photos/614810/pexels-photo-614810.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' },
    { id: 'rev-004', reviewer: 'ماريا سانتوس', nursery: 'أكاديمية قوس قزح', date: '2024-05-13', rating: 4, text: 'روضة ممتازة بشكل عام. الأطفال يُعتنى بهم جيداً والأنشطة تفاعلية ومثيرة.', status: 'موافق عليها', issues: 0, avatar: 'https://images.pexels.com/photos/1542085/pexels-photo-1542085.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' },
    { id: 'rev-005', reviewer: 'توم برادلي', nursery: 'حضانة تيني ستارز', date: '2024-05-12', rating: 1, text: 'تمت إزالة المحتوى غير اللائق من قبل المشرف.', status: 'مرفوضة', issues: 5, avatar: 'https://images.pexels.com/photos/1239291/pexels-photo-1239291.jpeg?auto=compress&cs=tinysrgb&w=80&h=80&dpr=1' },
  ];

  get filteredReviews(): Review[] {
    return this.reviews.filter(r => r.status === this.activeTab());
  }

  get pendingCount(): number {
    return this.reviews.filter(r => r.status === 'معلقة').length;
  }

  setTab(tab: 'معلقة' | 'موافق عليها' | 'مرفوضة') {
    this.activeTab.set(tab);
    this.selectedReview.set(null);
  }

  selectReview(review: Review) { this.selectedReview.set(review); }

  approveReview(review: Review, event: Event) {
    event.stopPropagation();
    review.status = 'موافق عليها';
    if (this.selectedReview()?.id === review.id) this.selectedReview.set(null);
  }

  rejectReview(review: Review, event: Event) {
    event.stopPropagation();
    review.status = 'مرفوضة';
    if (this.selectedReview()?.id === review.id) this.selectedReview.set(null);
  }

  getStarArray(rating: number): boolean[] {
    return [1,2,3,4,5].map(i => i <= rating);
  }
}
