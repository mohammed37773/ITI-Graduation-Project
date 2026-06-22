export interface NurseryListItem {
  id: number;
  name: string;
  description: string;
  dailyPrice: number;
  city: string;
  district: string;
  rating?: number;
  images?: { imageUrl: string }[];
}

export interface Review {
  id: number;
  rating: number;
  comment: string;
  parentName?: string; 
}