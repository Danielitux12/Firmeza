import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { AdminService } from '../../services/admin.service';
import { DashboardMetrics } from '../../models/dashboard.model';

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

  isLoading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.markForCheck();
    this.adminService.getMetrics().subscribe({
      next: (data) => {
        this.metrics = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'No se pudieron cargar las métricas del sistema. Verifica que la API esté activa.';
        console.error('Error al cargar métricas:', err);
        this.cdr.markForCheck();
      }
    });
  }
}
