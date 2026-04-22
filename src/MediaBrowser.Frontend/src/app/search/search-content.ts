import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, ViewChild, ElementRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MediaReadModel } from '../services/media.service';
import { SpinnerComponent } from '../spinner/spinner';
import { ReadonlyInfoSectionComponent } from '../media-editor/readonly-info-section/readonly-info-section.component';
import { SearchQueryParams } from './search-query-params';
import { PlayerNavigationState } from '../player/player';

@Component({
  selector: 'app-search-content',
  imports: [CommonModule, RouterModule, SpinnerComponent],
  templateUrl: './search-content.html',
  styleUrls: ['./search-content.css']
})
export class SearchContentComponent {
  protected readonly SearchQueryParams = SearchQueryParams;
  @Input() hasMoreResults: boolean = true;
  @Input() isLoading: boolean = false;
  @Input() results: MediaReadModel[] = [];
  @Input() parameters: SearchQueryParams = new SearchQueryParams();

  @Output() scroll = new EventEmitter<Event>();
  @Output() cardClick = new EventEmitter<void>();

  @ViewChild('searchResults', { static: false }) searchResultsElement!: ElementRef<HTMLDivElement>;

  onCardClick(): void {
    this.cardClick.emit();
  }

  getPlayerNavigationState(result: MediaReadModel, index: number): PlayerNavigationState {
    return {
      mediaData: result,
      searchContext: {
        currentIndex: index,
        searchParams: this.parameters
      }
    };
  }

  getTooltip(result: MediaReadModel): string {
    let tooltip = result.title;

    if (result.duration && !result.mime.startsWith('image/')) {
      tooltip += `\nDuration: ${ReadonlyInfoSectionComponent.formatDuration(result.duration)}`;
    }
    
    if (result.userStarRating) {
      tooltip += `\nRating: ${result.userStarRating} star(s)`;
    }

    if (result.cast && result.cast.length > 0) {
      tooltip += `\nCast: ${result.cast.join(', ')}`;
    }

    if (result.genres && result.genres.length > 0) {
      tooltip += `\nGenres: ${result.genres.join(', ')}`;
    }

    if (result.directors && result.directors.length > 0) {
      tooltip += `\nDirectors: ${result.directors.join(', ')}`;
    }

    if (result.producers && result.producers.length > 0) {
      tooltip += `\nProducers: ${result.producers.join(', ')}`;
    }

    if (result.writers && result.writers.length > 0) {
      tooltip += `\nWriters: ${result.writers.join(', ')}`;
    }

    return tooltip;
  }

  onScroll(event: Event): void {
    this.scroll.emit(event);
  }

  trackByResultId(index: number, result: MediaReadModel): string {
    return result.id;
  }

  isImage(result: MediaReadModel): boolean {
    return result.mime.startsWith('image/');
  }

  isVideo(result: MediaReadModel): boolean {
    return result.mime.startsWith('video/');
  }

  isAudio(result: MediaReadModel): boolean {
    return result.mime.startsWith('audio/');
  }

  getCenterIconClass(result: MediaReadModel): string {
    if (this.isImage(result)) {
      return 'fa-magnifying-glass';
    } else if (this.isVideo(result)) {
      return 'fa-play';
    } else if (this.isAudio(result)) {
      return 'fa-volume-high';
    }
    return 'fa-play';
  }

  getFileTypeIcon(result: MediaReadModel): string {
    if (this.isImage(result)) {
      return 'fa-image';
    } else if (this.isVideo(result)) {
      return 'fa-film';
    } else if (this.isAudio(result)) {
      return 'fa-music';
    }
    return 'fa-file';
  }
}