import { SidebarProvider } from '@/components/ui/sidebar';
import { AppSidebar } from '@/components/app-sidebar';
import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';

export default async function RootLayout({
                                           children,
                                         }: Readonly<{
  children: React.ReactNode;
}>) {
  const cookieStore = await cookies();
  const cookie = cookieStore.get('jwt');

  if (!cookie) {
    redirect('/login');
  } else {
    return (
      <SidebarProvider>
        <AppSidebar/>
        <main className="flex flex-col items-center justify-center min-h-screen w-full">
          {children}
        </main>
      </SidebarProvider>
    )
      ;
  }
}
