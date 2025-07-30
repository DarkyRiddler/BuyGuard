import { cookies } from 'next/headers';
import axios from '@/lib/utils';
import { User } from '@/types';
import { DateFilterForm } from '@/components/dashboard/date-filter';
import { Label } from '@/components/ui/label';
import { Card } from '@/components/ui/card';
import { PaginatedResponse } from '@/types';
import { StatusChart } from '@/components/dashboard/status-chart';
import { AmountChart } from '@/components/dashboard/amount-chart';

type Props = {
  searchParams: {
    dateFrom?: string;
    dateTo?: string;
  };
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

    

    const stats = {
      total: request.length,
      approved: request.filter(r => r.status === 'potwierdzono').length,
      rejected: request.filter(r => r.status === 'odrzucono').length,
      pending: request.filter(r => r.status === 'czeka').length,
      purchased: request.filter(r => r.status === 'zakupione').length,
      meanAmount: request.reduce((acc, curr) => acc + curr.amountPln, 0) / request.length,
      totalAmount: request.reduce((acc, curr) => acc + curr.amountPln, 0),
    };

    return (
      <div className="flex flex-col items-center justify-center gap-5">
        <h1 className="text-4xl font-bold">Witaj {user.firstName}</h1>
        <div className="flex flex-col gap-4">
          <Label>Wybierz zakres</Label>
          <DateFilterForm/>
        </div>
        <h1 className="text-4xl">
          {(!dateFromString && !dateToString)
            ? `Statystyki z ${new Date().toLocaleDateString("pl-PL", {
              month: "long",
              year: "numeric",
            })}`
            : `Statystyki od ${dateFrom.toLocaleDateString('pl-PL')} do ${dateTo.toLocaleDateString('pl-PL')}`}
        </h1>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Card className="p-5 rounded-2xl bg-white dark:bg-gray-800 shadow-md w-full max-w-md space-y-4 text-2xl">
            <p><strong>Łącznie zgłoszeń:</strong> {stats.total}</p>
            <p><strong>Potwierdzone:</strong> {stats.approved}</p>
            <p><strong>Odrzucone:</strong> {stats.rejected}</p>
            <p><strong>W trakcie:</strong> {stats.pending}</p>
            <p><strong>Zakupione:</strong> {stats.purchased}</p>
            <p><strong>Średnia wartość zamówień:</strong> {stats.meanAmount.toLocaleString("pl-PL", {minimumFractionDigits: 2, maximumFractionDigits: 2,})}{" "}PLN</p>
            <p><strong>Łączna kwota:</strong> {stats.totalAmount.toLocaleString("pl-PL", {minimumFractionDigits: 2, maximumFractionDigits: 2,})}{" "}PLN</p>
          </Card>
          <StatusChart stats={stats}/>
          <AmountChart className="col-span-1 md:col-span-2" requests={request}/>
        </div>
      </div>
    );
  } catch (e) {
    console.error('Error fetching user data:', e);
  }
}