import { useEffect, useState } from 'react';
import axios from '@/lib/utils';
import { Role } from '@/types';

export function useUserRole() {
  const [role, setRole] = useState<Role | null>(null);

  useEffect(() => {
    axios.get('/api/Users/me')
    .then((res) => {
      setRole(res.data.user.role);
    })
    .catch(() => setRole(null));
  }, []);

  return role;
}