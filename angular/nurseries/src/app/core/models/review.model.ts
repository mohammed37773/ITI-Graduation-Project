export interface Review {
    id: number;
  rating: number;
  comment: string;

  userId: string;
  userName: string;

  nurseryId: number;

  createdAt: Date;
}
