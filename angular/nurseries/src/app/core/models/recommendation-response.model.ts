import { Nursery } from "./nursery.model";

export interface RecommendationResponse {
     message: string;

  recommendedNurseries: Nursery[];
}
