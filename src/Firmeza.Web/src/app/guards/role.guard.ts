import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.getUser();
  const expectedRole = route.data['expectedRole'];

  if (!user) {
    router.navigate(['/Login']);
    return false;
  }

  if (expectedRole && user.role !== expectedRole) {
    // Si no coincide el rol, redirige al portal correspondiente
    if (user.role === 'Administrador') {
      router.navigate(['/admin']);
    } else if (user.role === 'Cliente') {
      router.navigate(['/cliente']);
    } else {
      router.navigate(['/']);
    }
    return false;
  }

  return true;
};
