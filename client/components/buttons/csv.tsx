'use client';

import axios from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { isAxiosError } from 'axios';
import { useRouter } from 'next/navigation';

export default function CsvButton() {
  const router = useRouter();

  async function csv() {
    try {
      const res = await axios.get('api/export/export');
      console.log(res);
      toast.success('Pobrano csv');
      setTimeout(() => router.push('/request/list'), 1000);
    } catch (error) {
      if (isAxiosError(error)) {
        if (isAxiosError(error)) {
          toast.error(error.response?.data?.message ?? 'Wystąpił nieznany błąd');
        } else {
          toast.error('Wystąpił błąd połączenia');
        }
      }
    }
  }

  return <Button onClick={csv} className="w-full">Pobierz csv</Button>;
}