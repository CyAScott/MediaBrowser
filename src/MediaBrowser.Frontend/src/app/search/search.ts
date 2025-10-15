import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Navigation, Router, RouterModule } from '@angular/router';
import { MediaReadModel, MediaService } from '../services/media.service';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';

interface SearchState {
  pageSize: number;
  pageIndex: number;
  keyword: string;
  results: MediaReadModel[];
  hasMoreResults: boolean;
  scrollPosition: number;
  sortBy: 'title' | 'createdOn' | 'duration' | 'userStarRating';
  sortDescending: boolean;
}

@Component({
  selector: 'app-search',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './search.html',
  styleUrls: ['./search.css']
})
export class SearchComponent implements OnInit, OnDestroy, AfterViewInit {
  private cdr = inject(ChangeDetectorRef);

  @ViewChild('searchResults', { static: false }) searchResultsElement!: ElementRef<HTMLElement>;
  
  cast: string | null = null;
  pageSize: number = 25;
  pageIndex: number = 0;
  keyword: string = '';
  results: MediaReadModel[] = [];
  isLoading: boolean = false;
  hasMoreResults: boolean = true;
  sortBy: 'title' | 'createdOn' | 'duration' | 'userStarRating' = 'title';
  sortDescending: boolean = false;
  
  readonly sortOptions = [
    { value: 'title', label: 'Title' },
    { value: 'createdOn', label: 'Created On' },
    { value: 'duration', label: 'Duration' },
    { value: 'userStarRating', label: 'Rating' }
  ] as const;
  
  private readonly STORAGE_KEY = 'search-component-state';
  private navigation: Navigation | null = null;
  private scrollRestorePending = false;
  private mainContentElement: HTMLElement | null = null;
  private scrollHandler: ((event: Event) => void) | null = null;

  constructor(private router: Router, private mediaService: MediaService) {
    this.navigation = this.router.currentNavigation();
    this.loadPersistedState();
  }

  onCardClick(): void {
    this.saveCurrentState();
  }

  trackByResultId(index: number, result: MediaReadModel): string {
    return result.id;
  }

  getStarDisplay(rating: number): string {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    let stars = '★'.repeat(fullStars);
    if (hasHalfStar) {
      stars += '☆';
    }
    return stars;
  }

  getCastTooltip(result: MediaReadModel): string {
    if (!result.cast || result.cast.length === 0) {
      return 'No cast information available';
    }
    return `Cast: ${result.cast.join(', ')}`;
  }

  async ngOnInit(): Promise<void> {

    const navigationState = this.navigation?.extras.state;
    if (navigationState) {
      this.cast = navigationState['cast'] || null;
    } else {
      this.cast = null;
    }

    // Only search if we don't have persisted results
    if (this.cast || this.results.length === 0) {
      await this.onSearch();
    } else {
      // We have persisted results, mark that we need to restore scroll position
      this.scrollRestorePending = true;
    }
  }

  ngAfterViewInit(): void {
    // Find the main content element that actually scrolls
    this.mainContentElement = document.querySelector('.main-content');
    
    // Create and store the scroll handler
    this.scrollHandler = this.onMainContentScroll.bind(this);
    
    // Add scroll listener to the main content element
    if (this.mainContentElement && this.scrollHandler) {
      this.mainContentElement.addEventListener('scroll', this.scrollHandler, { passive: true });
    }
    
    // Restore scroll position after view is initialized
    if (this.scrollRestorePending) {
      setTimeout(() => this.restoreScrollPosition(), 0);
    }
  }

  ngOnDestroy(): void {
    // Remove scroll listener to prevent memory leaks
    if (this.mainContentElement && this.scrollHandler) {
      this.mainContentElement.removeEventListener('scroll', this.scrollHandler);
    }
  }

  private onMainContentScroll(event: Event): void {
    const element = event.target as HTMLElement;
    this.handleScroll(element);
  }

  // Remove the unused HostListener methods
  // @HostListener('window:scroll', ['$event'])
  // @HostListener('document:scroll', ['$event'], { passive: true })

  async onSearch(): Promise<void> {
    // Reset for new search
    this.pageIndex = 0;
    this.results = [];
    this.hasMoreResults = true;
    
    // Clear scroll position for new search
    if (this.mainContentElement) {
      this.mainContentElement.scrollTop = 0;
    }
    
    await this.loadResults(true);
  }

