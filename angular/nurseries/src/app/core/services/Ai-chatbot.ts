import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment.prod';
import { ChatMessage } from '../models/chat-message.model';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ChatbotService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/aichat`; // مسار الـ AI Controller في الـ Backend

  // استخدام Signal لحفظ الرسائل وعرضها في الـ UI بشكل تلقائي وسريع
  chatHistory = signal<ChatMessage[]>([]);
  
  // دالة لبدء محادثة جديدة أو جلب المحادثة القديمة من الـ DB بناءً على الـ RAG
  getConversationMessages(conversationId: number): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.apiUrl}/conversations/${conversationId}`).pipe(
      tap(messages => this.chatHistory.set(messages))
    );
  }

  // دالة إرسال الرسالة للـ Backend (الـ .NET بياخدها ويبعتها لـ OpenAI مع الـ Context من قاعدة البيانات)
  sendMessage(conversationId: number, messageText: string): Observable<ChatMessage> {
    const payload = {
      conversationId: conversationId,
      messageText: messageText,
      sender: 'user'
    };

    // 1. ضيف رسالة المستخدم فوراً في الـ UI قبل ما الـ API يرد عشان الشات يبقى سريع
    const userMessage: ChatMessage = { conversationId, messageText, sender: 'user', createdAt: new Date() };
    this.chatHistory.update(messages => [...messages, userMessage]);

    // 2. ابعت الطلب للـ Backend
    return this.http.post<ChatMessage>(`${this.apiUrl}/send`, payload).pipe(
      tap((aiResponse) => {
        // 3. لما الـ الـ AI يرد (RAG Response)، ضيف رده في مصفوفة الشات
        this.chatHistory.update(messages => [...messages, aiResponse]);
      })
    );
  }

  // دالة لتنظيف الشات لو الأب عايز يبدأ من جديد
  clearChat() {
    this.chatHistory.set([]);
  }
}
