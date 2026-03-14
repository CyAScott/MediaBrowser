import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TypeaheadInputComponent } from './typeahead-input.component';

describe('TypeaheadInputComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TypeaheadInputComponent]
    }).compileComponents();
  });

  it('creates the component', () => {
    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('filters suggestions and toggles dropdown on input/focus', () => {
    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    const component = fixture.componentInstance;

    component.suggestions = ['Alpha', 'Beta', 'Alphabet', 'Gamma', 'Alps', 'Alfred'];
    fixture.detectChanges();

    component.onInput({ target: { value: 'Al' } } as unknown as Event);

    expect(component.displayValue).toBe('Al');
    expect(component.filteredSuggestions).toEqual(['Alpha', 'Alphabet', 'Alps', 'Alfred']);
    expect(component.showDropdown).toBe(true);

    component.displayValue = '';
    component.onFocus();
    expect(component.showDropdown).toBe(false);

    component.displayValue = 'Al';
    component.onFocus();
    expect(component.showDropdown).toBe(true);
  });

  it('limits suggestions to five entries', () => {
    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    const component = fixture.componentInstance;

    component.suggestions = ['a1', 'a2', 'a3', 'a4', 'a5', 'a6', 'a7'];
    component.onInput({ target: { value: 'a' } } as unknown as Event);

    expect(component.filteredSuggestions).toHaveLength(5);
  });

  it('handles keyboard navigation and escape', () => {
    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    const component = fixture.componentInstance;

    component.suggestions = ['Alpha', 'Alfred'];
    component.onInput({ target: { value: 'Al' } } as unknown as Event);

    const preventDefault = vi.fn();
    component.onKeyDown({ key: 'ArrowDown', preventDefault } as unknown as KeyboardEvent);
    expect(component.highlightedIndex).toBe(0);

    component.onKeyDown({ key: 'ArrowDown', preventDefault } as unknown as KeyboardEvent);
    expect(component.highlightedIndex).toBe(1);

    component.onKeyDown({ key: 'ArrowUp', preventDefault } as unknown as KeyboardEvent);
    expect(component.highlightedIndex).toBe(0);

    component.onKeyDown({ key: 'Escape', preventDefault } as unknown as KeyboardEvent);
    expect(component.showDropdown).toBe(false);
    expect(component.highlightedIndex).toBe(-1);
  });

  it('selects highlighted suggestion on Enter and emits selection', () => {
    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    const component = fixture.componentInstance;

    const changeSpy = vi.fn();
    component.registerOnChange(changeSpy);
    const emitSpy = vi.spyOn(component.suggestionSelected, 'emit');

    component.suggestions = ['Alpha', 'Alfred'];
    component.onInput({ target: { value: 'Al' } } as unknown as Event);
    component.highlightedIndex = 1;

    component.onKeyDown({ key: 'Enter', preventDefault: vi.fn() } as unknown as KeyboardEvent);

    expect(component.displayValue).toBe('Alfred');
    expect(component.highlightedIndex).toBe(-1);
    expect(changeSpy).toHaveBeenCalledWith('Alfred');
    expect(emitSpy).toHaveBeenCalledWith('Alfred');
  });

  it('returns early for keydown when dropdown is hidden', () => {
    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    const component = fixture.componentInstance;

    component.showDropdown = false;
    component.filteredSuggestions = ['Alpha'];

    const preventDefault = vi.fn();
    component.onKeyDown({ key: 'ArrowDown', preventDefault } as unknown as KeyboardEvent);

    expect(preventDefault).not.toHaveBeenCalled();
  });

  it('handles blur and calls onTouched after timeout', () => {
    vi.useFakeTimers();
    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    const component = fixture.componentInstance;

    component.showDropdown = true;
    const touchedSpy = vi.fn();
    component.registerOnTouched(touchedSpy);

    component.onBlur();
    expect(touchedSpy).toHaveBeenCalledTimes(1);
    expect(component.showDropdown).toBe(true);

    vi.advanceTimersByTime(151);
    expect(component.showDropdown).toBe(false);
    vi.useRealTimers();
  });

  it('updates disabled state and writeValue', () => {
    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.setDisabledState(true);
    expect(component.inputElement.nativeElement.disabled).toBe(true);

    component.setDisabledState(false);
    expect(component.inputElement.nativeElement.disabled).toBe(false);

    component.writeValue('Preset');
    expect(component.displayValue).toBe('Preset');

    component.writeValue('');
    expect(component.displayValue).toBe('');
  });

  it('hides dropdown on outside document click and keeps it for inside click', () => {
    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.showDropdown = true;
    const insideElement = fixture.debugElement.query(By.css('.typeahead-container')).nativeElement as HTMLElement;
    insideElement.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    expect(component.showDropdown).toBe(true);

    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    expect(component.showDropdown).toBe(false);
  });

  it('calls add/remove listener lifecycle hooks', () => {
    const addSpy = vi.spyOn(document, 'addEventListener');
    const removeSpy = vi.spyOn(document, 'removeEventListener');

    const fixture = TestBed.createComponent(TypeaheadInputComponent);
    fixture.detectChanges();

    expect(addSpy).toHaveBeenCalledWith('click', expect.any(Function));

    fixture.componentInstance.ngOnDestroy();
    expect(removeSpy).toHaveBeenCalledWith('click', expect.any(Function));

    addSpy.mockRestore();
    removeSpy.mockRestore();
  });
});
