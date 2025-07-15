import { cookies } from 'next/headers';
import axios from '@/lib/utils';
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { User } from '@/types';
import ResetPasswordDialog from '@/components/reset-password-dialog';

export default async function Account() {
  const cookieStore = await cookies();
  const token = cookieStore.get('jwt')?.value;

  if (!token) {
    return (
      <div className="flex flex-col justify-center items-center h-screen">
        <p className="text-red-500">Brak tokena – nie zalogowano.</p>
      </div>
    );
  }
  
  const roles: Record<string, string> = {
    'admin': 'Administrator',
    'manager': 'Manager',
    'employee': 'Użytkownik'
  }
  

  try {
    const { data } = await axios.get('/api/Users/me', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    console.log('User data:', data);
    
    const user: User = data.user;
    
    return (
      <div className="flex flex-col justify-center items-center h-screen">
        <Card className={'min-w-100'}>
          <CardHeader>
            <CardTitle className={'mx-auto text-2xl'}><span className="font-bold text-gray-700">{user.firstName} {user.lastName}</span></CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2 w-full">
              <div className="flex justify-between">
                <span className="font-semibold text-gray-700">Adres e-mail: </span>
                <span>{user.email}</span>
              </div>
              <div className="flex justify-between">
                <span className="font-semibold text-gray-700">Rola: </span>
                <span className="capitalize">{roles[user.role]}</span>
              </div>
              <div className="flex justify-between">
                <span className="font-semibold text-gray-700">Limit: </span>
                <span className="capitalize">{user.managerLimitPln}</span>
              </div>
            </div>
          </CardContent>
          <CardFooter>
            <ResetPasswordDialog />
          </CardFooter>
        </Card>
      </div>
    );
  } catch (error) {
    console.error('Error fetching user data:', error);
    return (
      <div className="flex flex-col justify-center items-center h-screen">
        <p className="text-red-500">Nie udało się pobrać danych użytkownika.</p>
      </div>
    );
  }
}