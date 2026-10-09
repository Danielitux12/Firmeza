import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NavbarComponent } from '../shared/navbar/navbar.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './home.component.html',
})
export class HomeComponent {
  private readonly authService = inject(AuthService);
  protected readonly subtitle = signal('La plataforma integral para gestionar clientes y aliados comerciales con firmeza.');

  get currentUser() {
    return this.authService.getUser();
  }

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  get isCliente(): boolean {
    return this.authService.isCliente();
  }
}
