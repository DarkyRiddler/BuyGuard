'use client';

import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { z } from 'zod';
import { Form, FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import axios from '@/lib/utils';
import { useState } from 'react';
import { FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';

const FormSchema = z.object({
  oldPassword: z.string().min(6, {
    message: 'Hasło musi mieć co najmniej 6 znaków',
  }),
  newPassword: z.string().min(6, {
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
  if (data.newPassword !== data.confirmPassword) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['confirmPassword'],
      message: 'Hasła nie pasują do siebie',
    });
  }
});

export default function ResetPasswordDialog() {
  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      oldPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  });

  async function onSubmit(data: z.infer<typeof FormSchema>) {
    toast('You submitted the following values', {
      description: (
        <pre className="mt-2 w-[320px] rounded-md bg-neutral-950 p-4">
          <code className="text-white">{JSON.stringify(data, null, 2)}</code>
        </pre>
      ),
    });

    try {
      const res = await axios.patch('auth/change-password', data);
      console.log('Response from server:', res);
    } catch (error) {
      console.error(error);
    }
  }

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button type="button" className="w-full">Zmień hasło</Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[425px]">
        <FormProvider {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="w-full">
            <DialogHeader>
              <DialogTitle>Zmień hasło</DialogTitle>
              <DialogDescription>
                Wprowadź swoje nowe hasło, aby zaktualizować swoje konto.
              </DialogDescription>
            </DialogHeader>
            <FormField
              control={form.control}
              name="oldPassword"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Stare hasło</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage/>
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="newPassword"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Nowe hasło</FormLabel>
                  <FormControl>
                    <Input {...field} />
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
                    <Input {...field} />
                  </FormControl>
                  <FormMessage/>
                </FormItem>
              )}
            />
            <DialogFooter>
              <DialogClose asChild>
                <Button variant="outline" type="button">Anuluj</Button>
              </DialogClose>
              <Button type="submit">Zmień</Button>
            </DialogFooter>
          </form>
        </FormProvider>
      </DialogContent>
    </Dialog>
  );
}
