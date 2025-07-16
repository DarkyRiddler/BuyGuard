export interface User
{
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  managerLimitPln: number;
  role: Role;
}

export type Role = 'admin' | 'manager' | 'employee';