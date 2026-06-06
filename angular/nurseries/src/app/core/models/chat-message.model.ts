export interface ChatMessage {
    id?: number;

  content: string;

  sender: 'user' | 'bot';

  timestamp: Date;
}
