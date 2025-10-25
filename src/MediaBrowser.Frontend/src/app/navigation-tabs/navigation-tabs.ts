import { Component, inject, HostListener } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UsersService } from '../services/users.service';
import { firstValueFrom } from 'rxjs';
import { SearchComponent } from '../search/search';

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
  isDropdownOpen = false;

  constructor() {}

  navigateToRoute(route: string, queryParams: { [key: string]: any } = {}): void {
    this.router.navigate([route], {
      queryParams: queryParams,
      queryParamsHandling: 'replace'
    });
  }

  navigateToSearch(): void {
    SearchComponent.clearPagePositionState();
    this.navigateToRoute('/search', { sort: SearchComponent.DEFAULT_SORT });
  }

  isActiveRoute(route: string): boolean {
    return this.router.url === route;
  }

  async logout(): Promise<void> {
    await firstValueFrom(this.usersService.logout());
    this.router.navigate(['/login']);
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  navigateToMeta(option: string): void {
    this.isDropdownOpen = false;
    this.router.navigate([`/meta/${option}`]);
  }

  isMetaRouteActive(): boolean {
    return this.router.url.startsWith('/meta/');
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.dropdown-container')) {
      this.isDropdownOpen = false;
    }
  }
}