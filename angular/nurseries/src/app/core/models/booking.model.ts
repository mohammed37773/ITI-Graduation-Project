export interface Booking {
  id: number;
  nurseryId: number;
  childId: number;
  startDate: string;
  status: number | 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed';
  amount: number;
}
