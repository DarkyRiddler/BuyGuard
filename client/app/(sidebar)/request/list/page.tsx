import axios from '@/lib/utils';
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

interface PaginatedResponse {
  request: Request[];
  totalPages: number;
  currentPage: number;
  totalRequests: number;
  filters?: {
    status?: string;
    minAmount?: number;
    maxAmount?: number;
    dateFrom?: string;
    dateTo?: string;
    searchName?: string;
    sortBy?: string;
    sortOrder?: string;
  };
}

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
  } = await searchParams;

  const page = Number(pageString) || 1;
  const pageSize = Number(pageSizeString) || 10;

  const cookieStore = await cookies();
  const cookie = cookieStore.get('jwt');
  
  try {
    const { data } = await axios.get<PaginatedResponse>('/api/Requests', {
      headers: {
        Authorization: `Bearer ${cookie?.value}`,
      },
      params: {
        page,
        pageSize,
      },
    });
    console.log('Requests:', data);

    return (
      <div>
        <Table>
          <TableCaption>A list of your recent invoices.</TableCaption>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[100px]">Invoice</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Method</TableHead>
              <TableHead className="text-right">Amount</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow>
              <TableCell className="font-medium">INV001</TableCell>
              <TableCell>Paid</TableCell>
              <TableCell>Credit Card</TableCell>
              <TableCell className="text-right">$250.00</TableCell>
            </TableRow>
          </TableBody>
        </Table>
        <RequestPagination currentPage={data.currentPage} totalPages={data.totalPages} />
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