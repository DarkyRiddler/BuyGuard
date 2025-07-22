export interface User
{
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  managerLimitPln: number;
  role: Role;
}
export interface Request
{
  title: string;
  amount_pln: number;
  description: string;
  reason: string;
  link: string;
}

export type Role = 'admin' | 'manager' | 'employee';