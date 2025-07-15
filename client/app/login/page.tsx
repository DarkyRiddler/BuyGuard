// client/app/login/page.tsx
'use client';

import { Button } from '@/components/ui/button';
import axios from '@/lib/utils';

async function onSubmit() {
  console.log('onSubmit clicked');
  try {
    const { data } = await axios.post('auth/login', {
      'email': 'dwisniowski@gmail.com',
      'password': 'sigma',
    });
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

export default function Page() {
  return (
    <div className="flex flex-col gap-4 items-center justify-center h-screen">
      <Button onClick={onSubmit}>
        Ustaw Cookie
      </Button>
    </div>
  );
}
