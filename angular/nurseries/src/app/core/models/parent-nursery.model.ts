export interface NurseryListItem {
  id: number;
  name: string;
  description: string;
  dailyPrice: number;
  city: string;
  district: string;
  ageRangeMin: number;
  ageRangeMax: number;
  avgRating?: number;
  imageUrls?: string[];
}

export interface Review {
  rating: number;
  comment: string;
  parentName?: string; 
  createdAt: any;
}