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
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import axios from '@/lib/utils';
import { useState } from 'react';

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
  
  return (
    <Dialog>
      <form className={'w-full'}>
        <DialogTrigger asChild>
          <Button className={'w-full'}>Zmień hasło</Button>
        </DialogTrigger>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Zmień hasło</DialogTitle>
            <DialogDescription>
              Wprowadź swoje nowe hasło, aby zaktualizować swoje konto.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4">
            <div className="grid gap-3">
              <Label htmlFor="name-1">Obecne hasło:</Label>
              <Input id="name-1" name="name" defaultValue="Pedro Duarte"/>
            </div>
            <div className="grid gap-3">
              <Label htmlFor="username-1">Nowe hasło:</Label>
              <Input id="username-1" name="username" defaultValue="@peduarte"/>
            </div>
            <div className="grid gap-3">
              <Label htmlFor="username-1">Potwierdź hasło</Label>
              <Input id="username-1" name="username" defaultValue="@peduarte"/>
            </div>
          </div>
          <DialogFooter>
            <DialogClose asChild>
              <Button variant="outline">Anuluj</Button>
            </DialogClose>
            <Button type="submit">Zmień</Button>
          </DialogFooter>
        </DialogContent>
      </form>
    </Dialog>
  );
}
