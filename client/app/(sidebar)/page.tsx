import { cookies } from 'next/headers';
import axios from '@/lib/utils';
import { User } from '@/types';
import { DateFilterForm } from '@/components/dashboard/date-filter';
import { Label } from '@/components/ui/label';
import { Card } from '@/components/ui/card';
import { PaginatedResponse } from '@/types';

type Props = {
  searchParams: {
    dateFrom?: string;
    dateTo?: string;
  };
};

const Months: Record<number, string> = {
  0: 'Styczeń',
  1: 'Luty',
  2: 'Marzec',
  3: 'Kwiecień',
  4: 'Maj',
  5: 'Czerwiec',
  6: 'Lipiec',
  7: 'Sierpień',
  8: 'Wrzesień',
  9: 'Październik',
  10: 'Listopad',
  11: 'Grudzień',
};


export default async function Home({ searchParams }: Props) {
  const {
    dateFrom: dateFromString,
    dateTo: dateToString,
  } = await searchParams;

  const today = new Date();
  const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);

  const dateFrom = dateFromString ? new Date(dateFromString) : firstDayOfMonth;
  const dateTo = dateToString ? new Date(dateToString) : today;

  const cookieStore = await cookies();
  const token = cookieStore.get('jwt')?.value;

  try {
    const userResponse = await axios.get('api/Users/me', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    const user: User = userResponse.data.user;

    const { data: { request } } = await axios.get<PaginatedResponse>('api/Requests', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: {
        page: 1,
        pageSize: 10000, // duże, by mi wszystko zwróciło, a inny endpoint nie istnieje
        dateFrom: dateFrom, // gdy nie podane, zakres do obecnego miesiąca
        dateTo: dateTo,
      },
    });

    console.log(request);

    const stats = {
      total: request.length,
      approved: request.filter(r => r.status === 'potwierdzono').length,
      rejected: request.filter(r => r.status === 'odrzucono').length,
      pending: request.filter(r => r.status === 'czeka').length,
      purchased: request.filter(r => r.status === 'zakupione').length,
      totalAmount: request.reduce((acc, curr) => acc + curr.amountPln, 0),
    };

    console.log(stats);

    return (
      <div className="flex flex-col items-center justify-center gap-5">
        <h1 className="text-4xl font-bold">Witaj {user.firstName}</h1>
        <div className="flex flex-col gap-4">
          <Label>Wybierz zakres</Label>
          <DateFilterForm/>
        </div>
        <h1 className="text-4xl">
          {(!dateFromString && !dateToString)
            ? `Statystyki z ${Months[dateFrom.getMonth()]} ${dateFrom.getFullYear()}`
            : `Statystyki od ${dateFrom.toLocaleDateString('pl-PL')} do ${dateTo.toLocaleDateString('pl-PL')}`}
        </h1>
        <div className="flex gap-2">
          <Card className="p-5 rounded-2xl bg-white dark:bg-gray-800 shadow-md w-full max-w-md space-y-4">
            <p><strong>Łącznie zgłoszeń:</strong> {stats.total}</p>
            <p><strong>Potwierdzone:</strong> {stats.approved}</p>
            <p><strong>Odrzucone:</strong> {stats.rejected}</p>
            <p><strong>W trakcie:</strong> {stats.pending}</p>
            <p><strong>Zakupione:</strong> {stats.purchased}</p>
            <p><strong>Łączna kwota (PLN):</strong> {stats.totalAmount.toFixed(2)}</p>
          </Card>
        </div>
      </div>
    );
  } catch (e) {
    console.error('Error fetching user data:', e);
  }
}