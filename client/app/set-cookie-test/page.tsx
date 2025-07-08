// client/app/set-cookie-test/page.tsx
'use client';

import { useState } from 'react';

async function setCookie() {
  console.log('setCookie clicked');
  const res = await fetch('https://localhost:7205/set-cookie', {
    method: 'POST',
    credentials: 'include',
  });

  const data = await res.json();
  console.log(data);
}
export default function Page() {
  
  const [message, setMessage] = useState<string>('Brak cookie');
  
  async function getCookie() {
    console.log('getCookie clicked');
    const res = await fetch('api/get-cookie', {})
    if (res.status === 200) {
      const cookie = await res.json();
      console.log(cookie)
      if (cookie.jwt) {
        setMessage(cookie.jwt)
      } else {
        setMessage('Cookie not found');
      }
    }
  }
  
  return (
    <div className="flex flex-col gap-4 items-center justify-center h-screen">
    <button onClick={setCookie}>
      Ustaw Cookie
    </button>
      <button onClick={getCookie}>Zobacz zawartość cookie</button>
      <p>{message}</p>
    </div>
  );
}
