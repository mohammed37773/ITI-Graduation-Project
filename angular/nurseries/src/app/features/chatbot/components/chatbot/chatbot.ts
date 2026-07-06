import { AfterViewChecked, Component, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';

interface Message {
  text: string;
  isUser: boolean;
  timestamp: Date;
  role: 'user' | 'assistant';
}

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './chatbot.html',
  styleUrl: './chatbot.css',
})
export class Chatbot implements AfterViewChecked {
  private http = inject(HttpClient);

  private apiUrl = environment.backUrl + '/api/Ai/chat';

  @ViewChild('chatContainer') chatContainer!: ElementRef;

  userMessage = signal<string>('');
  messages = signal<Message[]>([
    {
      text: 'مرحباً بك في المساعد الذكي لشبكة حضاناتنا. 👶💚 يمكنك سؤالي عن الحضانات المتاحة، الأنشطة، أو البحث عن حضانة مناسبة لطفلك.',
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

    // 1️⃣ جلب بيانات الجلسة
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

    // 2️⃣ الحماية الأمنية — منع غير الـ Parent
    if (userRole !== 'Parent') {
      this.messages.set([
        ...this.messages(),
        { text, isUser: true, role: 'user', timestamp: new Date() },
        {
          text: `⚠️ عذراً يا ريس، حسابك الحالي هو (${userRole}). نظام الذكاء الاصطناعي متاح فقط لأولياء الأمور (Parent). يرجى تسجيل الدخول بحساب ولي أمر لتجربة الشات بوت.`,
          isUser: false,
          role: 'assistant',
          timestamp: new Date()
        }
      ]);
      this.userMessage.set('');
      return;
    }

    // 3️⃣ ✅ بناء الـ history من الرسائل السابقة فقط — قبل إضافة الرسالة الحالية للـ messages
    //    عشان الرسالة الحالية متتبعتش مرتين (مرة في history ومرة في message)
    const history = this.messages()
      .slice(1) // شيل رسالة الترحيب الأولى
      .map((m: Message) => ({
        role: m.role,
        content: m.text
      }));

    // 4️⃣ إضافة رسالة المستخدم للشاشة بعد بناء الـ history
    this.messages.set([
      ...this.messages(),
      { text, isUser: true, role: 'user', timestamp: new Date() }
    ]);
    this.userMessage.set('');
    this.isTyping.set(true);

    // 5️⃣ بناء الـ Headers
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${savedToken}`,
      'Content-Type': 'application/json'
    });

    // 6️⃣ بناء الـ Body مطابق للـ ChatRequestDto
    const requestBody = {
      message: text,
      history: history,  // ✅ history صحيحة — مش فيها الرسالة الحالية
      latitude: null,
      longitude: null
    };

    // 7️⃣ إرسال الطلب
    this.http.post<any>(this.apiUrl, requestBody, { headers }).subscribe({
      next: (res: any) => {
        this.isTyping.set(false);

        // ✅ استخدام ?? بدل || عشان يتعامل صح مع القيم الفارغة
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

        if (err.status === 401)
          errorText = '🔴 انتهت صلاحية الجلسة أو الـ Token غير صالح (401 Unauthorized). يرجى إعادة تسجيل الدخول.';
        else if (err.status === 403)
          errorText = '🚫 غير مسموح لحسابك بالوصول لهذا المورد (403 Forbidden).';
        else if (err.error?.message)
          errorText = err.error.message;

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