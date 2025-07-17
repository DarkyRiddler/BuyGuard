'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useEffect } from 'react';
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
import { User } from '@/types';
import { isAxiosError } from 'axios';
import Link from 'next/link';


// TODO: Usunięcie Maila oraz dodanie hasła i limitu
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

async function fetchUser(id: string) {
  try {
    const res = await axios.get(`/api/Users/${id}`);
    return res.data;
  } catch (error) {
    console.error('Error fetching user:', error);
    return null;
  }
}

export default function InputForm() {
  const params = useParams();
  const id = params.id as string;
  
  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      firstname: '',
      lastname: '',
      email: '',
    },
  });

  useEffect(() => {
    fetchUser(id)
    .then((user: User) => {
      form.reset({
        firstname: user.firstName || '',
        lastname: user.lastName || '',
        email: user.email || '',
      })
    })
    .catch((error) => {
      console.error('Error fetching user:', error);
      toast.error('Nie udało się pobrać danych użytkownika');
    })
  }, [form, id]);

  async function onSubmit(data: z.infer<typeof FormSchema>) {
    try {
      await axios.patch(`api/Users/${id}`, data);
      toast.success('Użytkownik został zaktualizowany pomyślnie');
    } catch (error) {
      if (isAxiosError(error)) {
        if (isAxiosError(error)) {
          toast.error(error.response?.data ?? 'Wystąpił nieznany błąd');
        } else {
          toast.error('Wystąpił błąd połączenia');
        }
      }
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
          <Button type="button" className={'w-full'} asChild>
            <Link href={'/user/list'}>Powrót do listy</Link>
          </Button>
        </form>
      </Form>
  );
}
