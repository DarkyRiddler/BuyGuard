export default function LoginLayout({ children }: { children: React.ReactNode }) {
  return (
    <main className="flex flex-col items-center justify-center min-h-screen w-full">
      {children}
    </main>
  );
}
