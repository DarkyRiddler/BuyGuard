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
import { useParams } from 'next/navigation';
import { User } from '@/types';
import { isAxiosError } from 'axios';
import Link from 'next/link';
import { Eye, EyeOff } from 'lucide-react';
import { useUser } from '@/context/user-context';


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
  password: z.string().min(6, {
    message: 'Hasło musi mieć co najmniej 6 znaków',
  })
  .refine(val => /[A-Z]/.test(val), {
    message: 'Hasło musi mieć przynajmniej jedną dużą literę',
  })
  .refine(val => /[a-z]/.test(val), {
    message: 'Hasło musi mieć przynajmniej jedną małą literę',
  })
  .refine(val => /[0-9]/.test(val), {
    message: 'Hasło musi zawierać liczby',
  })
  .refine(val => /[^a-zA-Z0-9]/.test(val), {
    message: 'Hasło musi mieć znaki specjalne',
  }),
  confirmPassword: z.string(),
  managerLimitPln: z
  .number()
  .min(0, { message: 'Limit musi być liczbą większą lub równą 0' })
  .optional(),
}).superRefine((data, ctx) => {
  if (data.password !== data.confirmPassword) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['confirmPassword'],
      message: 'Hasła nie pasują do siebie',
    });
  }
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
  const user = useUser()
  
  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      firstname: '',
      lastname: '',
      email: '',
      password: '',
      confirmPassword: '',
      managerLimitPln: user?.role === 'admin' ? 0 : undefined,
    },
  });

  useEffect(() => {
    fetchUser(id)
    .then((user: User) => {
      form.reset({
        firstname: user.firstName || '',
        lastname: user.lastName || '',
        email: user.email || '',
        managerLimitPln: user.managerLimitPln || undefined,
      });
    })
    .catch((error) => {
      console.error('Error fetching user:', error);
      toast.error('Nie udało się pobrać danych użytkownika');
    });
  }, [form, id]);
  
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

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
        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Hasło</FormLabel>
              <FormControl>
                <div className={'relative'}>
                  <Input {...field} type={showPassword ? 'text' : 'password'}/>
                  <button
                    type="button"
                    onClick={() => setShowPassword((prev) => !prev)}
                    className="absolute right-2 top-1/2 -translate-y-1/2 text-sm text-gray-500"
                  >
                    {showPassword ? <EyeOff className="w-5 h-5"/> : <Eye className="w-5 h-5"/>}
                  </button>
                </div>
              </FormControl>
              <FormMessage/>
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="confirmPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Potwierdź hasło</FormLabel>
              <FormControl>
                <div className={'relative'}>
                  <Input {...field} type={showConfirmPassword ? 'text' : 'password'}/>
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword((prev) => !prev)}
                    className="absolute right-2 top-1/2 -translate-y-1/2 text-sm text-gray-500"
                  >
                    {showConfirmPassword ? <EyeOff className="w-5 h-5"/> : <Eye className="w-5 h-5"/>}
                  </button>
                </div>
              </FormControl>
              <FormMessage/>
            </FormItem>
          )}
        />
        {user?.role === 'admin' && (
          <FormField
            control={form.control}
            name="managerLimitPln"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Limit</FormLabel>
                <FormControl>
                  <Input type={'number'} {...field} onChange={(e) =>
                    field.onChange(e.target.value === '' ? undefined : Number(e.target.value))
                  }/>
                </FormControl>
                <FormMessage/>
              </FormItem>
            )}
          />
        )}
        <Button type="submit" className={'w-full'}>Edytuj</Button>
        <Button type="button" className={'w-full'} asChild>
          <Link href={'/user/list'}>Powrót do listy</Link>
        </Button>
      </form>
    </Form>
  );
}
