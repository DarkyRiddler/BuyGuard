import axios, { cn } from '@/lib/utils';
import { isAxiosError } from 'axios';
import { cookies } from 'next/headers';
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { RequestPagination } from '@/components/request/request-pagination';
import { PageSizeSelect } from '@/components/request/page-size-select';
import { Label } from '@/components/ui/label';
import { getUserFromCookie } from '@/lib/auth';
import Link from 'next/link';
import { Badge } from '@/components/ui/badge';
import FiltrationCard from '@/components/request/filtration-card';
import CsvButton from '@/components/buttons/csv';

const badgeClassName: Record<RequestStatus, string> = {
  czeka: 'bg-yellow-100 text-yellow-800',
  potwierdzono: 'bg-green-100 text-green-800',
  odrzucono: 'bg-red-100 text-red-800',
  zakupione: 'bg-blue-100 text-blue-800',
};

type Props = {
  searchParams: {
    page?: string;
    pageSize?: string;
  };
};

export default async function RequestListPage({ searchParams }: Props) {
  const {
    page: pageString,
    pageSize: pageSizeString,
    ...filterParams
  } = await searchParams;

  const page = Number(pageString) || 1;
  const pageSize = Number(pageSizeString) || 10;

  const cookieStore = await cookies();
  const cookie = cookieStore.get('jwt');

  try {
    const user = await getUserFromCookie();

    const { data } = await axios.get<PaginatedResponse>('/api/Requests', {
      headers: {
        Authorization: `Bearer ${cookie?.value}`,
      },
      params: {
        page,
        pageSize,
        ...filterParams
      },
    });

    return (
      <div className={'flex flex-col gap-4 w-full'}>
        <FiltrationCard/>
        <div className={'flex gap-4 justify-end'}>
          <CsvButton className="h-full"/>
          <div className={'flex flex-col gap-2'}>
            <Label className={'text-sm'}>Liczba zgłoszeń na stronę:</Label>
            <PageSizeSelect defaultValue={pageSize}/>
          </div>
        </div>
        <Table>
          <TableCaption>A list of your recent invoices.</TableCaption>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[100px]">Kod</TableHead>
              <TableHead>Nazwa</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Kwota</TableHead>
              <TableHead className="text-right">Data</TableHead>
              {user?.role !== 'employee' && (<TableHead className="text-center">Pracownik</TableHead>)}
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.request.map((request: Request) => (
              <TableRow key={request.id} className="hover:bg-gray-200 dark:hover:bg-gray-800 dark:hover:text-white">
                <TableCell className="font-medium">
                  <Link href={`/request/${request.id}`} className="block w-full h-full">
                    {request.id}
                  </Link>
                </TableCell>
                <TableCell>
                  <Link href={`/request/${request.id}`} className="block w-full h-full">
                    {request.description}
                  </Link>
                </TableCell>
                <TableCell>
                  <Badge asChild className={cn('block w-full h-full', badgeClassName[request.status as RequestStatus])}>
                    <Link href={`/request/${request.id}`} className="text-center">
                      {request.status}
                    </Link>
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <Link href={`/request/${request.id}`} className="block w-full h-full">
                    {request.amountPln} zł
                  </Link>
                </TableCell>
                <TableCell className="text-right">
                  <Link href={`/request/${request.id}`} className="block w-full h-full">
                    {new Date(request.createdAt).toLocaleDateString()}
                  </Link>
                </TableCell>
                {user?.role !== 'employee' && (
                  <TableCell className="text-center">
                    <Link href={`/request/${request.id}`} className="block w-full h-full">
                      {request.userName}
                    </Link>
                  </TableCell>
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <RequestPagination currentPage={data.currentPage} totalPages={data.totalPages}/>
      </div>
    );

  } catch (error) {
    if (isAxiosError(error)) {
      console.error('Error fetching requests:', error.response?.data || error.message);
    } else {
      console.error('Unexpected error:', error);
    }
  }
}