import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { AdminService } from '../../services/admin.service';
import { DashboardMetrics } from '../../models/dashboard.model';
import { Cliente } from '../../models/cliente.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly cdr = inject(ChangeDetectorRef);

  metrics: DashboardMetrics = {
    totalClientes: 0,
    totalClientesActivos: 0,
    totalEmpresas: 0,
    totalEmpresasActivas: 0
  };

  recentClientes: Cliente[] = [];
  isLoading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.markForCheck();

    this.adminService.getMetrics().subscribe({
      next: (data) => {
        this.metrics = data;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error al cargar métricas:', err);
      }
    });

    this.adminService.getClientes(1, 5, '', 'all', 'active').subscribe({
      next: (res) => {
        this.recentClientes = res.items || [];
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'No se pudieron sincronizar los datos del dashboard.';
        console.error('Error al cargar registros:', err);
        this.cdr.markForCheck();
      }
    });
  }

  formatPhone(phone: string | null | undefined): string {
    if (!phone) return '—';
    const digits = phone.replace(/\D/g, '');
    if (digits.length === 10) {
      return `+57 ${digits.slice(0, 3)} ${digits.slice(3, 6)} ${digits.slice(6)}`;
    }
    if (digits.length === 12 && digits.startsWith('57')) {
      const local = digits.slice(2);
      return `+57 ${local.slice(0, 3)} ${local.slice(3, 6)} ${local.slice(6)}`;
    }
    return phone;
  }
}
