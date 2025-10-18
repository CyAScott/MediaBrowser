import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, ViewChild, ElementRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MediaReadModel } from '../services/media.service';
import { SpinnerComponent } from '../spinner/spinner';
import { ReadonlyInfoSectionComponent } from '../media-editor/readonly-info-section/readonly-info-section.component';

@Component({
  selector: 'app-search-content',
  imports: [CommonModule, RouterModule, SpinnerComponent],
  templateUrl: './search-content.html',
  styleUrls: ['./search-content.css']
})
export class SearchContentComponent {
  @Input() hasMoreResults: boolean = true;
  @Input() isLoading: boolean = false;
  @Input() results: MediaReadModel[] = [];

  @Output() scroll = new EventEmitter<Event>();
  @Output() cardClick = new EventEmitter<void>();

  @ViewChild('searchResults', { static: false }) searchResultsElement!: ElementRef<HTMLDivElement>;

  onCardClick(): void {
    this.cardClick.emit();
  }

  getTooltip(result: MediaReadModel): string {
    let tooltip = result.title;

    if (result.duration) {
      tooltip += `\nDuration: ${ReadonlyInfoSectionComponent.formatDuration(result.duration)}`;
    }
    
    if (result.userStarRating) {
      tooltip += `\nRating: ${result.userStarRating} star(s)`;
    }

    if (result.cast && result.cast.length > 0) {
      tooltip += `\nCast: ${result.cast.join(', ')}`;
    }

    return tooltip;
  }

  onScroll(event: Event): void {
    this.scroll.emit(event);
  }

  trackByResultId(index: number, result: MediaReadModel): string {
    return result.id;
  }
}