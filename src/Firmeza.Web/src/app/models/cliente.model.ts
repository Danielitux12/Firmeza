export interface Cliente {
  id?: string;
  name: string;
  documentNumber: string;
  email: string;
  phone?: string;
  age?: number;
  address?: string;
  userId?: string;
  role?: string;
  isActive: boolean;
}

export interface SaveCliente {
  id?: string;
  name: string;
  documentNumber: string;
  email: string;
  phone?: string;
  age?: number;
  address?: string;
  isActive?: boolean;
}
