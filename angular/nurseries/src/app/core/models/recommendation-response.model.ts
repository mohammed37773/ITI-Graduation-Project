import { NurseryModel } from "./nursery.model";

export interface RecommendationResponse {
     message: string;

  recommendedNurseries: NurseryModel[];
}
