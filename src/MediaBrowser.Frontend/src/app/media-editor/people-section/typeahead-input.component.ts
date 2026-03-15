import { CommonModule } from '@angular/common';
import { Component, EventEmitter, forwardRef, Input, Output, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-typeahead-input',
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TypeaheadInputComponent),
      multi: true
    }
  ],
  templateUrl: './typeahead-input.component.html',
  styleUrls: ['../media-editor.css', './people-section.css', './typeahead-input.component.css']
})
export class TypeaheadInputComponent implements ControlValueAccessor, OnInit, OnDestroy {
  @ViewChild('inputElement', { static: true }) inputElement!: ElementRef<HTMLInputElement>;
  
  @Input() suggestions: string[] = [];
  @Input() placeholder: string = '';
  @Output() suggestionSelected = new EventEmitter<string>();

  displayValue: string = '';
  filteredSuggestions: string[] = [];
  showDropdown: boolean = false;
  highlightedIndex: number = -1;

  private onChange = (value: string) => {};
  private onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.typeahead-container')) {
      this.showDropdown = false;
    }
  }
  private onTouched = () => {};
  private updateFilteredSuggestions(): void {
    if (!this.displayValue) {
      this.filteredSuggestions = [];
      return;
    }

    const searchValue = this.displayValue.toLowerCase();
    this.filteredSuggestions = this.suggestions
      .filter(suggestion => suggestion.toLowerCase().includes(searchValue))
      .slice(0, 5); // Limit to 5 suggestions for performance
  }

  ngOnDestroy(): void {
    document.removeEventListener('click', this.onDocumentClick.bind(this));
  }

  ngOnInit(): void {
    document.addEventListener('click', this.onDocumentClick.bind(this));
  }

  onBlur(): void {
    // Delay hiding dropdown to allow for mouse clicks on suggestions
    setTimeout(() => {
      this.showDropdown = false;
    }, 150);
    this.onTouched();
  }

  onFocus(): void {
    this.updateFilteredSuggestions();
    this.showDropdown = this.displayValue.length > 0 && this.filteredSuggestions.length > 0;
  }

  onInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.displayValue = target.value;
    this.updateFilteredSuggestions();
    this.showDropdown = this.displayValue.length > 0 && this.filteredSuggestions.length > 0;
    this.highlightedIndex = -1;
    this.onChange(this.displayValue);
  }

  onKeyDown(event: KeyboardEvent): void {
    if (!this.showDropdown || this.filteredSuggestions.length === 0) {
      return;
    }

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.highlightedIndex = Math.min(this.highlightedIndex + 1, this.filteredSuggestions.length - 1);
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.highlightedIndex = Math.max(this.highlightedIndex - 1, -1);
        break;
      case 'Enter':
        event.preventDefault();
        if (this.highlightedIndex >= 0) {
          this.selectSuggestion(this.filteredSuggestions[this.highlightedIndex]);
        }
        break;
      case 'Escape':
        event.preventDefault();
        this.showDropdown = false;
        this.highlightedIndex = -1;
        break;
    }
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  selectSuggestion(suggestion: string): void {
    this.displayValue = suggestion;
    this.showDropdown = false;
    this.highlightedIndex = -1;
    this.onChange(suggestion);
    this.suggestionSelected.emit(suggestion);
    
    // Focus the input element to maintain user experience
    if (this.inputElement) {
      this.inputElement.nativeElement.focus();
    }
  }

  setDisabledState(isDisabled: boolean): void {
    if (this.inputElement) {
      this.inputElement.nativeElement.disabled = isDisabled;
    }
  }

  focus(): void {
    if (this.inputElement) {
      this.inputElement.nativeElement.focus();
    }
  }

  writeValue(value: string): void {
    this.displayValue = value || '';
  }
}