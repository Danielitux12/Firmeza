import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  showPassword = false;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  // Validaciones sencillas
  emailError = '';
  passwordError = '';

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  fillCredentials(role: 'admin' | 'cliente'): void {
    if (role === 'admin') {
      this.email = 'admin@firmeza.com';
      this.password = 'Admin123*';
    } else {
      this.email = 'cliente@firmeza.com';
      this.password = 'Cliente123*';
    }
    this.validateForm();
  }

  validateForm(): boolean {
    let isValid = true;
    this.emailError = '';
    this.passwordError = '';

    // Validación sencilla de Correo
    const cleanEmail = this.email.trim();
    if (!cleanEmail) {
      this.emailError = 'El correo electrónico es obligatorio.';
      isValid = false;
    } else if (!cleanEmail.includes('@') || !cleanEmail.includes('.')) {
      this.emailError = 'Ingresa un formato de correo válido (ej: usuario@correo.com).';
      isValid = false;
    }

    // Validación sencilla de Contraseña
    if (!this.password) {
      this.passwordError = 'La contraseña es obligatoria.';
      isValid = false;
    } else if (this.password.length < 6) {
      this.passwordError = 'La contraseña debe tener al menos 6 caracteres.';
      isValid = false;
    }

    return isValid;
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.validateForm()) {
      return;
    }

    this.isLoading = true;
    this.authService.login({ email: this.email.trim(), password: this.password }).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.successMessage = `¡Bienvenido! Sesión iniciada como ${res.role}. Redirigiendo...`;
        setTimeout(() => {
          if (res.role === 'Administrador') {
            this.router.navigate(['/admin']);
          } else if (res.role === 'Cliente') {
            this.router.navigate(['/cliente']);
          } else {
            this.router.navigate(['/']);
          }
        }, 150);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Error al iniciar sesión:', err);
        if (err.status === 401) {
          this.errorMessage = err.error?.detail || 'Correo o contraseña incorrectos.';
        } else if (err.status === 0) {
          this.errorMessage = 'No se pudo conectar con el servidor de la API (http://localhost:5246). Asegúrate de que esté encendida.';
        } else {
          this.errorMessage = err.error?.detail || err.error?.message || 'Ocurrió un error inesperado al procesar la solicitud.';
        }
      }
    });
  }
}
