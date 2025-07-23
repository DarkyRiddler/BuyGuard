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

const badgeClassName: Record<RequestStatus, string> = {
    czeka: 'bg-yellow-100 text-yellow-800',
    potwierdzono: 'bg-green-100 text-green-800',
    odrzucono: 'bg-red-100 text-red-800',
    zakupione: 'bg-blue-100 text-blue-800',
};

// Funkcja do określenia koloru AI Score
const getAIScoreColor = (score: number | null): string => {
    if (score === null) return 'text-gray-400';
    if (score >= 8) return 'text-green-600 font-semibold';
    if (score >= 6) return 'text-yellow-600 font-medium';
    if (score >= 4) return 'text-orange-600';
    return 'text-red-600 font-medium';
};

// Funkcja do określenia badge dla AI Score
const getAIScoreBadge = (score: number | null): string => {
    if (score === null) return 'bg-gray-100 text-gray-500';
    if (score >= 8) return 'bg-green-100 text-green-700';
    if (score >= 6) return 'bg-yellow-100 text-yellow-700';
    if (score >= 4) return 'bg-orange-100 text-orange-700';
    return 'bg-red-100 text-red-700';
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
            },
        });

        console.log(data);

        return (
            <div className={'flex flex-col gap-4 w-full'}>
                <div className={'flex justify-between items-center'}>
                    <h1 className="text-2xl font-bold">Lista Requestów (DEV)</h1>
                    <div className={'flex flex-col gap-2'}>
                        <Label className={'text-sm'}>Liczba zgłoszeń na stronę:</Label>
                        <PageSizeSelect defaultValue={pageSize}/>
                    </div>
                </div>
                <Table>
                    <TableCaption>Lista requestów z AI Score (wersja deweloperska)</TableCaption>
                    <TableHeader>
                        <TableRow>
                            <TableHead className="w-[100px]">Kod</TableHead>
                            <TableHead>Nazwa</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-center">AI Score</TableHead>
                            <TableHead className="text-right">Kwota</TableHead>
                            <TableHead className="text-right">Data</TableHead>
                            {user?.role !== 'employee' && (<TableHead className="text-center">Pracownik</TableHead>)}
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {data.request.map((request: Request) => (
                            <TableRow key={request.id} className="hover:bg-gray-200">
                                <TableCell className="font-medium">
                                    <Link href={`/request/${request.id}`} className="block w-full h-full">
                                        {request.id}
                                    </Link>
                                </TableCell>
                                <TableCell>
                                    <Link href={`/request/${request.id}`} className="block w-full h-full">
                                        {request.title || request.description}
                                    </Link>
                                </TableCell>
                                <TableCell>
                                    <Badge asChild className={cn('block w-full h-full', badgeClassName[request.status as RequestStatus])}>
                                        <Link href={`/request/${request.id}`} className="text-center">
                                            {request.status}
                                        </Link>
                                    </Badge>
                                </TableCell>
                                <TableCell className="text-center">
                                    <Link href={`/request/${request.id}`} className="block w-full h-full">
                                        {request.aiScore !== null ? (
                                            <div className="flex flex-col items-center gap-1">
                                                <Badge className={cn('text-xs px-2 py-1', getAIScoreBadge(request.aiScore))}>
                                                    {request.aiScore?.toFixed(1)}/10
                                                </Badge>
                                                {request.aiScoreGeneratedAt && (
                                                    <span className="text-xs text-gray-500">
                            {new Date(request.aiScoreGeneratedAt).toLocaleDateString('pl-PL')}
                          </span>
                                                )}
                                            </div>
                                        ) : (
                                            <Badge className="bg-gray-100 text-gray-500 text-xs px-2 py-1">
                                                Brak AI
                                            </Badge>
                                        )}
                                    </Link>
                                </TableCell>
                                <TableCell className="text-right">
                                    <Link href={`/request/${request.id}`} className="block w-full h-full">
                                        {request.amountPln} zł
                                    </Link>
                                </TableCell>
                                <TableCell className="text-right">
                                    <Link href={`/request/${request.id}`} className="block w-full h-full">
                                        {new Date(request.createdAt).toLocaleDateString('pl-PL')}
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

                {/* Legenda AI Score */}
                <div className="mt-4 p-4 bg-gray-50 rounded-lg">
                    <h3 className="text-sm font-semibold mb-2">Legenda AI Score:</h3>
                    <div className="flex flex-wrap gap-2 text-xs">
                        <Badge className="bg-green-100 text-green-700">8.0-10.0 - Bardzo przydatne</Badge>
                        <Badge className="bg-yellow-100 text-yellow-700">6.0-7.9 - Przydatne</Badge>
                        <Badge className="bg-orange-100 text-orange-700">4.0-5.9 - Średnio przydatne</Badge>
                        <Badge className="bg-red-100 text-red-700">0.0-3.9 - Mało przydatne</Badge>
                        <Badge className="bg-gray-100 text-gray-500">Brak AI - Nie obliczono</Badge>
                    </div>
                </div>
            </div>
        );

    } catch (error) {
        if (isAxiosError(error)) {
            console.error('Error fetching requests:', error.response?.data || error.message);
        } else {
            console.error('Unexpected error:', error);
        }

        return (
            <div className="flex flex-col gap-4 w-full">
                <h1 className="text-2xl font-bold text-red-600">Błąd ładowania requestów</h1>
                <p className="text-gray-600">Nie udało się załadować listy requestów. Spróbuj ponownie.</p>
            </div>
        );
    }
}