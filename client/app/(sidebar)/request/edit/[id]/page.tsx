'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useEffect, useRef, useState } from 'react';
import { toast } from 'sonner';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import axios from '@/lib/utils';
import { useParams } from 'next/navigation';
import { Textarea } from '@/components/ui/textarea';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { useUser } from '@/context/user-context';
import { AttachmentsDropzone } from '@/components/request/attachments-dropzone';
import { Attachment } from '@/types';

const baseUrl = process.env.NEXT_PUBLIC_BASE_URL || 'https://localhost:7205';

const FormSchema = z.object({
  title: z.string().min(1, {
    message: 'Tytuł jest wymagany',
  }),
  amountPln: z.union([
    z.string()
    .min(1, { message: 'Kwota jest wymagana' })
    .transform((val) => parseFloat(val.replace(',', '.')))
    .pipe(
      z.number()
      .max(100000, { message: 'Kwota musi być mniejsza lub równa 100 000 zł' })
      .min(0.01, { message: 'Kwota musi być większa od zera' }),
    ),
    z.number()
    .max(100000, { message: 'Kwota musi być mniejsza lub równa 100 000 zł' })
    .min(0.01, { message: 'Kwota musi być większa od zera' }),
  ]),
  description: z.string().min(1, {
    message: 'Opis jest wymagany',
  }),
  reason: z.string().min(1, {
    message: 'Uzasadnienie jest wymagane',
  }),
  url: z.string().regex(/[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)/gi, {
    message: 'Podaj poprawny adres URL',
  }),
});

type FormValues = z.infer<typeof FormSchema>;

async function deleteAttachment(id: string, updateList: () => void) {
  try {
    const res = await axios.delete(`/api/Attachments/${id}`);
    console.log('Attachment deleted:', res);
    updateList();
  } catch (error) {
    console.error('Error deleting attachment:', error);
    toast.error('Nie udało się usunąć załącznika');
  }
}

export default function InputForm() {
  const user = useUser();
  const params = useParams();
  const id = params.id as string;

  const form = useForm<FormValues>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      title: '',
      amountPln: 0,
      description: '',
      reason: '',
      url: '',
    },
  });

  const attachmentsRef = useRef<File[]>([]);
  const [currentAttachments, setCurrentAttachments] = useState<Attachment[]>([]);

  useEffect(() => {
    async function fetchRequest(id: string) {
      try {
        const res = await axios.get(`/api/Requests/${id}`);
        form.reset({
          title: res.data.title || '',
          amountPln: res.data.amountPln || 0,
          description: res.data.description || '',
          reason: res.data.reason || '',
          url: res.data.url || '',
        });

        const { data } = await axios.get<Attachment[]>(`/api/Attachments/requests/${id}/attachment`);
        setCurrentAttachments(data);
        console.log('Fetched attachments:', data);

      } catch (error) {
        console.error('Error fetching request:', error);
        toast.error('Nie udało się pobrać danych zgłoszenia');
      }
    }

    fetchRequest(id);
  }, [id, form]);

  if (user?.role === 'admin' || user?.role === 'manager') {
    return (
      <Card>
        <CardContent className="py-8">
          <Alert className="border-red-200 bg-red-50">
            <AlertDescription className="text-red-800">
              Nie masz uprawnień do edycji zgłoszeń.
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    );
  }
  const onSubmit = async (data: FormValues) => {
    try {
      console.log(data, data.amountPln);
      const payload = {
        ...data,
      };
      console.log(payload);

      if (attachmentsRef.current.length > 0) {
        for (const file of attachmentsRef.current) {
          const formData = new FormData();
          formData.append('file', file);

          await axios.post(
            `/api/Attachments/requests/${id}/attachment`,
            formData,
            {
              headers: {
                'Content-Type': 'multipart/form-data',
              },
            },
          );
        }
      }

      const res = await axios.put(`api/Requests/${id}`, payload);
      console.log('Response from server:', res);
      toast.success('Zgłoszenie zostało zaktualizowane');
    } catch (error) {
      console.error(error);
      toast.error('Wystąpił błąd podczas aktualizacji zgłoszenia');
    }
  };

  return (
    <Form {...form}>
      <h1 className="text-2xl font-semibold mb-6">Edytuj zgłoszenie:</h1>
      <form onSubmit={form.handleSubmit(onSubmit)} className="w-1/3 space-y-6">
        <FormField
          control={form.control}
          name="title"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Tytuł:</FormLabel>
              <FormControl>
                <Input {...field} />
              </FormControl>
              <FormMessage/>
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="amountPln"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Cena (PLN):</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  step="0.01"
                  {...field}
                  onChange={(e) => {
                    const value = e.target.value;
                    if (value === '' || !isNaN(Number(value))) {
                      field.onChange(value === '' ? '' : Number(value));
                    }
                  }}
                  value={field.value}
                />
              </FormControl>
              <FormMessage/>
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Opis:</FormLabel>
              <FormControl>
                <Textarea {...field} />
              </FormControl>
              <FormMessage/>
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="reason"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Uzasadnienie:</FormLabel>
              <FormControl>
                <Textarea {...field} />
              </FormControl>
              <FormMessage/>
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="url"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Link:</FormLabel>
              <FormControl>
                <Input type="text" {...field} />
              </FormControl>
              <FormMessage/>
            </FormItem>
          )}
        />
        <div>
          <FormLabel>Załączniki:</FormLabel>
          <AttachmentsDropzone
            title="Dodaj załączniki (max 5MB każdy)"
            onFilesChange={(files) => {
              attachmentsRef.current = files;
            }}
          />
          <Card className="mt-4">
            <CardHeader>
              <FormLabel>Pliki dołączone</FormLabel>
            </CardHeader>
            <CardContent>
              {currentAttachments.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                  {currentAttachments.map((att, index) => {
                    const isImage = att.mimeType.startsWith('image/');
                    return (
                      <div key={index} className="border rounded p-2 shadow-sm bg-white dark:bg-slate-800">
                        {isImage ? (
                          // eslint-disable-next-line @next/next/no-img-element
                          <img
                            src={`${baseUrl}${att.fileUrl}`}
                            alt={`Załącznik ${index + 1}`}
                            className="w-full h-48 object-cover rounded"
                          />
                        ) : (
                          <div
                            className="flex flex-col items-center justify-center h-48 bg-gray-100 dark:bg-gray-700 rounded">
                  <span className="text-sm text-gray-600 dark:text-gray-300">
                    📄 PDF lub inny plik
                  </span>
                          </div>
                        )}
                        <Button
                          onClick={() => deleteAttachment(att.id, () =>
                            setCurrentAttachments((prev) => prev.filter((a) => a.id !== att.id)))}
                          className="mt-2 block text-center hover:underline text-sm"
                          variant="destructive"
                        >
                          Usuń załącznik
                        </Button>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <p className="text-muted-foreground text-sm">Brak załączników.</p>
              )}
            </CardContent>
          </Card>
        </div>
        <Button type="submit" className={'w-full'}>Zapisz zmiany</Button>
      </form>
    </Form>
  );
}