export interface NurseryListItem {
  id: number;
  name: string;
  description: string;
  dailyPrice: number;
  city: string;
  district: string;
  capacity: number;
  ageRangeMin: number;
  ageRangeMax: number;
  avgRating?: number;
  imageUrls?: string[];
  latitude?: string;
  longitude?: string
}

export interface Review {
  rating: number;
  comment: string;
  parentName?: string; 
  createdAt: any;
}