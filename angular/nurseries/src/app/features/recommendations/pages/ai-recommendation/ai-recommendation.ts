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
  address: string;
  dailyPrice: number;
  avgRating: number;
  matchReason?: string;
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
  aiResponseText = signal<string | null>(null); // ✅ نص رد الـ AI منفصل
  hasResults = signal<boolean>(false);

  constructor() {
    this.initForm();
  }

  private initForm() {
    this.recommendationForm = this.fb.group({
      rate: ['', [Validators.required]],
      budget: ['', [Validators.required]],
      locationPreference: ['', [Validators.required]],
      careType: ['', [Validators.required]],
      extraEquipment: [''],
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
    if (this.recommendationForm.invalid) {
      this.recommendationForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.hasResults.set(false);
    this.aiResponseText.set(null);
    this.recommendations.set([]);

    const auth = this.getAuthHeaders();
    if (!auth.isValidParent) {
      this.isLoading.set(false);
      this.errorMessage.set('⚠️ عذراً، ميزة الترشيحات الذكية متاحة فقط لحسابات أولياء الأمور.');
      return;
    }

    const formValues = this.recommendationForm.value;

    // ✅ بناء رسالة واضحة للـ AI مع كل معايير البحث
    const message = `عايز حضانة في ${formValues.locationPreference} سعرها لا يزيد عن ${formValues.budget} جنيه يومياً وتقييمها فوق ${formValues.rate}`;

    const aiPayload = {
      message,
      history: [],   // ✅ مطلوب من الـ backend (ChatRequestDto)
      latitude: null,
      longitude: null,
    };

    console.log('AI Payload:', aiPayload.message);

    this.http.post<any>(this.apiUrl, aiPayload, { headers: auth.headers }).subscribe({
      next: (res) => {
        this.isLoading.set(false);

        // ✅ الـ backend بيرجع { response: "...", nurseries: [...] }
        const aiText: string = res.response ?? '';
        const nurseriesList: any[] = res.nurseries ?? [];

        // ✅ لو فيه حضانات فعلية من الداتابيز، اعرضهم
        if (nurseriesList.length > 0) {
          this.recommendations.set(
            nurseriesList.map((n: any) => ({
              id: n.id,
              name: n.name,
              city: n.city ?? '',
              address: n.address ?? '',
              dailyPrice: n.dailyPrice,
              avgRating: n.avgRating,
              matchReason: aiText, // رد الـ AI كسبب للترشيح
            }))
          );
          this.hasResults.set(true);
        } else if (aiText) {
          // ✅ لو مفيش حضانات بس فيه رد نصي من الـ AI (مثلاً "مفيش حضانات في المنطقة دي")
          this.aiResponseText.set(aiText);
          this.hasResults.set(true);
        } else {
          this.errorMessage.set('لم يعثر الذكاء الاصطناعي على حضانات تطابق هذه المعايير حالياً.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        console.error('خطأ في الاتصال بالـ AI API:', err);
        if (err.status === 401) {
          this.errorMessage.set('🔴 انتهت صلاحية الجلسة، يرجى إعادة تسجيل الدخول.');
        } else if (err.status === 403) {
          this.errorMessage.set('🚫 غير مسموح لحسابك بالوصول لهذه الميزة.');
        } else {
          this.errorMessage.set(err.error?.message || 'حدث خطأ أثناء معالجة الطلب.');
        }
      },
    });
  }
}