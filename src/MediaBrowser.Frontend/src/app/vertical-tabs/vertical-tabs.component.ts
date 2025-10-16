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
  selector: 'app-vertical-tabs',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="vertical-tabs" *ngIf="usersService.isAuthenticated()">
      <div class="tab-list">
        <button 
          *ngFor="let tab of tabs" 
          class="tab-button"
          [class.active]="isActiveRoute(tab.route)"
          (click)="navigateToRoute(tab.route)">
          <span class="tab-icon" [innerHTML]="tab.icon"></span>
        </button>
      </div>
      <div class="tab-footer">
        <button 
          class="tab-button logout-button"
          (click)="logout()"
          title="Logout">
          <span class="tab-icon"><i class="fa fa-sign-out"></i></span>
        </button>
      </div>
    </nav>
  `,
  styles: [`
    .vertical-tabs {
      background: var(--surface2-bg);
      box-shadow: 2px 0 5px rgba(0, 0, 0, 0.1);
      display: flex;
      flex-direction: column;
      position: fixed;
      z-index: 1000;
      justify-content: space-between;
    }

    /* Desktop styles */
    @media (min-width: 769px) {
      .vertical-tabs {
        width: 60px;
        height: 100vh;
        left: 0;
        top: 0;
      }
    }

    /* Mobile styles */
    @media (max-width: 768px) {
      .vertical-tabs {
        width: 100%;
        height: 70px;
        left: 0;
        bottom: 0;
        top: auto;
        flex-direction: row;
        box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.15);
        padding: 0 1rem;
      }
    }

    .tab-list {
      display: flex;
      flex-direction: column;
      padding: 1rem 0;
      gap: 0.5rem;
    }

    /* Desktop tab list */
    @media (min-width: 769px) {
      .tab-list {
        flex-direction: column;
        padding: 1rem 0;
        gap: 0.5rem;
      }
    }

    /* Mobile tab list */
    @media (max-width: 768px) {
      .tab-list {
        flex-direction: row;
        flex: 1;
        padding: 0.5rem 0;
        gap: 1rem;
        justify-content: space-evenly;
        align-items: center;
      }
    }

    .tab-footer {
      display: flex;
      flex-direction: column;
      padding: 1rem 0;
      border-top: 1px solid var(--fg3);
    }

    /* Desktop tab footer */
    @media (min-width: 769px) {
      .tab-footer {
        flex-direction: column;
        padding: 1rem 0;
        border-top: 1px solid var(--fg3);
        border-left: none;
      }
    }

    /* Mobile tab footer */
    @media (max-width: 768px) {
      .tab-footer {
        flex-direction: row;
        padding: 0.5rem 0;
        border-top: none;
        border-left: 1px solid var(--fg3);
        margin-left: 1rem;
        padding-left: 1rem;
      }
    }

    .tab-button {
      width: 44px;
      height: 44px;
      margin: 0 auto;
      border: none;
      border-radius: var(--radius2);
      background: var(--surface3-bg);
      color: var(--fg2);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.3s ease;
      position: relative;
    }

    /* Desktop tab button hover/active states */
    @media (min-width: 769px) {
      .tab-button:hover {
        color: var(--fg1);
        transform: translateX(2px);
      }

      .tab-button.active {
        color: var(--fg1);
        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
        transform: translateX(4px);
      }

      .tab-button.active::before {
        color: var(--fg1);
        content: '';
        position: absolute;
        left: -8px;
        top: 50%;
        transform: translateY(-50%);
        width: 4px;
        height: 24px;
        background: var(--fg1);
        border-radius: 0 2px 2px 0;
      }
    }

    /* Mobile tab button hover/active states */
    @media (max-width: 768px) {
      .tab-button:hover {
        color: var(--fg1);
        transform: translateY(-2px);
      }

      .tab-button.active {
        color: var(--fg1);
        box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.2);
        transform: translateY(-4px);
      }

      .tab-button.active::before {
        color: var(--fg1);
        content: '';
        position: absolute;
        left: 50%;
        transform: translateX(-50%);
        top: -8px;
        width: 24px;
        height: 4px;
        background: var(--fg1);
        border-radius: 0 0 2px 2px;
      }
    }

    .tab-icon {
      font-size: 20px;
      line-height: 1;
    }

    .logout-button:hover {
      color: var(--fg1);
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }
  `]
})
export class VerticalTabsComponent {
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