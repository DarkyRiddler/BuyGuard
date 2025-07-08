import { NextResponse } from 'next/server';

export async function GET(request: Request) {
  const cookieHeader = request.headers.get('cookie') || '';
  const cookies = cookieHeader.split('; ').reduce((acc, cur) => {
    const [key, value] = cur.split('=');
    acc[key] = value;
    return acc;
  }, {} as Record<string, string>);

  const jwt = cookies['jwt'] || null;

  return NextResponse.json({ jwt });
}
