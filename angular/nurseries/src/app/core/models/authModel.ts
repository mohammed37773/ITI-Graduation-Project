export interface RegisterDto {
  fullName: string | null;
  email: string | null;
  password: string | null;
  role: string | null; 
}


export interface LoginDto {
  email: string | null;
  password: string | null;
}


export interface AuthResponseDto {
  id: string
  token: string;
  fullName: string;
  email: string;
  role: string;
}