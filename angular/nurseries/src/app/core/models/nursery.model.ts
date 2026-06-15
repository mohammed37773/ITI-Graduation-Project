import { Review } from './review.model';
import { NurseryImage } from './nursery-image.model';
import { NurseryLocation } from '../models/nursery-location.model';
export interface Nursery {
   id: number;
  name: string;
  description: string;
  dailyPrice: number;
  ageRangeMin: number;
  ageRangeMax: number;
  capacity: number;
  avgRating: number;
  isVerified: boolean;
  location?: NurseryLocation;
  images?: NurseryImage[];
}
