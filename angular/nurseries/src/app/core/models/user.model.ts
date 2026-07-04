export interface User {
  id: string;
  fullName: string;
  email: string;
  emailConfirmed: boolean;
  phoneNumber: string;
  locationLat?: number;
  locationLng?: number;
  createdAt: Date;
  lockoutEnd: Date | null;
  roles: string[];
}
