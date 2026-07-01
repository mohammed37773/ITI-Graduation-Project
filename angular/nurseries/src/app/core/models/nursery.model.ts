import { NurseryLocation } from '../models/nursery-location.model';

export interface Nursery {
  id?: number;
  name: string;
  description: string;
  dailyPrice: number;
  ageRangeMin: number;
  ageRangeMax: number;
  capacity: number;
  address: string;
  city: string;
  district: string;
  latitude: number;
  longitude: number;
  imageUrls?: string[];
}

// export interface NurseryImage {
//   id: number;
//   imageUrl: string;
// }
