import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
})
export class HomeComponent {
  private readonly authService = inject(AuthService);
  protected readonly subtitle = signal('La plataforma que necesitas para gestionar tu negocio con firmeza.');

  get currentUser() {
    return this.authService.getUser();
  }

  logout() {
    this.authService.logout();
  }
}
