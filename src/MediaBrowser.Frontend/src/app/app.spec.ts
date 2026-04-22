import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { App } from './app';
import { ToastService } from './toast/toast.service';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideZonelessChangeDetection()]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the application layout', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-navigation-tabs')).toBeTruthy();
    expect(compiled.querySelector('main.main-content')).toBeTruthy();
  });

  it('should render a success toast at the app shell when one is shown', () => {
    const fixture = TestBed.createComponent(App);
    const toastService = TestBed.inject(ToastService);

    toastService.showSuccess('Saved successfully');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const toast = compiled.querySelector('.toast');

    expect(toast?.textContent).toContain('Saved successfully');
    expect(toast?.classList.contains('toast--success')).toBe(true);
  });
});
