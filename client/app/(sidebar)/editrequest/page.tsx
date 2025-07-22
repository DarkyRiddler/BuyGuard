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
import { Eye, EyeOff } from 'lucide-react';
import axios from '@/lib/utils';
import { Plus } from 'lucide-react';



const FormSchema = z.object({
  title: z.string().min(1, {
    message: 'Tytuł jest wymagany',
  }),
  price: z.preprocess((val) => Number(val), z.number()
    .max(100000, { message: 'Kwota musi być mniejsza lub równa 100 000 zł' })
    .min(1, { message: 'Kwota musi być większa od zera' })
  ),
  description: z.string().min(1,{
    message: 'Opis jest wymagany',
  }),
  image: z.any().optional(),
  link: z.string().url({ message: 'Podaj poprawny adres URL' }),
  });



export default function InputForm() {
  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      title: '',
      price: 0,
      description: '',
      link: '',
    },
  });

  const USER_ID = 1;

   useEffect(() => {
    async function fetchUser() {
      try {
        const res = await axios.get('/api/Users/${USER_ID}');
        form.reset({
          title: res.data.title || '',
          price: res.data.price || '',
          description: res.data.description || '',
          //link: res.link || '',
        });
      } catch (error) {
        console.error('Błąd pobierania zgłoszenia:', error);
        toast.error('Nie udało się załadować zgłoszenia');
      }
    }

    fetchUser();
  }, [form]);

  async function onSubmit(data: z.infer<typeof FormSchema>) {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const {  ...sanitizedData } = data;
    toast('You submitted the following values', {
      description: (
        <pre className="mt-2 w-[320px] rounded-md bg-neutral-950 p-4">
          <code className="text-white">{JSON.stringify(data, null, 2)}</code>
        </pre>
      ),
    });
    
    try {
      const res = await axios.put('api/Users/${USER_ID}', sanitizedData);
      console.log('Response from server:', res);
    } catch (error) {
      console.error(error);
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
            name="price"
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
                  <Input {...field} />
                </FormControl>
                <FormMessage/>
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="link"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Link:</FormLabel>
                <FormControl>
                  <Input type="url" {...field} />
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

                    {field.value && (
                        <img
                        src={URL.createObjectURL(field.value)}
                        alt="Podgląd"
                        className="w-24 h-24 object-cover rounded-xl border"
                        />
                    )}
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
