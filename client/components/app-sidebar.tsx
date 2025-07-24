import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup, SidebarGroupContent,
  SidebarHeader,
  SidebarMenu, SidebarMenuButton, SidebarMenuItem,
} from '@/components/ui/sidebar';
import { List, User, Users, UserRoundPlus } from 'lucide-react';
import { DarkModeToggle } from '@/components/dark-mode-toggle';
import AppLogo from '@/public/logo_transparent.png';
import Link from 'next/link';
import Image from 'next/image';

const links = [
  {
    title: 'Rejestracja',
    url: '/register',
    icon: UserRoundPlus
  },
  {
    title: 'Użytkownicy',
    url: '/user/list',
    icon: Users
  },
  {
    title: 'Lista zgłoszeń',
    url: '/request/list',
    icon: List
  },
  {
    title: 'Dodaj zgłoszenie', // pozniej ogarne conditional rendering
    url: '/request/create',
    icon: List
  },

];

export function AppSidebar() {
  return (
    <Sidebar>
      <SidebarHeader className="dark:bg-gray-950">
        <Link href="/"><Image src={AppLogo} priority={true} alt="logo"/></Link>
      </SidebarHeader>
      <SidebarContent className="dark:bg-gray-950">
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {links.map((link) => (
                <SidebarMenuItem key={link.title}>
                  <SidebarMenuButton className = 'text-xl' asChild>
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
                <span className = "text-lg">Konto</span>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
          <DarkModeToggle/>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  );
}