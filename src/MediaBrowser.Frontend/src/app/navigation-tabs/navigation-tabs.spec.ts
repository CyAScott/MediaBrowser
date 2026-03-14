import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of } from 'rxjs';
import { NavigationTabsComponent } from './navigation-tabs';
import { UsersService } from '../services/users.service';
import { SearchComponent } from '../search/search';

describe('NavigationTabsComponent', () => {
  let component: NavigationTabsComponent;

  const routerMock = {
    url: '/search',
    navigate: vi.fn()
  };

  const usersServiceMock = {
    logout: vi.fn(() => of(void 0)),
    isAuthenticated: vi.fn(() => true)
  };

  beforeEach(() => {
    routerMock.url = '/search';
    routerMock.navigate.mockReset();
    usersServiceMock.logout.mockClear();
    usersServiceMock.isAuthenticated.mockClear();

    TestBed.configureTestingModule({
      imports: [NavigationTabsComponent],
      providers: [
        { provide: Router, useValue: routerMock },
        { provide: UsersService, useValue: usersServiceMock }
      ]
    });

    const fixture = TestBed.createComponent(NavigationTabsComponent);
    component = fixture.componentInstance;
  });

  it('uses SearchComponent default sort', () => {
    expect(component.defaultSort).toBe(SearchComponent.DEFAULT_SORT);
  });

  it('returns true only for the exact active route', () => {
    routerMock.url = '/search';
    expect(component.isActiveRoute('/search')).toBe(true);
    expect(component.isActiveRoute('/import')).toBe(false);
  });

  it('logs out and navigates to login', async () => {
    await component.logout();

    expect(usersServiceMock.logout).toHaveBeenCalledTimes(1);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('toggles dropdown visibility', () => {
    expect(component.isDropdownOpen).toBe(false);

    component.toggleDropdown();
    expect(component.isDropdownOpen).toBe(true);

    component.toggleDropdown();
    expect(component.isDropdownOpen).toBe(false);
  });

  it('navigates to selected metadata section and closes dropdown', () => {
    component.isDropdownOpen = true;

    component.navigateToMeta('genres');

    expect(component.isDropdownOpen).toBe(false);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/meta/genres']);
  });

  it('detects metadata route activity based on /meta/ prefix', () => {
    routerMock.url = '/meta/cast';
    expect(component.isMetaRouteActive()).toBe(true);

    routerMock.url = '/search';
    expect(component.isMetaRouteActive()).toBe(false);
  });

  it('closes dropdown on outside document click', () => {
    component.isDropdownOpen = true;

    const outsideElement = document.createElement('div');
    component.onDocumentClick({ target: outsideElement } as unknown as Event);

    expect(component.isDropdownOpen).toBe(false);
  });

  it('keeps dropdown open when clicking inside dropdown container', () => {
    component.isDropdownOpen = true;

    const container = document.createElement('div');
    container.className = 'dropdown-container';

    const insideElement = document.createElement('span');
    container.appendChild(insideElement);

    component.onDocumentClick({ target: insideElement } as unknown as Event);

    expect(component.isDropdownOpen).toBe(true);
  });
});
