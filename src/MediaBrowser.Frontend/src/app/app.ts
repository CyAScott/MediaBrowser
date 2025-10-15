import { Component, signal, inject, OnInit } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { VerticalTabsComponent } from './vertical-tabs/vertical-tabs.component';
import { UsersService } from './services/users.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, VerticalTabsComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('render');
  protected usersService = inject(UsersService);
  private router = inject(Router);

  async ngOnInit(): Promise<void> {
    // Try to load current user on app initialization
    try {
      await this.usersService.getCurrentUser();
      
      // If user is authenticated and on root path, redirect to search
      if (this.usersService.isAuthenticated() && this.router.url === '/') {
        this.router.navigate(['/search']);
      }
    } catch (error) {
      // If loading user fails and not on login page, redirect to login
      if (!this.router.url.startsWith('/login')) {
        this.router.navigate(['/login']);
      }
    }
  }
}
