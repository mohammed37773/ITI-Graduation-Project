import { AfterViewChecked, Component, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http'; // 👈 استيراد HttpHeaders

interface Message {
  text: string;
  isUser: boolean;
  timestamp: Date;
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
  
  private apiUrl = 'http://localhost:5104/api/Ai/chat';

  @ViewChild('chatContainer') chatContainer!: ElementRef;

  userMessage = signal<string>('');
  messages = signal<Message[]>([
    {
      text: 'مرحباً بك في المساعد الذكي لشبكة حضاناتنا. 👶💚 يمكنك سؤالي عن الحضانات المتاحة، الأنشطة، أو البحث عن حضانة مناسبة لطفلك.',
      isUser: false,
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

    // 1️⃣ جلب بيانات الجلسة بالكامل لفحص الـ Role والـ Token
    const sessionData = localStorage.getItem('user_session');
    let savedToken = '';
    let userRole = '';

    if (sessionData) {
      try {
        const parsedSession = JSON.parse(sessionData);
        savedToken = parsedSession.token || '';
        userRole = parsedSession.role || ''; // لقط الـ Role (مثل NurseryAdmin أو Parent)
      } catch (e) {
        console.error('خطأ أثناء قراءة الـ user_session جوه الشات بوت:', e);
      }
    }

    // 2️⃣ الحماية الأمنية: منع غير الـ Parent من استهلاك الـ API طبقاً لتعليمات الباك إند
    if (userRole !== 'Parent') {
      this.messages.set([
        ...this.messages(),
        { text: text, isUser: true, timestamp: new Date() },
        { 
          text: `⚠️ عذراً يا ريس، حسابك الحالي هو (${userRole}). نظام الذكاء الاصطناعي متاح فقط لأولياء الأمور (Parent). يرجى تسجيل الدخول بحساب ولي أمر لتجربة الشات بوت.`, 
          isUser: false, 
          timestamp: new Date() 
        }
      ]);
      this.userMessage.set('');
      return;
    }

    // 3️⃣ إضافة رسالة المستخدم للشاشة بعد تخطي الفحص
    const currentMessages = this.messages();
    this.messages.set([...currentMessages, { text, isUser: true, timestamp: new Date() }]);
    this.userMessage.set('');
    this.isTyping.set(true);

    // 4️⃣ بناء الـ Headers بشكل صارم وصحيح باستخدام HttpHeaders لضمان عدم اختفائها
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${savedToken}`,
      'Content-Type': 'application/json'
    });

    // 5️⃣ بناء الـ Body مطابق للـ ChatRequestDto بالملي
    const requestBody = {
      message: text,
      latitude: null,
      longitude: null
    };

    // 6️⃣ إرسال الطلب
    this.http.post<any>(this.apiUrl, requestBody, { headers }).subscribe({
      next: (res) => {
        this.isTyping.set(false);
        
        // تأمين قراءة الرد بأي شكل يرجعه السيرفر
        const botReply = res.response || res.reply || res.message || 'تمت معالجة استفسارك بنجاح!';
        
        this.messages.set([
          ...this.messages(),
          { text: botReply, isUser: false, timestamp: new Date() }
        ]);
      },
      error: (err) => {
        this.isTyping.set(false);
        console.error('❌ خطأ في الاتصال بالـ Chatbot API:', err);
        
        let errorText = 'عذراً، واجهت مشكلة في الاتصال بالمساعد الذكي حالياً.';
        
        if (err.status === 401) {
          errorText = '🔴 انتهت صلاحية الجلسة أو الـ Token غير صالح (401 Unauthorized). يرجى إعادة تسجيل الدخول.';
        } else if (err.status === 403) {
          errorText = '🚫 غير مسموح لحسابك بالوصول لهذا المورد (403 Forbidden).';
        } else if (err.error && err.error.message) {
          errorText = err.error.message;
        }

        this.messages.set([
          ...this.messages(),
          { text: errorText, isUser: false, timestamp: new Date() }
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