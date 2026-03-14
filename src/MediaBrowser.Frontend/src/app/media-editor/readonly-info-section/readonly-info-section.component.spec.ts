import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ReadonlyInfoSectionComponent } from './readonly-info-section.component';

describe('ReadonlyInfoSectionComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReadonlyInfoSectionComponent]
    }).compileComponents();
  });

  it('formats date/time, duration, and file sizes', () => {
    const fixture = TestBed.createComponent(ReadonlyInfoSectionComponent);
    const component = fixture.componentInstance;

    const toLocaleSpy = vi
      .spyOn(Date.prototype, 'toLocaleString')
      .mockReturnValue('mocked-locale-time');

    expect(component.formatDateTime('1700000000000')).toBe('mocked-locale-time');

    expect(ReadonlyInfoSectionComponent.formatDuration(undefined)).toBe('0:00');
    expect(ReadonlyInfoSectionComponent.formatDuration(65)).toBe('1:05');
    expect(ReadonlyInfoSectionComponent.formatDuration(3661)).toBe('1:01:01');
    expect(component.formatDuration(125)).toBe('2:05');

    expect(component.formatFileSize(undefined)).toBe('0.00 MB');
    expect(component.formatFileSize(5 * 1024 * 1024)).toBe('5.00 MB');

    toLocaleSpy.mockRestore();
  });
});
