'use client';

import { useParams, useRouter } from 'next/navigation';
import axios from '@/lib/utils';
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { toast } from 'sonner';
import { useEffect, useState } from 'react';
import { Request, RequestStatus } from '@/types';
import { Pencil, Trash2, Save, X } from 'lucide-react';
import { useUser } from '@/context/user-context';
import { isAxiosError } from 'axios';

const baseUrl = process.env.NEXT_PUBLIC_BASE_URL || 'https://localhost:7205';

interface Note {
  id: number;
  body: string;
  createdAt: string;
  isOwner: boolean;
  author: {
    id: number;
    name: string;
    email: string;
  };
}

export default function InputForm() {
  const user = useUser();
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;

  const [request, setRequest] = useState<Request | null>(null);
  const [loading, setLoading] = useState(true);

  const [status, setStatus] = useState<RequestStatus | null>(null);
  const [statusUpdating, setStatusUpdating] = useState(false);
  
  const [notes, setNotes] = useState<Note[]>([]);
  const [newNote, setNewNote] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [editingNoteId, setEditingNoteId] = useState<number | null>(null);
  const [editedNoteText, setEditedNoteText] = useState('');

  useEffect(() => {
    async function fetchRequest() {
      try {
        const res = await axios.get(`/api/Requests/${id}`);
        setRequest(res.data);
        setStatus(res.data.status);
        console.log(res.data);
      } catch (error) {
        toast.error('Nie udało się pobrać danych zgłoszenia');
      } finally {
        setLoading(false);
      }
    }

    async function fetchNotes() {
      try {
        const res = await axios.get(`/api/Notes/requests/${id}`);
        setNotes(res.data);
      } catch (error) {
        toast.error('Nie udało się pobrać notatek');
      }
    }

    fetchRequest();
    fetchNotes();
  }, [id]);

  const handleAddNote = async () => {
    if (!newNote.trim()) return;

    setSubmitting(true);
    try {
      await axios.post(`/api/Notes/requests/${id}`, { body: newNote });
      toast.success('Dodano notatkę');
      setNewNote('');
      const res = await axios.get(`/api/Notes/requests/${id}`);
      setNotes(res.data);
    } catch (err) {
      toast.error('Błąd przy dodawaniu notatki');
    } finally {
      setSubmitting(false);
    }
  };

  const handleSaveEdit = async (noteId: number) => {
    if (!editedNoteText.trim()) return;

    try {
      await axios.put(`/api/Notes/${noteId}`, { body: editedNoteText });
      toast.success('Zapisano zmiany');
      setEditingNoteId(null);
      const res = await axios.get(`/api/Notes/requests/${id}`);
      setNotes(res.data);
    } catch (err) {
      toast.error('Błąd przy zapisie notatki');
    }
  };

  const handleDeleteNote = async (noteId: number) => {
    if (!confirm('Czy na pewno chcesz usunąć tę notatkę?')) return;

    try {
      await axios.delete(`/api/Notes/${noteId}`);
      toast.success('Usunięto notatkę');
      const res = await axios.get(`/api/Notes/requests/${id}`);
      setNotes(res.data);
    } catch (err) {
      toast.error('Błąd przy usuwaniu notatki');
    }
  };

  if (loading) return <p>Ładowanie...</p>;
  if (!request) return <p>Brak danych.</p>;

  async function handleStatusChange(newVal: RequestStatus) {
    try {
      setStatusUpdating(true);
      const { data } = await axios.patch(`/api/Requests/${id}/status`, { status: newVal });
      setStatus(newVal);
      toast.success(data.message);
    } catch (error) {
      if (isAxiosError(error)) {
        console.error('Error fetching requests:', error.response?.data || error.message);
        toast.error(error.response?.data || error.message);
      } else {
        console.error('Unexpected error:', error);
      }
    } finally {
      setStatusUpdating(false);
    }
  }

  return (
    <Card className="min-w-200 max-w-275 shadow-none">
      <CardHeader>
        <CardTitle className="mx-auto text-2xl">
          <span className="font-bold text-3xl text-slate-950 dark:text-sky-50">
            {request.title} {request.aiScore == null && (user?.role==='admin' || user?.role==='manager') ? '' : "- Ai Score: "+request.aiScore}
          </span>
        </CardTitle>
      </CardHeader>

      <CardContent>
        <div className="space-y-2 w-full text-2xl mb-5">
          <div className="flex justify-between">
            <span className="font-semibold bg-slate-950/10 dark:bg-sky-50/17 text-slate-950 dark:text-sky-50">Kwota (PLN):</span>
            <span>{request.amountPln}</span>
          </div>
          <div className="flex justify-between border-t border-gray-200 dark:border-gray-200/10 my-2">
            <span className="font-semibold bg-slate-950/10 dark:bg-sky-50/17 text-slate-950 dark:text-sky-50">Opis:</span>
            <span>{request.description}</span>
          </div>
          <div className="flex justify-between border-t border-gray-200 dark:border-gray-200/10 my-2">
            <span className="font-semibold bg-slate-950/10 dark:bg-sky-50/17 text-slate-950 dark:text-sky-50">Powód:</span>
            <span>{request.reason}</span>
          </div>
          <div className="flex justify-between border-t border-gray-200 dark:border-gray-200/10 my-2">
            <span className="font-semibold bg-slate-950/10 dark:bg-sky-50/17 text-slate-950 dark:text-sky-50">Link:</span>
            <span>
              <u><a target="_blank" rel="noopener noreferrer" href={'https://' + request.url}>
                {request.url}
              </a></u>
            </span>
          </div>
          <div className="flex justify-between border-t border-gray-200 dark:border-gray-200/10 my-2">
            <span className="font-semibold bg-slate-950/10 dark:bg-sky-50/17 text-slate-950 dark:text-sky-50">Status:</span>
            {user?.role !== 'employee' && (
              <Select value={status as string} disabled={statusUpdating}
                      onValueChange={(newVal) => handleStatusChange(newVal as RequestStatus)}>
                <SelectTrigger className="w-[180px] hover:cursor-pointer">
                  <SelectValue/>
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    <SelectLabel>Statusy</SelectLabel>
                    <SelectItem className='hover:cursor-pointer' value="czeka">Oczekujący</SelectItem>
                    <SelectItem className='hover:cursor-pointer' value="potwierdzono">Zatwierdź</SelectItem>
                    <SelectItem className='hover:cursor-pointer' value="odrzucono">Odrzuć</SelectItem>
                    <SelectItem className='hover:cursor-pointer' value="zakupione">Zakupiony</SelectItem>
                  </SelectGroup>
                </SelectContent>
              </Select>)}
          </div>
          {request.attachments && request.attachments.length > 0 && (
            <div className="mt-6">
              <h3 className="text-xl font-bold mb-3">Załączniki</h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                {request.attachments.map((att, index) => {
                  const isImage = att.mimeType.startsWith('image/');
                  return (
                    <div key={index} className="border rounded p-2 shadow-sm bg-white dark:bg-slate-800">
                      {isImage ? (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img
                          src={`${baseUrl}${att.fileUrl}`}
                          alt={`Załącznik ${index + 1}`}
                          className="w-full h-48 object-cover rounded"
                        />
                      ) : (
                        <div
                          className="flex flex-col items-center justify-center h-48 bg-gray-100 dark:bg-gray-700 rounded">
                          <span className="text-sm text-gray-600 dark:text-gray-300"> PDF lub inny plik</span>
                        </div>
                      )}
                      <a
                        href={`${baseUrl}${att.fileUrl}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="mt-2 block text-center text-blue-600 hover:underline text-sm"
                      >
                        Pobierz
                      </a>
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>

        {/* Sekcja notatek */}
        <div className="mt-8">
          <h3 className="text-xl font-bold mb-3">Komentarze</h3>

          <div className="space-y-4 max-h-96 overflow-y-auto border rounded p-4">
            {notes.length === 0 && <p className="text-gray-500 text-2xl">Brak notatek</p>}
            {notes.map((note) => (
              <div
                key={note.id}
                className="border-b pb-2 last:border-none text-base relative"
              >
                {editingNoteId === note.id ? (
                  <div className="flex flex-col gap-2">
                    <textarea
                      value={editedNoteText}
                      onChange={(e) => setEditedNoteText(e.target.value)}
                      rows={3}
                      className="border rounded p-2 w-full"
                    />
                    <div className="flex gap-2 self-end">
                      <button
                        onClick={() => handleSaveEdit(note.id)}
                        className="bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700"
                      >
                        <Save size={16} className="inline-block mr-1"/>
                        Zapisz
                      </button>
                      <button
                        onClick={() => setEditingNoteId(null)}
                        className="bg-gray-300 text-black px-3 py-1 rounded hover:bg-gray-400"
                      >
                        <X size={16} className="inline-block mr-1"/>
                        Anuluj
                      </button>
                    </div>
                  </div>
                ) : (
                  <>
                    <p className="text-gray-800 dark:text-gray-100 whitespace-pre-wrap">{note.body}</p>
                    <div className="text-xs text-gray-500 mt-1">
                      {note.author.name} &middot; {new Date(note.createdAt).toLocaleString()}
                      {note.isOwner && (
                        <span className="ml-2 text-green-600 font-semibold">(Twoja notatka)</span>
                      )}
                    </div>

                    {note.isOwner && (
                      <div className="absolute top-0 right-0 flex gap-2">
                        <button
                          onClick={() => {
                            setEditingNoteId(note.id);
                            setEditedNoteText(note.body);
                          }}
                          className="text-blue-500 hover:text-blue-700"
                          title="Edytuj"
                        >
                          <Pencil size={16}/>
                        </button>
                        <button
                          onClick={() => handleDeleteNote(note.id)}
                          className="text-red-500 hover:text-red-700"
                          title="Usuń"
                        >
                          <Trash2 size={16}/>
                        </button>
                      </div>
                    )}
                  </>
                )}
              </div>
            ))}
          </div>

          <div className="mb-2 mt-2 flex flex-col space-y-2">
            <textarea
              value={newNote}
              onChange={(e) => setNewNote(e.target.value)}
              rows={3}
              placeholder=" Dodaj nową notatkę..."
              className="border text-xl rounded p-2 w-full resize-none"
            />
          </div>
        </div>
      </CardContent>

      <CardFooter className="flex flex-col items-center space-y-2">
            <button
              onClick={handleAddNote}
              disabled={submitting}
              className="bg-yellow-500 font-semibold text-slate-950 px-4 py-2 rounded hover:bg-yellow-700 hover:cursor-pointer disabled:opacity-50 mt-5"
            >
              {submitting ? 'Dodawanie...' : 'Dodaj notatkę'}
            </button>
        <button
          onClick={() => router.push('/request/list')}
          className="bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-800 px-4 py-2 font-semibold rounded hover:bg-gray-400 hover:cursor-pointer"
        >
          Powrót do listy
        </button>
        {user?.role === 'employee' && request.status === 'czeka' ? <button
          onClick={() => router.push(`/request/edit/${id}`)}
          className="bg-blue-500 text-white px-4 py-2 rounded hover:accent hover:cursor-pointer"
        >
          Edytuj zgłoszenie
        </button> : ''}

      </CardFooter>
    </Card>
  );
}