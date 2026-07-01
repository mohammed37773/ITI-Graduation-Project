export interface ChatMessage {
    id?: number;
  conversationId: number;
  sender: 'user' | 'ai';
  messageText: string;
  createdAt?: Date;
}
