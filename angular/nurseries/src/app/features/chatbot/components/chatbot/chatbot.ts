import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ChatbotService } from '../../../../core/services/Ai-chatbot';
import { CommonModule } from '@angular/common';

interface Message {
  sender: 'user' | 'ai';
  text: string;
  timestamp: Date;
}

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './chatbot.html',
  styleUrl: './chatbot.css',
})
export class Chatbot implements OnInit {
  private fb = inject(FormBuilder);
  readonly chatService = inject(ChatbotService);

  // تعريف الـ Signals لإدارة الـ State بقوة أنجولار 21
  messages = signal<Message[]>([]);
  isLoading = signal<boolean>(false); 
  currentConversationId = signal<number>(1); 

  chatForm: FormGroup = this.fb.group({
    message: ['', Validators.required]
  });

  ngOnInit() {
    this.loadChatHistory();
  }

  // 1. جلب تاريخ المحادثة من الـ API عند فتح الشاشة
  loadChatHistory() {
    this.isLoading.set(true);
    this.chatService.getConversationMessages(this.currentConversationId()).subscribe({
      next: (history: any) => {
        if (history && history.length > 0) {
          const mappedMessages = history.map((msg: any) => ({
            sender: msg.role === 'user' ? 'user' : 'ai',
            text: msg.content || msg.text,
            timestamp: new Date(msg.createdAt || new Date())
          }));
          this.messages.set(mappedMessages);
        }
        this.isLoading.set(false);
        this.scrollToBottom();
      },
      error: (err) => {
        console.error('فشل في تحميل المحادثات السابقة:', err);
        this.isLoading.set(false);
      }
    });
  }

  // 2. إرسال الرسالة وربطها بالـ AI API
  onSendMessage() {
    if (this.chatForm.invalid || this.isLoading()) return;

    const userText = this.chatForm.value.message.trim();
    if (!userText) return;

    // عرض رسالة المستخدم فوراً (Optimistic UI)
    this.messages.update(prev => [...prev, {
      sender: 'user',
      text: userText,
      timestamp: new Date()
    }]);

    this.chatForm.reset(); 
    this.isLoading.set(true); // تشغيل لودر التفكير فوراً
    this.scrollToBottom(); 

    // استدعاء الـ API الحقيقي
    this.chatService.sendMessage(this.currentConversationId(), userText).subscribe({
      next: (res: any) => {
        this.isLoading.set(false); // إيقاف الـ لودر بعد الرد
        
        const aiReply = res?.reply || res?.response || res?.content || 'عذراً، لم أستطع معالجة الرد.';
        
        this.messages.update(prev => [...prev, {
          sender: 'ai',
          text: aiReply,
          timestamp: new Date()
        }]);

        this.scrollToBottom();
      },
      error: (err) => {
        console.error('خطأ في الـ AI API:', err);
        this.isLoading.set(false);
        
        this.messages.update(prev => [...prev, {
          sender: 'ai',
          text: '⚠️ عذراً، حدث خطأ أثناء الاتصال بالخادم. يرجى المحاولة مرة أخرى.',
          timestamp: new Date()
        }]);
        this.scrollToBottom();
      }
    });
  }

  // 3. مسح المحادثة بالكامل وتصفير الشاشة
  onClearChat() {
    if (confirm('هل أنت متأكد من رغبتك في مسح هذه المحادثة بالكامل؟')) {
      this.messages.set([]); // تصفير الـ Signal لايف
    }
  }

  private scrollToBottom() {
    setTimeout(() => {
      const chatContainer = document.querySelector('.chat-messages');
      if (chatContainer) {
        chatContainer.scrollTo({
          top: chatContainer.scrollHeight,
          behavior: 'smooth'
        });
      }
    }, 50);
  }
}