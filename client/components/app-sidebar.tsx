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
import { List, User, Users, UserRoundPlus } from 'lucide-react';
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
    icon: Users
  },
  //KIEDY BEDZIE GOTOWE, ODKOMENTUJCIE (seba nie gniewaj sie)
  //{
  //  title: 'Zgłoszenia',
  //  url: '/requestlist',
  //  icon: List
  //},
];

export function AppSidebar() {
  return (
    <Sidebar>
      <SidebarHeader className="dark:bg-gray-950">
        <a href = "/"><img src = "/logo_transparent.png" alt = "logo"/></a>
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