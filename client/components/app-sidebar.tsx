import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup, SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu, SidebarMenuButton, SidebarMenuItem,
} from '@/components/ui/sidebar';
import { List, User, UserRoundPlus, SquarePen } from 'lucide-react';
import { DarkModeToggle } from '@/components/dark-mode-toggle';
import Link from 'next/link';

const links = [
  {
    title: 'Rejestracja',
    url: '/register',
    icon: UserRoundPlus
  },
  {
    title: 'Użytkownicy',
    url: '/user/list',
    icon: List
  },
  {
    title: 'Zgłoszenia',
    url: '/request',
    icon: List
  },
  {
    title: 'Zgłoszenia - edycja',
    url: '/editrequest',
    icon: SquarePen
  },
];

export function AppSidebar() {
  return (
    <Sidebar>
      <SidebarHeader className="dark:bg-gray-950">
        <h1 className="text-lg font-semibold text-center">BuyGuard 🛒</h1>
      </SidebarHeader>
      <SidebarContent className="dark:bg-gray-950">
        <SidebarGroup>
          <SidebarGroupLabel>Linki tu mozna dać</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {links.map((link) => (
                <SidebarMenuItem key={link.title}>
                  <SidebarMenuButton asChild>
                    <Link href={link.url}>
                      {link.icon && <link.icon/>}
                      <span>{link.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter className="dark:bg-gray-950">
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton asChild>
              <Link href={'/account'}>
                <User/>
                <span>Konto</span>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
          <DarkModeToggle/>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  );
}