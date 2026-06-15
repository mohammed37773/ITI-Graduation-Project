export interface User {
    id: number;
    fullName: string;
    email: string;
    phoneNumber: string;
    locationLat?: number;
    locationLng?: number;
    createdAt: Date;
}
