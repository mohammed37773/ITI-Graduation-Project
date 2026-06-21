export interface Booking {
id: number;
  nurseryId: number;
  nurseryName?: string;
  childFullName?: string;
  startDate: string;
  status: 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed';
  amount?: number;
}
