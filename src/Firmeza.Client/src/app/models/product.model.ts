export enum ProductStatus {
  Available = 1,
  Unavailable = 2,
  Discontinued = 3
}

export interface Product {
  id?: number;
  name: string;
  price: number;
  isAvailable?: boolean;
}
