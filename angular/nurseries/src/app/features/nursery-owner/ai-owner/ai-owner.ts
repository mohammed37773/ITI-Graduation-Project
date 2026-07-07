import { AfterViewChecked, Component, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment'; 

interface Message {
  text: string;
  isUser: boolean;
  timestamp: Date;
  role: 'user' | 'assistant';
}

@Component({
  selector: 'app-ai-owner',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './ai-owner.html',
  styleUrl: './ai-owner.css',
})
export class AiOwner implements AfterViewChecked {
  private http = inject(HttpClient);
  private apiUrl = environment.backUrl + '/api/Ai/admin-agent';

  @ViewChild('chatContainer') chatContainer!: ElementRef;

  // تعريف الـ Signal للمدخلات
  userMessage = signal<string>('');
  
  messages = signal<Message[]>([
    {
      text: `مرحباً بك يا فندم! أنا المساعد الذكي الخاص بإدارة حضانتك. كيف يمكنني مساعدتك اليوم؟`,
      isUser: false,
      role: 'assistant',
      timestamp: new Date()
    }
  ]);
  
  isTyping = signal<boolean>(false);

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  sendMessage() {
    const text = this.userMessage().trim();
    if (!text) return;

    // 1️⃣ جلب بيانات الجلسة من الـ LocalStorage
    const sessionData = localStorage.getItem('user_session');
    let savedToken = '';
    let userRole = '';

    if (sessionData) {
      try {
        const parsedSession = JSON.parse(sessionData);
        savedToken = parsedSession.token || '';
        userRole = parsedSession.role || '';
      } catch (e) {
        console.error('خطأ أثناء قراءة الـ user_session:', e);
      }
    }

    // 2️⃣ الحماية الأمنية — السماح فقط للـ NurseryAdmin (صاحب الحضانة) بالدخول
    if (userRole !== 'NurseryAdmin') {
      this.messages.set([
        ...this.messages(),
        { text, isUser: true, role: 'user', timestamp: new Date() },
        {
          text: `⚠️ عذراً يا ريس، حسابك الحالي هو (${userRole}). نظام المساعد الذكي للإدارة متاح فقط لأصحاب الحضانات (NurseryAdmin). يرجى تسجيل الدخول بالحساب الصحيح.`,
          isUser: false,
          role: 'assistant',
          timestamp: new Date()
        }
      ]);
      this.userMessage.set(''); 
      return;
    }

    // 3️⃣ بناء الـ history من الرسائل السابقة فقط
    const history = this.messages()
      .slice(1) // تخطي رسالة الترحيب الأولى
      .map((m: Message) => ({
        role: m.role,
        content: m.text
      }));

    // 4️⃣ إضافة رسالة المستخدم الحالية للشاشة
    this.messages.set([
      ...this.messages(),
      { text, isUser: true, role: 'user', timestamp: new Date() }
    ]);
    
    // تفريغ قيمة الـ Input فوراً وتفعيل مؤشر الكتابة
    this.userMessage.set('');
    this.isTyping.set(true);

    // 5️⃣ بناء الـ Headers وتمرير الـ Token
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${savedToken}`,
      'Content-Type': 'application/json'
    });

    // 6️⃣ بناء الـ Body المطابق للـ Swagger DTO الخاص بك
    const requestBody = {
      message: text,
      history: history,
      latitude: null,
      longitude: null
    };

    // 7️⃣ إرسال الطلب إلى الـ API
    this.http.post<any>(this.apiUrl, requestBody, { headers }).subscribe({
      next: (res: any) => {
        this.isTyping.set(false);

        // استخراج الرد بأمان بناءً على المتوقع من السيرفر
        const botReply = res.response ?? res.reply ?? res.message ?? 'تمت معالجة استفسارك بنجاح!';

        this.messages.set([
          ...this.messages(),
          { text: botReply, isUser: false, role: 'assistant', timestamp: new Date() }
        ]);
      },
      error: (err: any) => {
        this.isTyping.set(false);
        console.error('❌ خطأ في الاتصال بالـ Chatbot API:', err);

        let errorText = 'عذراً، واجهت مشكلة في الاتصال بالمساعد الذكي حالياً.';

        if (err.status === 401) {
          errorText = '🔴 انتهت صلاحية الجلسة أو الـ Token غير صالح (401 Unauthorized). يرجى إعادة تسجيل الدخول.';
        } else if (err.status === 403) {
          errorText = '🚫 غير مسموح لحسابك بالوصول لهذا المورد (403 Forbidden).';
        } else if (err.error?.message) {
          errorText = err.error.message;
        }

        this.messages.set([
          ...this.messages(),
          { text: errorText, isUser: false, role: 'assistant', timestamp: new Date() }
        ]);
      }
    });
  }

  private scrollToBottom() {
    try {
      this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
    } catch (err) {}
  }
}