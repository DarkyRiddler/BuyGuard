import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup, SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu, SidebarMenuButton, SidebarMenuItem,
} from '@/components/ui/sidebar';
import { User } from 'lucide-react';
import { DarkModeToggle } from '@/components/dark-mode-toggle';

const links = [
  {
    title: 'Konto',
    url: '/account',
    icon: User
  }
]

export function AppSidebar() {
  return (
    <Sidebar>
      <SidebarHeader>
        <h1 className="text-lg font-semibold">BuyGuard 🛒</h1>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Linki tu mozna dać</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {links.map((link) => (
                <SidebarMenuItem key={link.title}>
                  <SidebarMenuButton asChild>
                    <a href={link.url}>
                      <link.icon />
                      <span>{link.title}</span>
                    </a>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter>
        <DarkModeToggle />
      </SidebarFooter>
    </Sidebar>
  )
}