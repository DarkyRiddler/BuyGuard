'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useEffect, useState } from 'react';
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
import { Plus } from 'lucide-react';
import { useParams } from 'next/navigation';
import { Textarea } from '@/components/ui/textarea';

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
          .min(0.01, { message: 'Kwota musi być większa od zera' })
      ),
    z.number()
      .max(100000, { message: 'Kwota musi być mniejsza lub równa 100 000 zł' })
      .min(0.01, { message: 'Kwota musi być większa od zera' })
  ]),
  description: z.string().min(1, {
    message: 'Opis jest wymagany',
  }),
  reason: z.string().min(1, {
    message: 'Uzasadnienie jest wymagane',
  }),
  url: z.string().regex(/[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)/gi, { 
    message: 'Podaj poprawny adres URL' 
  }),
  image: z.any().optional(),
});

type FormValues = z.infer<typeof FormSchema>;

export default function InputForm() {
  const params = useParams();
  const id = params.id as string;
  const [existingImageUrl, setExistingImageUrl] = useState<string | null>(null);
  
  const form = useForm<FormValues>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      title: '',
      amountPln: 0,
      description: '',
      reason: '',
      url: '',
      image: undefined,
    },
  });

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
          image: undefined,
        });

        const attachmentsRes = await axios.get(`/api/Attachments/requests/${id}/attachment`);
        const firstAttachment = attachmentsRes.data?.[0];

        if (firstAttachment?.fileUrl && firstAttachment.mimeType.startsWith('image/')) {
          setExistingImageUrl(firstAttachment.fileUrl);
        }
      } catch(error) {
        console.error('Error fetching request:', error);
        toast.error('Nie udało się pobrać danych zgłoszenia');
      }
    }
    fetchRequest(id);
  }, [id, form]);

  const onSubmit = async (data: FormValues) => {
    try {
      console.log(data, data.amountPln);
      const payload = {
        ...data,
      };
      console.log(payload)
      
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
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="image"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Zdjęcie:</FormLabel>
              <FormControl>
                <div className="flex items-center gap-4">
                  <label
                    htmlFor="upload"
                    className="cursor-pointer w-24 h-24 border-2 border-dashed border-gray-300 rounded-xl flex items-center justify-center hover:bg-gray-100 transition"
                  >
                    <Plus className="w-6 h-6 text-gray-500" />
                    <input
                      id="upload"
                      type="file"
                      accept="image/*"
                      className="hidden"
                      onChange={(e) => {
                        const file = e.target.files?.[0];
                        if (file) field.onChange(file);
                      }}
                    />
                  </label>

                  {field.value ? (
                    <img
                      src={URL.createObjectURL(field.value)}
                      alt="Podgląd"
                      className="w-24 h-24 object-cover rounded-xl border"
                    />
                  ) : existingImageUrl ? (
                    <img
                      src={existingImageUrl}
                      alt="Istniejący załącznik"
                      className="w-24 h-24 object-cover rounded-xl border"
                    />
                  ) : null}
                </div>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" className={'w-full'}>Zapisz zmiany</Button>
      </form>
    </Form>
  );
}