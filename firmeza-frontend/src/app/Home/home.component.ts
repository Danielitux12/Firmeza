import { Component, signal } from '@angular/core';
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  protected readonly subtitle = signal('La plataforma que necesitas para gestionar tu negocio con firmeza.');
}
