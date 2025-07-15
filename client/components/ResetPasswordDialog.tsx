import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

export default function ResetPasswordDialog() {
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
              <Input id="name-1" name="name" defaultValue="Pedro Duarte" />
            </div>
            <div className="grid gap-3">
              <Label htmlFor="username-1">Nowe hasło:</Label>
              <Input id="username-1" name="username" defaultValue="@peduarte" />
            </div>
            <div className="grid gap-3">
              <Label htmlFor="username-1">Potwierdź hasło</Label>
              <Input id="username-1" name="username" defaultValue="@peduarte" />
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
  )
}
