import { useEffect, useState } from 'react';

export default function Home() {
  const [data, setData] = useState(null);

  useEffect(() => {
    fetch('https://localhost:5159/weatherforecast') // domyślny endpoint ASP.NET
      .then(res => res.json())
      .then(setData);
  }, []);

  return (
    <div>
      <h1>Dane z backendu:</h1>
      <pre>{JSON.stringify(data, null, 2)}</pre>
    </div>
  );
}