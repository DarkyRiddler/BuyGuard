export default function LoginLayout({ children }: { children: React.ReactNode }) {
  return (
    <main className="flex flex-col items-center justify-center min-h-screen w-full">
      <div className = "relative w-1/3 ">
          <h1 className = "absolute bottom-20 w-1/1 text-4xl text-slate-900 dark:text-yellow-500 font-bold text-center"> BuyGuard 
          </h1>

          <h2 className = 'absolute bottom-10 w-1/1 text-2xl text-slate-900 dark:text-sky-50 font bold text-center'>Zaloguj się</h2>
        </div>
      
      {children}
    </main>
  );
}
