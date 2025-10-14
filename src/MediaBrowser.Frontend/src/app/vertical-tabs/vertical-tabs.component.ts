import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

interface TabItem {
  route: string;
  icon: string;
}

@Component({
  selector: 'app-vertical-tabs',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="vertical-tabs">
      <div class="tab-list">
        <button 
          *ngFor="let tab of tabs" 
          class="tab-button"
          [class.active]="isActiveRoute(tab.route)"
          (click)="navigateToRoute(tab.route)">
          <span class="tab-icon" [innerHTML]="tab.icon"></span>
        </button>
      </div>
    </nav>
  `,
  styles: [`
    .vertical-tabs {
      width: 60px;
      height: 100vh;
      background: #1E1E1E;
      box-shadow: 2px 0 5px rgba(0, 0, 0, 0.1);
      display: flex;
      flex-direction: column;
      position: fixed;
      left: 0;
      top: 0;
      z-index: 1000;
    }

    .tab-list {
      display: flex;
      flex-direction: column;
      padding: 1rem 0;
      gap: 0.5rem;
    }

    .tab-button {
      width: 44px;
      height: 44px;
      margin: 0 auto;
      border: none;
      border-radius: 8px;
      background: rgba(212, 212, 212, 0.1);
      color: #D4D4D4;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.3s ease;
      position: relative;
    }

    .tab-button:hover {
      background: rgba(212, 212, 212, 0.2);
      transform: translateX(2px);
    }

    .tab-button.active {
      background: rgba(212, 212, 212, 0.3);
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
      transform: translateX(4px);
    }

    .tab-button.active::before {
      content: '';
      position: absolute;
      left: -8px;
      top: 50%;
      transform: translateY(-50%);
      width: 4px;
      height: 24px;
      background: #D4D4D4;
      border-radius: 0 2px 2px 0;
    }

    .tab-icon {
      font-size: 20px;
      line-height: 1;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }
  `]
})
export class VerticalTabsComponent implements OnInit {
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

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.navigateToRoute('/search');
  }

  navigateToRoute(route: string): void {
    this.router.navigate([route]);
  }

  isActiveRoute(route: string): boolean {
    return this.router.url === route;
  }
}