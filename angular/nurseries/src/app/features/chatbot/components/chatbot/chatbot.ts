import { AfterViewChecked, Component, ElementRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface Message {
  text: string;
  isUser: boolean;
  timestamp: Date;
}

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule , FormsModule],
  templateUrl: './chatbot.html',
  styleUrl: './chatbot.css',
})
export class Chatbot implements AfterViewChecked {
  private http = inject(HttpClient);
  

  @ViewChild('chatContainer') chatContainer!: ElementRef;

  userMessage = signal<string>('');
  messages = signal<Message[]>([
    {
      text: 'مرحباً بك في المساعد الطبي الذكي لشبكة IncuCare. 👶💚 يمكنك سؤالي عن أي استفسار طبي يخص الأطفال المبتسرين أو العناية المركزة لحديثي الولادة (NICU).',
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

    // 1. إضافة رسالة المستخدم فوراً للشاشة
    const currentMessages = this.messages();
    this.messages.set([...currentMessages, { text, isUser: true, timestamp: new Date() }]);
    this.userMessage.set('');
    this.isTyping.set(true);

    // 2. إرسال السؤال لـ API الـ .NET (تأكد من تعديل الـ URL للـ Endpoint الخاص بك)
    this.http.post<any>('https://localhost:7195/api/Parent/chatbot', { question: text }).subscribe({
      next: (res) => {
        this.isTyping.set(false);
        this.messages.set([
          ...this.messages(),
          { text: res.reply || res.response || 'عذراً، لم أستطع فهم الإجابة حالياً.', isUser: false, timestamp: new Date() }
        ]);
      },
      error: (err) => {
        this.isTyping.set(false);
        console.error('خطأ في الشات بوت:', err);
        this.messages.set([
          ...this.messages(),
          { text: 'عذراً، واجهت مشكلة في الاتصال بالخادم الطبي الذكي. يرجى المحاولة لاحقاً.', isUser: false, timestamp: new Date() }
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