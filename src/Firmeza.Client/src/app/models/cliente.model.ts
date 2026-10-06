export interface Cliente {
  id?: number;
  name: string;
  documentNumber: string;
  email: string;
  phone?: string;
  age?: number;
  address: string;
  userId?: string;
  isActive: boolean;
}
