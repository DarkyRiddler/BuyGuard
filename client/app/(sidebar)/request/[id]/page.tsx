'use client';

import { useParams, useRouter } from 'next/navigation';
import axios from '@/lib/utils';
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { toast } from 'sonner';
import { useEffect, useState } from 'react';


export default function InputForm() {
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;

  const [request, setRequest] = useState<any | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchRequest() {
      try {
        const res = await axios.get(`/api/Requests/${id}`);
        setRequest(res.data);
      } catch (error) {
        console.error('Error fetching request:', error);
        toast.error('Nie udało się pobrać danych zgłoszenia');
      } finally {
        setLoading(false);
      }
    }

    fetchRequest();
  }, [id]);

  if (loading) return <p>Ładowanie...</p>;
  if (!request) return <p>Brak danych.</p>;

  return (
    <Card className="min-w-200">
      <CardHeader>
        <CardTitle className="mx-auto text-2xl">
          <span className="text-4xl font-semibold text-slate-950 dark:text-sky-50">
            {request.title}
          </span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-2 w-full text-2xl mb-5">
          <div className="flex justify-between">
            <span className="dark:bg-gray-800 bg-yellow-200 font-semibold text-slate-950 dark:text-sky-50">
              Cena (PLN):
            </span>
            <span>{request.amountPln}</span>
          </div>
            <div className="flex justify-between">
            <span className="dark:bg-gray-800 bg-yellow-200 font-semibold text-slate-950 dark:text-sky-50">
              Opis:
            </span>
            <span>{request.description}</span>
          </div>
            <div className="flex justify-between">
            <span className="dark:bg-gray-800 bg-yellow-200 font-semibold text-slate-950 dark:text-sky-50">
              Powód:
            </span>
            <span>{request.reason}</span>
          </div>
                    <div className="flex justify-between">
            <span className="dark:bg-gray-800 bg-yellow-200 font-semibold text-slate-950 dark:text-sky-50">
              Link:
            </span>
            <span><a target="_blank" href={"https://" + request.url} >{request.url}</a></span>
          </div>
          <div className="flex justify-between">
            <span className="dark:bg-gray-800 bg-yellow-200 font-semibold text-slate-950 dark:text-sky-50">
              Status:
            </span>
            <span>{request.status}</span>
          </div>
        </div>
      </CardContent>
      <CardFooter className="flex flex-col items-center space-y-2">
        <button
          onClick={() => router.push('/request/list')}
          className="text-2xl bg-gray-300 dark:bg-gray-700 px-4 py-2 rounded hover:bg-gray-400 dark:hover:bg-gray-800 hover:cursor-pointer"
        >
          Powrót do listy
        </button>
        <button
          onClick={() => router.push(`/request/edit/${id}`)}
          className="text-2xl bg-yellow-600 text-white px-4 py-2 rounded hover:bg-yellow-700 hover:cursor-pointer"
        >
          Edytuj zgłoszenie
        </button>
      </CardFooter>
    </Card>
  );
}