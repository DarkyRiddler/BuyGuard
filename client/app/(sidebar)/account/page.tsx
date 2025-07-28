import { cookies } from 'next/headers';
import axios from '@/lib/utils';
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { User } from '@/types';
import ResetPasswordDialog from '@/components/forms/reset-password-dialog';
import { LogoutButton } from '@/components/buttons/logout';
import { CompanySettingsForm } from '@/components/forms/company-settings-form';

export default async function Account() {
  const cookieStore = await cookies();
  const token = cookieStore.get('jwt')?.value;

  const roles: Record<string, string> = {
    'admin': 'Administrator',
    'manager': 'Manager',
    'employee': 'Użytkownik',
  };
  
  try {
    const { data } = await axios.get('/api/Users/me', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    const user: User = data.user;

    return (

      <Card className={'min-w-150'}>

        <CardHeader>
          <CardTitle className={'mx-auto text-2xl'}><span
            className="font-bold text-slate-950 dark:text-sky-50">{user.firstName} {user.lastName}</span></CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2 w-full text-2xl mb-5 px-6">
            <div className="flex justify-between">
              <span className="font-semibold text-slate-950 dark:text-sky-50">Adres e-mail: </span>
              <span>{user.email}</span>
            </div>
            <div className="flex justify-between">
              <span className="font-semibold text-slate-950 dark:text-sky-50">Rola: </span>
              <span className="capitalize">{roles[user.role]}</span>
            </div>
            <div className="flex justify-between">
              <span className="font-semibold text-slate-950 dark:text-sky-50">Limit: </span>
              <span className="capitalize">{user.managerLimitPln}</span>
            </div>
          </div>
        </CardContent>
        <CardFooter className="flex flex-col items-center space-y-2">
          <ResetPasswordDialog/>
          <LogoutButton/>
          {user?.role === 'admin' && <CompanySettingsForm token={token} />}
        </CardFooter>
      </Card>
    );
  } catch (error) {
    console.error('Error fetching user data:', error);
    return (
      <p className="text-red-500">Nie udało się pobrać danych użytkownika.</p>
    );
  }
}