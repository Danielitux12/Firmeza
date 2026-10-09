import { Routes } from '@angular/router';
import { HomeComponent } from './Home/home.component';
import { LoginComponent } from './Login/login.component';
import { AdminDashboardComponent } from './admin/dashboard/admin-dashboard.component';
import { AdminClientesComponent } from './admin/clientes/admin-clientes.component';
import { AdminEmpresasComponent } from './admin/empresas/admin-empresas.component';
import { ClientePortalComponent } from './cliente/portal/cliente-portal.component';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';

export const routes: Routes = [
  // Ruta pública principal
  { path: '', component: HomeComponent },

  // Autenticación
  { path: 'Login', component: LoginComponent },
  { path: 'login', component: LoginComponent },

  // Portal exclusivo para Clientes
  {
    path: 'cliente',
    component: ClientePortalComponent,
    canActivate: [authGuard, roleGuard],
    data: { expectedRole: 'Cliente' }
  },

  // Portal exclusivo para Administradores
  {
    path: 'admin',
    component: AdminDashboardComponent,
    canActivate: [authGuard, roleGuard],
    data: { expectedRole: 'Administrador' }
  },
  {
    path: 'admin/clientes',
    component: AdminClientesComponent,
    canActivate: [authGuard, roleGuard],
    data: { expectedRole: 'Administrador' }
  },
  {
    path: 'admin/empresas',
    component: AdminEmpresasComponent,
    canActivate: [authGuard, roleGuard],
    data: { expectedRole: 'Administrador' }
  },

  // Fallback
  { path: '**', redirectTo: '' }
];
