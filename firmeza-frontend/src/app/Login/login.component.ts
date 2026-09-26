import { Component, signal } from '@angular/core';
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
})
export class LoginComponent {
  protected readonly title = signal('firmeza-frontend');
}
