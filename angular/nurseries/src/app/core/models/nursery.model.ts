import { Review } from './review.model';
import { NurseryImage } from './nursery-image.model';
export interface Nursery {
    id: number;
    name: string;
    description: string;
    address: string;
    city: string;

    dailyPrice: number;
    monthlyPrice: number;

    latitude: number;
    longitude: number;

    averageRating: number;

    imageUrl: string;

    isAvailable: boolean;

    reviews?: Review[];

    images?: NurseryImage[];
}
