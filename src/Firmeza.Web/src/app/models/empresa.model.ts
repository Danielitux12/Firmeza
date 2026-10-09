export interface Empresa {
  id?: string;
  name: string;
  nit: string;
  email: string;
  phone?: string;
  address?: string;
  isActive: boolean;
}

export interface SaveEmpresa {
  id?: string;
  name: string;
  nit: string;
  email: string;
  phone?: string;
  address?: string;
  isActive?: boolean;
}