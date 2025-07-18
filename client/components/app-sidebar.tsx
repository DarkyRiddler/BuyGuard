import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup, SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu, SidebarMenuButton, SidebarMenuItem,
} from '@/components/ui/sidebar';
import { icons } from 'lucide-react';
import { DarkModeToggle } from '@/components/dark-mode-toggle';
import Link from 'next/link';

const links = [
  {
    title: 'Konto',
    url: '/account',
    icon: icons.User
  },
  {
    title: 'Login',
    url: '/login',
    icon: icons.Pencil
  },
  {
    title: 'Rejestracja',
    url: '/register',
    icon: icons.FilePenLine
  },
  {
    title: 'Użytkownicy',
    url: '/userlist',
    icon: icons.Users
  },
]

export function AppSidebar() {
  return (
    <Sidebar>
      <SidebarHeader className = 'dark:bg-gray-950'>
        <h1 className="text-3xl font-semibold text-center">BuyGuard 🛒</h1>
      </SidebarHeader>
      <SidebarContent className = 'dark:bg-gray-950 '>
        <SidebarGroup>
          <SidebarGroupLabel>Linki tu mozna dać</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {links.map((link) => (
                <SidebarMenuItem key={link.title}>
                  <SidebarMenuButton className = 'text-xl' asChild>
                    <Link href={link.url}>
                      {link.icon && <link.icon />}
                      <span>{link.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter  className = 'dark:bg-gray-950'>
        <DarkModeToggle />
      </SidebarFooter>
    </Sidebar>
  )
}