  toggleSortDirection(): void {
    this.sortDescending = !this.sortDescending;
    this.onSearch(); // Re-search with new sort order
  }

  onSortChange(): void {
    this.onSearch(); // Re-search with new sort option
  }

  async loadResults(isNewSearch: boolean = false): Promise<void> {
    if (this.isLoading || (!this.hasMoreResults && !isNewSearch)) {
      return;
    }

    try {
      this.isLoading = true;
      const response = await firstValueFrom(this.mediaService.search({
        cast: this.cast ? [this.cast] : undefined,
        keywords: this.keyword || undefined,
        take: this.pageSize,
        skip: this.pageIndex * this.pageSize,
        sort: this.sortBy,
        descending: this.sortDescending,
      }));
      
      if (isNewSearch) {
        this.results = [...response.results];
      } else {
        this.results = [...this.results, ...response.results];
      }

      // Check if we have more results
      this.hasMoreResults = response.results.length === this.pageSize;
      
      // Increment page index for next load
      if (response.results.length > 0) {
        this.pageIndex++;
      }
      
      this.cdr.detectChanges();
      
      // Save state after loading results
      this.saveCurrentState();
      
      console.log('Results loaded, total:', this.results.length, 'hasMore:', this.hasMoreResults);
    } catch (error) {
      console.error('Search error:', error);
      if (isNewSearch) {
        this.results = [];
      }
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  private handleScroll(element: HTMLElement): void {
    const threshold = 100; // Load more when 100px from bottom
    
    // Save scroll position for persistence
    this.saveScrollPosition(element.scrollTop);
    
    if (element.scrollHeight - element.scrollTop - element.clientHeight < threshold) {
      this.loadResults();
    }
  }

  // Keep the old method for compatibility but make it call the new one
  onScroll(event: Event): void {
    const element = event.target as HTMLElement;
    this.handleScroll(element);
  }

  private loadPersistedState(): void {
    try {
      const savedState = localStorage.getItem(this.STORAGE_KEY);
      if (savedState) {
        const state: SearchState = JSON.parse(savedState);
        this.pageSize = state.pageSize;
        this.pageIndex = state.pageIndex;
        this.keyword = state.keyword;
        this.results = state.results || [];
        this.hasMoreResults = state.hasMoreResults;
        this.sortBy = state.sortBy || 'title';
        this.sortDescending = state.sortDescending || false;
      }
    } catch (error) {
      console.error('Error loading persisted search state:', error);
      // Reset to defaults on error
      this.resetToDefaults();
    }
  }

  private saveCurrentState(): void {
    try {
      const scrollPosition = this.getCurrentScrollPosition();
      const state: SearchState = {
        pageSize: this.pageSize,
        pageIndex: this.pageIndex,
        keyword: this.keyword,
        results: this.results,
        hasMoreResults: this.hasMoreResults,
        scrollPosition,
        sortBy: this.sortBy,
        sortDescending: this.sortDescending
      };
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
    } catch (error) {
      console.error('Error saving search state:', error);
    }
  }

  private saveScrollPosition(scrollTop: number): void {
    try {
      const savedState = localStorage.getItem(this.STORAGE_KEY);
      if (savedState) {
        const state: SearchState = JSON.parse(savedState);
        state.scrollPosition = scrollTop;
        localStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
      }
    } catch (error) {
      console.error('Error saving scroll position:', error);
    }
  }

  private getCurrentScrollPosition(): number {
    if (this.mainContentElement) {
      return this.mainContentElement.scrollTop;
    }
    return 0;
  }

  private restoreScrollPosition(): void {
    try {
      const savedState = localStorage.getItem(this.STORAGE_KEY);
      if (savedState && this.mainContentElement) {
        const state: SearchState = JSON.parse(savedState);
        if (state.scrollPosition > 0) {
          this.mainContentElement.scrollTop = state.scrollPosition;
        }
      }
    } catch (error) {
      console.error('Error restoring scroll position:', error);
    } finally {
      this.scrollRestorePending = false;
    }
  }

  private resetToDefaults(): void {
    this.pageSize = 25;
    this.pageIndex = 0;
    this.keyword = '';
    this.results = [];
    this.hasMoreResults = true;
    this.sortBy = 'title';
    this.sortDescending = false;
  }
}