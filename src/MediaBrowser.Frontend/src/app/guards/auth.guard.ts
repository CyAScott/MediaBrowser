import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UsersService } from '../services/users.service';

export const authGuard: CanActivateFn = async (route, state) => {
  const usersService = inject(UsersService);
  const router = inject(Router);

  try {
    await usersService.getCurrentUser();
    return true;
  } catch (error) {
    router.navigate(['/login']);
    return false;
  }
};