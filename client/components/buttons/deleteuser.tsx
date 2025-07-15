'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import axios from 'axios';
import {
  Dialog,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';

const USER_ID = 1; // ← zmień na dynamiczne ID

export default function DeleteButton() {
  const [open, setOpen] = useState(false);

  async function handleDelete() {
    try {
      await axios.delete('/api/Users/${USER_ID}');
      setOpen(false);
      window.location.href = '/';
    } catch (error) {
      console.error(error);
    }
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
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
          <Button variant="outline" onClick={() => setOpen(false)}>
            Anuluj
          </Button>
          <Button className="bg-red-600 hover:bg-red-700" onClick={handleDelete}>
            Usuń
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}