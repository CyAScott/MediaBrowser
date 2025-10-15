import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UsersService } from '../services/users.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  private usersService = inject(UsersService);
  private router = inject(Router);

  username: string = '';
  password: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';

  async onSubmit(): Promise<void> {
    if (!this.username.trim() || !this.password.trim()) {
      this.errorMessage = 'Please enter both username and password';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    try {
      await firstValueFrom(this.usersService.login({
        username: this.username.trim(),
        password: this.password
      }));
      this.router.navigate(['/search']);
    } catch (error) {
      console.error('Login error:', error);
      this.errorMessage = 'An error occurred during login. Please try again.';
    } finally {
      this.isLoading = false;
    }
  }

  onUsernameKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSubmit();
    }
  }

  onPasswordKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSubmit();
    }
  }
}