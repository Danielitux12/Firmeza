export enum EmployeeStatus {
  Active = 1,
  Inactive = 2,
  Pending = 3
}

export interface Employee {
  id?: number;
  firstName: string;
  lastName: string;
  documentNumber: string;
  email: string;
  phone?: string;
  position: string;
  salary: number;
  isActive: boolean;
}
