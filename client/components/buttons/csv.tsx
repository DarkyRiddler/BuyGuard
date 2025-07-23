'use client';

import axios from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { isAxiosError } from 'axios';

export default function CsvButton() {
  async function csv() {
    try {
      const res = await axios.get('api/export/export', {
        responseType: 'blob', // bardzo ważne!
      });

      // Tworzymy tymczasowy link do pobrania
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'requests_report.zip');
      document.body.appendChild(link);
      link.click();

      toast.success('Pobrano raport ZIP');
    } catch (error) {
      if (isAxiosError(error)) {
        toast.error(error.response?.data?.message ?? 'Błąd pobierania pliku');
      } else {
        toast.error('Nieznany błąd');
      }
    }
  }

  return <Button onClick={csv} className="w-full">📥 Pobierz raport ZIP</Button>;
}