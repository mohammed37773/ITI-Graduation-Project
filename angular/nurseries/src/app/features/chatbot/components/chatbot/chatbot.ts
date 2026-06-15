import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ChatbotService } from '../../../../core/services/Ai-chatbot';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-chatbot',
  imports: [CommonModule , ReactiveFormsModule],
  templateUrl: './chatbot.html',
  styleUrl: './chatbot.css',
})
export class Chatbot {
  private fb = inject(FormBuilder);
  constructor( readonly chatService: ChatbotService) {} // عملنا Inject للـ Service

  chatForm: FormGroup = this.fb.group({
    message: ['', Validators.required]
  });

  // افتراضياً هنثبت رقم المحادثة بـ 1، ومستقبلاً الـ Backend هيعمل لوود للـ Active Session
  currentConversationId = 1; 
  isLoading = false; // مؤشر تحميل يعبر عن تفكير الـ AI (Thinking...)

  ngOnInit() {
    // جلب الرسائل السابقة أول ما يفتح الشاشة عشان الـ RAG يكمل المحادثة
    this.chatService.getConversationMessages(this.currentConversationId).subscribe();
  }

  onSendMessage() {
    if (this.chatForm.valid) {
      const userText = this.chatForm.value.message;
      this.chatForm.reset(); // فضي خانة الكتابة فوراً
      this.isLoading = true;  // شغل الـ Spinner بتاع الـ AI

      this.chatService.sendMessage(this.currentConversationId, userText).subscribe({
        next: (res) => {
          this.isLoading = false; // الـ AI رد بنجاح، اقفل الـ Spinner
          this.scrollToBottom();   // انزل بالشات لتحت عشان يبان الرد الجديد
        },
        error: (err) => {
          console.error('AI Chat Error:', err);
          this.isLoading = false;
        }
      });
    }
  }

  private scrollToBottom() {
    setTimeout(() => {
      const chatContainer = document.querySelector('.chat-messages');
      if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
      }
    }, 50);
  }
}
