// client/app/login/page.tsx
'use client';

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
import { useState } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { toast } from 'sonner';
import axios from '@/lib/utils';
import { isAxiosError } from 'axios';

const FormSchema = z.object({
  email: z.string().email({
    message: 'Nieprawidłowy adres e-mail',
  }),
  password: z.string().min(6, {
    message: 'Hasło musi mieć co najmniej 6 znaków',
  }),
});

export default function InputForm() {
  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  });

  async function onSubmit(data: z.infer<typeof FormSchema>) {
    try {
      await axios.post('auth/login', data);
      toast.success('Zalogowano pomyślnie');
      setTimeout(() => window.location.href = '/', 1000);
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

  const [showPassword, setShowPassword] = useState(false);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="w-1/3 space-y-6">
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
        <Button type="submit" className={'w-full'}>Zaloguj się</Button>
      </form>
    </Form>
  );
}