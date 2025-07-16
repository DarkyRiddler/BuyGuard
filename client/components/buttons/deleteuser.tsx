'use client';

import { Button } from '@/components/ui/button';
import axios from '@/lib/utils';
import {
  Dialog,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter, DialogClose,
} from '@/components/ui/dialog';
import { toast } from 'sonner';
import { isAxiosError } from 'axios';
import { useRouter } from 'next/navigation';

type DeleteButtonProps = {
  userId: number;
};

export default function DeleteButton({ userId }: DeleteButtonProps) {
  const router = useRouter();
  
  async function handleDelete() {
    try {
      await axios.delete(`api/Users/${userId}`,);
      toast.success('Usunięto użytkownika pomyślnie');
      setTimeout(() => router.refresh(), 1000);
    } catch (error) {
      if (isAxiosError(error)) {
        if (isAxiosError(error)) {
          console.log(error);
          toast.error(error.response?.data?.message ?? 'Wystąpił nieznany błąd');
        } else {
          toast.error('Wystąpił błąd połączenia');
        }
      }
    }
  }

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button className="w-full bg-red-600 hover:bg-red-700">
          Usuń
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Potwierdzenie usunięcia</DialogTitle>
          <DialogDescription>
            Czy na pewno chcesz usunąć tego użytkownika? Tej operacji nie można cofnąć.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter className="flex justify-end gap-2 pt-4">
          <DialogClose>
            Anuluj
          </DialogClose>
          <Button className="bg-red-600 hover:bg-red-700" onClick={handleDelete}>
            Usuń
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}