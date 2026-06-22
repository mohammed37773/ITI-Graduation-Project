export interface InitiatePaymentDto {
  bookingId: number;
  paymentMethod: number;
}

export interface PaymentResponse {
  paymentId: number;
  paymentUrl: string;
}

export interface PaymentStatusResponse {
  paymentId: number;
  status: 'Pending' | 'Completed' | 'Failed';
  amount: number;
  method: string;
  paidAt?: string;
}