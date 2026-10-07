export interface Empresa {
  id?: number;
  name: string;
  nit: string;
  email: string;
  phone?: string;
  address?: string;
  isActive: boolean;
}