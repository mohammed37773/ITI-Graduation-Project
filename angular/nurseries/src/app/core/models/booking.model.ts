export interface Booking {
  id: number;
  nurseryId: number;
  nurseryName: string;
  childId: number;
  childName: string;
  startDate: string;
  status: number | 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed';
  totalPrice: number;
}
