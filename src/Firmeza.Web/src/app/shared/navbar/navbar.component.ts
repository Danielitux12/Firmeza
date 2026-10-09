import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html'
})
export class NavbarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  get currentUser() {
    return this.authService.getUser();
  }

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  get isCliente(): boolean {
    return this.authService.isCliente();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/Login']);
  }
}
