import { Routes } from '@angular/router';
import { LoginComponent } from './features/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { LancamentosListComponent } from './features/lancamentos/lancamentos-list/lancamentos-list.component';
import { LancamentoFormComponent } from './features/lancamentos/lancamento-form/lancamento-form.component';
import { ConsolidadoComponent } from './features/consolidado/consolidado.component';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'lancamentos',
    component: LancamentosListComponent,
    canActivate: [authGuard]
  },
  {
    path: 'lancamentos/novo',
    component: LancamentoFormComponent,
    canActivate: [authGuard]
  },
  {
    path: 'consolidado',
    component: ConsolidadoComponent,
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: '/dashboard' }
];
