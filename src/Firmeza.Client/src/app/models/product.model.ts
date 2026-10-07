export enum ProductStatus {
  Available = 1,
  Unavailable = 2,
  Discontinued = 3
}

export interface Product {
  id?: number;
  empresaId?: number | null;
  name: string;
  price: number;
  status?: ProductStatus;
  isActive?: boolean;
}
