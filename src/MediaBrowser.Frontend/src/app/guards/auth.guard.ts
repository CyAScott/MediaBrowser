import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UsersService } from '../services/users.service';

export const authGuard: CanActivateFn = async () => {
  const usersService = inject(UsersService);
  const router = inject(Router);

  if (!usersService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  return true;
};