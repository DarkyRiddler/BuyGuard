'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useRef, useState } from 'react';
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
import { Textarea } from '@/components/ui/textarea';
import { isAxiosError } from 'axios';
import { AttachmentsDropzone } from '@/components/request/attachments-dropzone';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useUser } from '@/context/user-context';



const FormSchema = z.object({
  title: z.string().min(1, {
    message: 'Tytuł jest wymagany',
  }),
  amount_pln: z.preprocess((val) => parseFloat(z.string().parse(val)),
    z.number().max(100000, { message: 'Kwota musi być mniejsza lub równa 100 000 zł' })
    .min(1, { message: 'Kwota musi być większa od zera' }),
  ),
  description: z.string().min(1, {
    message: 'Opis jest wymagany',
  }),
  reason: z.string().min(1, {
    message: 'Uzasadnienie jest wymagane',
  }),
  url: z.string().regex(/[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)/gi, { message: 'Podaj poprawny adres URL' }),
});


export default function InputForm() {
  const user = useUser();

  const imageRef = useRef<File | null>(null);
  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      title: '',
      amount_pln: 0,
      description: '',
      reason: '',
      url: '',
    },
  });

  const attachmentsRef = useRef<File[]>([]);
  const [attachmentKey, setAttachmentKey] = useState(0);
  if (user?.role === 'admin' || user?.role === 'manager') {
        return (
            <Card>
                <CardContent className="py-8">
                    <Alert className="border-red-200 bg-red-50">
                        <AlertDescription className="text-red-800">
                            Nie masz uprawnień do tworzenia zgłoszeń.
                        </AlertDescription>
                    </Alert>
                </CardContent>
            </Card>
        );
    }

  async function onSubmit(data: z.infer<typeof FormSchema>) {
    try {
      const response = await axios.post('api/Requests', data);
      const requestId = response.data.requestId;

      if (attachmentsRef.current.length > 0) {
        for (const file of attachmentsRef.current) {
          const formData = new FormData();
          formData.append('file', file);

          await axios.post(
            `/api/Attachments/requests/${requestId}/attachment`,
            formData,
            {
              headers: {
                'Content-Type': 'multipart/form-data',
              },
            },
          );
        }
      }

      toast.success('Zgłoszenie zostało pomyślnie utworzone');
      form.reset();
      attachmentsRef.current = [];
      // Reset attachment component by changing key
      setAttachmentKey(prev => prev + 1);
    } catch (error) {
      if (isAxiosError(error)) {
        toast.error(error.response?.data ?? 'Wystąpił nieznany błąd');
      } else {
        toast.error('Wystąpił błąd połączenia');
      }
    }
  }


  return (
    <Form {...form}>
      <h1 className="text-2xl font-semibold mb-6">Dodaj zgłoszenie:</h1>
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
          name="amount_pln"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Cena:</FormLabel>
              <FormControl>
                <Input type="number" {...field} />
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
                < Textarea {...field} />
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
                < Textarea {...field} />
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
            key={attachmentKey}
            onFilesChange={(files) => {
              attachmentsRef.current = files;
            }}
          />
        </div>
        <Button type="submit" className={'w-full'}>Dodaj zgłoszenie</Button>
      </form>
    </Form>

  );
}
