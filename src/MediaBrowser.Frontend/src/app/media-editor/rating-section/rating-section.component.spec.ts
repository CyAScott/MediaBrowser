import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { RatingSectionComponent } from './rating-section.component';

describe('RatingSectionComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RatingSectionComponent]
    }).compileComponents();
  });

  it('creates the component', () => {
    const fixture = TestBed.createComponent(RatingSectionComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('returns regular stars when rating is null and mixed stars when rating is set', () => {
    const fixture = TestBed.createComponent(RatingSectionComponent);
    const component = fixture.componentInstance;

    expect(component.getStarClass(1)).toBe('fa-regular fa-star');

    component.rating = 3;
    expect(component.getStarClass(1)).toBe('fa-solid fa-star');
    expect(component.getStarClass(3)).toBe('fa-solid fa-star');
    expect(component.getStarClass(4)).toBe('fa-regular fa-star');
  });

  it('updates rating and emits when setRating is called', () => {
    const fixture = TestBed.createComponent(RatingSectionComponent);
    const component = fixture.componentInstance;
    const emitSpy = vi.spyOn(component.ratingChange, 'emit');

    component.setRating(4);

    expect(component.rating).toBe(4);
    expect(emitSpy).toHaveBeenCalledWith(4);
    expect(emitSpy).toHaveBeenCalledTimes(1);
  });

  it('renders five star buttons and updates via click', () => {
    const fixture = TestBed.createComponent(RatingSectionComponent);
    const component = fixture.componentInstance;
    const emitSpy = vi.spyOn(component.ratingChange, 'emit');

    fixture.detectChanges();
    const starButtons = fixture.debugElement.queryAll(By.css('button.star-button'));

    expect(starButtons).toHaveLength(5);

    starButtons[1].nativeElement.click();
    fixture.detectChanges();

    expect(component.rating).toBe(2);
    expect(emitSpy).toHaveBeenCalledWith(2);

    const starIcons = fixture.debugElement.queryAll(By.css('button.star-button i'));
    expect(starIcons[0].nativeElement.className).toContain('fa-solid');
    expect(starIcons[1].nativeElement.className).toContain('fa-solid');
    expect(starIcons[2].nativeElement.className).toContain('fa-regular');
  });
});
