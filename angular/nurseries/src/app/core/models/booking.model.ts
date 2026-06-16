export interface Booking {
id: number;
  nurseryId: number;
  nurseryName: string;
  childName: string;
  childAge: number;
  startDate: Date;
  status: 'pending' | 'approved' | 'rejected'; // حالة الطلب
  dailyPrice: number;
}
