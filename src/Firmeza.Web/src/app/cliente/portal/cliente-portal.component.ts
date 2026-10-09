import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { AuthService, UserProfileResponse } from '../../services/auth.service';

@Component({
  selector: 'app-cliente-portal',
  standalone: true,
  imports: [CommonModule, NavbarComponent],
  templateUrl: './cliente-portal.component.html'
})
export class ClientePortalComponent implements OnInit {
  private readonly authService = inject(AuthService);

  profile: UserProfileResponse | null = null;
  isLoading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.authService.getProfile().subscribe({
      next: (data) => {
        this.profile = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'No se pudo cargar la información de tu perfil. Verifica tu sesión.';
        console.error('Error al cargar perfil:', err);
      }
    });
  }

  get userEmail(): string {
    return this.profile?.email || this.authService.getUser()?.email || '';
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
