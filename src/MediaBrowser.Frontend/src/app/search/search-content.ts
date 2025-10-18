import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, ViewChild, ElementRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MediaReadModel } from '../services/media.service';
import { SpinnerComponent } from '../spinner/spinner';

@Component({
  selector: 'app-search-content',
  imports: [CommonModule, RouterModule, SpinnerComponent],
  templateUrl: './search-content.html',
  styleUrls: ['./search-content.css']
})
export class SearchContentComponent {
  @ViewChild('searchResults', { static: false }) searchResultsElement!: ElementRef<HTMLDivElement>;

  @Input() results: MediaReadModel[] = [];
  @Input() hasMoreResults: boolean = true;
  @Input() isLoading: boolean = false;

  @Output() scroll = new EventEmitter<Event>();
  @Output() cardClick = new EventEmitter<void>();

  trackByResultId(index: number, result: MediaReadModel): string {
    return result.id;
  }

  getCastTooltip(result: MediaReadModel): string {
    if (!result.cast || result.cast.length === 0) {
      return 'No cast information available';
    }
    return `Cast: ${result.cast.join(', ')}`;
  }

  onScroll(event: Event): void {
    this.scroll.emit(event);
  }

  onCardClick(): void {
    this.cardClick.emit();
  }
}