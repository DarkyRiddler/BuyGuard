import { SidebarProvider } from '@/components/ui/sidebar';
import { AppSidebar } from '@/components/app-sidebar';

export default function RootLayout({
                                     children,
                                   }: Readonly<{
  children: React.ReactNode;
}>) {
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
