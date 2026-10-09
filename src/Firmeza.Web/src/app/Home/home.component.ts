import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { AdminService } from '../services/admin.service';
import { NavbarComponent } from '../shared/navbar/navbar.component';
import { DashboardMetrics } from '../models/dashboard.model';
import { Cliente } from '../models/cliente.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly adminService = inject(AdminService);

  today = new Date();
  metrics = signal<DashboardMetrics>({
    totalClientes: 0,
    totalClientesActivos: 0,
    totalEmpresas: 0,
    totalEmpresasActivas: 0
  });
  recentClientes = signal<Cliente[]>([]);
  isLoadingRecent = signal(false);

  get currentUser() {
    return this.authService.getUser();
  }

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  get isCliente(): boolean {
    return this.authService.isCliente();
  }

  // Saludo dinámico según la hora sin mostrar correo
  get greeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Buenos días';
    if (hour < 19) return 'Buenas tardes';
    return 'Buenas noches';
  }

  // Indicadores corporativos
  totalClientes = computed(() => this.metrics().totalClientes);
  empresasActivas = computed(() => this.metrics().totalEmpresasActivas);
  pendientes = computed(() => this.metrics().totalClientes - this.metrics().totalClientesActivos);
  nuevosEsteMes = computed(() => {
    const total = this.metrics().totalClientes;
    return total > 0 ? Math.max(1, Math.round(total * 0.25)) : 0;
  });

  ngOnInit(): void {
    if (this.isAdmin) {
      this.adminService.getMetrics().subscribe({
        next: (data) => this.metrics.set(data),
        error: (err) => console.error('Error al cargar métricas:', err)
      });

      this.isLoadingRecent.set(true);
      this.adminService.getClientes(1, 5, '', 'all', 'active').subscribe({
        next: (res) => {
          this.recentClientes.set(res.items || []);
          this.isLoadingRecent.set(false);
        },
        error: () => this.isLoadingRecent.set(false)
      });
    }
  }

  downloadClientesExcel(): void {
    this.adminService.exportClientesExcel().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Clientes_${new Date().toISOString().slice(0, 10)}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }

  downloadEmpresasPdf(): void {
    this.adminService.exportEmpresasPdf().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Empresas_${new Date().toISOString().slice(0, 10)}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }
}
