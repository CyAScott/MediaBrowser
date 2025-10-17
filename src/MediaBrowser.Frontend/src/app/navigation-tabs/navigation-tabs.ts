import { Component, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UsersService } from '../services/users.service';
import { firstValueFrom } from 'rxjs';

interface TabItem {
  route: string;
  icon: string;
}

@Component({
  selector: 'app-navigation-tabs',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navigation-tabs.html',
  styleUrls: ['./navigation-tabs.css']
})
export class NavigationTabsComponent {
  private router = inject(Router);
  protected usersService = inject(UsersService);

  tabs: TabItem[] = [
    {
      route: '/search',
      icon: '<i class="fa fa-film"></i>'
    },
    {
      route: '/import',
      icon: '<i class="fa fa-upload"></i>'
    },
    {
      route: '/cast',
      icon: '<i class="fa fa-users"></i>'
    }
  ];

  constructor() {}

  navigateToRoute(route: string): void {
    this.router.navigate([route]);
  }

  isActiveRoute(route: string): boolean {
    return this.router.url === route;
  }

  async logout(): Promise<void> {
    await firstValueFrom(this.usersService.logout());
    this.router.navigate(['/login']);
  }
}