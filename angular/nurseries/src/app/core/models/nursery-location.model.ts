export interface NurseryLocation {
  id: number;
  nurseryId: number;
  address: string;
  city: string;
  district?: string;
  latitude?: number;
  longitude?: number;
  region?: string;
}