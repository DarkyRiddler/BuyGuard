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



const FormSchema = z.object({
  firstname: z.string().min(1, {
    message: 'Imię jest wymagane',
  }),
  lastname: z.string().min(1, {
    message: 'Nazwisko jest wymagane',
  }),
  email: z.string().email({
    message: 'Nieprawidłowy adres e-mail',
  }),
  });



export default function InputForm() {
  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      firstname: '',
      lastname: '',
      email: '',
    },
  });

  const USER_ID = 1;

   useEffect(() => {
    async function fetchUser() {
      try {
        const res = await axios.get('/api/Users/${USER_ID}');
        form.reset({
          firstname: res.data.firstname || '',
          lastname: res.data.lastname || '',
          email: res.data.email || '',
        });
      } catch (error) {
        console.error('Błąd pobierania użytkownika:', error);
        toast.error('Nie udało się załadować danych użytkownika');
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
        <form onSubmit={form.handleSubmit(onSubmit)} className="w-1/3 space-y-6">
          <FormField
            control={form.control}
            name="firstname"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Imię</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage/>
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="lastname"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Nazwisko</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>

                <FormMessage/>
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Adres e-mail</FormLabel>
                <FormControl>
                  <Input {...field} />
                </FormControl>
                <FormMessage/>
              </FormItem>
            )}
          />
          <Button type="submit" className={'w-full'}>Edytuj</Button>
        </form>
      </Form>
  );
}
