import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
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
}
