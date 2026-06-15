import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ChatMessage } from '../models/chat-message.model';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ChatbotService {
  private http = inject(HttpClient);
  
  // Base URL pointing to your Langflow instance
  private apiUrl = `${environment.apiUrl}`
  private apiKey = `${environment.apiKey}`; 
  chatHistory = signal<ChatMessage[]>([]);
  
  getConversationMessages(conversationId: number): Observable<ChatMessage[]> {
    // Keeping your existing history fetcher if needed for your database
    // return this.http.get<ChatMessage[]>(`${environment.apiUrl}/conversations/${conversationId}`).pipe(
    return this.http.get<ChatMessage[]>(`${environment.apiUrl}/`).pipe(
      tap(messages => this.chatHistory.set(messages))
    );
  }

  sendMessage(conversationId: number, messageText: string): Observable<any> {
    // 1. Build the specific payload expected by the API
    const payload = {
      output_type: 'chat',
      input_type: 'chat',
      input_value: messageText,
      session_id: conversationId.toString() // Using conversationId as the session tracking ID
    };

    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'x-api-key': this.apiKey
    });

    const userMessage: ChatMessage = { 
      conversationId, 
      messageText, 
      sender: 'user', 
      createdAt: new Date() 
    };
    this.chatHistory.update(messages => [...messages, userMessage]);

    // 4. Send the POST request
    return this.http.post<any>(this.apiUrl, payload, { headers }).pipe(
      tap((apiResponse) => {

        const aiText = apiResponse?.outputs?.[0]?.outputs?.[0]?.results?.message?.text || 'Sorry. I am not available now.\n please try later';

        const aiMessage: ChatMessage = {
          conversationId,
          messageText: aiText,
          sender: 'ai',
          createdAt: new Date()
        };

        // Update the signal with the AI response
        this.chatHistory.update(messages => [...messages, aiMessage]);
      })
    );
  }

  clearChat() {
    this.chatHistory.set([]);
  }
}