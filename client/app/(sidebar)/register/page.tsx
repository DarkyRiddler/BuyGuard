'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useState } from 'react';
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
}).superRefine((data, ctx) => {
  if (data.password !== data.confirmPassword) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['confirmPassword'],
      message: 'Hasła nie pasują do siebie',
    });
  }
});

export default function InputForm() {
  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      firstname: '',
      lastname: '',
      email: '',
      password: '',
      confirmPassword: '',
    },
  });

  async function onSubmit(data: z.infer<typeof FormSchema>) {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const { confirmPassword, ...sanitizedData } = data;
    toast('You submitted the following values', {
      description: (
        <pre className="mt-2 w-[320px] rounded-md bg-neutral-950 p-4">
          <code className="text-white">{JSON.stringify(data, null, 2)}</code>
        </pre>
      ),
    });
    
    try {
      const res = await axios.post('api/Users', sanitizedData);
      console.log('Response from server:', res);
    } catch (error) {
      console.error(error);
    }
  }

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

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
          <Button type="submit" className={'w-full'}>Zarejestruj</Button>
        </form>
      </Form>
  );
}
