import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { environment } from '../../../../../environments/environment';
interface NurseryRecommendation {
  id: number;
  name: string;
  city: string;
  district: string;
  dailyPrice: number;
  rating: number;
  matchReason: string;
}

@Component({
  selector: 'app-ai-recommendation',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './ai-recommendation.html',
  styleUrl: './ai-recommendation.css',
})
export class AiRecommendation {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);

  private apiUrl = environment.backUrl + '/api/Ai/recommend';

  recommendationForm!: FormGroup;
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  recommendations = signal<NurseryRecommendation[]>([]);
  hasResults = signal<boolean>(false);

  constructor() {
    this.initForm();
  }

  private initForm() {
    this.recommendationForm = this.fb.group({
      // babyAgeDays: ['', [Validators.required, Validators.min(1), Validators.max(90)]], // حديثي الولادة حتى 90 يوم مثلاً
      rate: ['', [Validators.required]],
      budget: ['', [Validators.required]],
      locationPreference: ['', [Validators.required]],
      careType: ['', [Validators.required]], // نوع الرعاية الطبية المطلوبة
      extraEquipment: [''], // أجهزة خاصة مثل تنفس صناعي أو علاج ضوئي
    });
  }

  private getAuthHeaders(): { headers: HttpHeaders; isValidParent: boolean; role: string } {
    const sessionData = localStorage.getItem('user_session');
    let token = '';
    let role = '';
    if (sessionData) {
      try {
        const parsed = JSON.parse(sessionData);
        token = parsed.token || '';
        role = parsed.role || '';
      } catch (e) {
        console.error('خطأ في قراءة الجلسة:', e);
      }
    }
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
    return { headers, isValidParent: role === 'Parent', role };
  }

  getRecommendations() {
    console.log('Submitting AI recommendation request...');
    if (this.recommendationForm.invalid) {
      this.recommendationForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.hasResults.set(false);

    const auth = this.getAuthHeaders();
    if (!auth.isValidParent) {
      this.isLoading.set(false);
      this.errorMessage.set(
        `⚠️ عذراً، ميزة الترشيحات الذكية متاحة فقط لحسابات أولياء الأمور (Parent).`,
      );
      return;
    }

    const formValues = this.recommendationForm.value;

    // صياغة الـ Prompt الموجه للـ AI ليفهم أننا نبحث في شبكة حواضن طبية لحديثي الولادة فقط
    const aiPayload = {
      // message: `طلب ترشيح عاجل لحاضنة ذكية لحديث ولادة. عمر الطفل: ${formValues.babyAgeDays} يوم. الميزانية اليومية: ${formValues.budget} ج.م. الموقع: ${formValues.locationPreference}. مستوى الرعاية الطبية المطلوبة: ${formValues.careType}. تجهيزات خاصة مطلوبة: ${formValues.extraEquipment || 'رعاية قياسية'}`,
      message: `عايز حضانة في محافظة ${formValues.locationPreference} سعرها لا يزيد عن ${formValues.budget} جنيه وتقييمها فوق ال 3`,
      latitude: null,
      longitude: null,
    };

    this.http.post<any>(this.apiUrl, aiPayload, { headers: auth.headers }).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        const recommendedData = res.recommendations || res.nurseries || res;

        if (Array.isArray(recommendedData) && recommendedData.length > 0) {
          this.recommendations.set(recommendedData);
          this.hasResults.set(true);
        } else if (res.response || res.message) {
          this.recommendations.set([
            {
              id: 0,
              name: 'تحليل الحواضن المتاحة لحالة طفلك',
              city: formValues.locationPreference,
              district: '',
              dailyPrice: Number(formValues.budget),
              rating: 5,
              matchReason: res.response || res.message,
            },
          ]);
          this.hasResults.set(true);
        } else {
          this.errorMessage.set(
            'لم يعثر الذكاء الاصطناعي على حواضن طبية تطابق هذه المعايير الدقيقة حالياً.',
          );
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.status === 401) {
          this.errorMessage.set('🔴 انتهت صلاحية الجلسة، يرجى إعادة تسجيل الدخول كـ Parent.');
        } else {
          this.errorMessage.set(err.error?.message || 'حدث خطأ أثناء معالجة طلب الحاضنة.');
        }
      },
    });
  }
}
