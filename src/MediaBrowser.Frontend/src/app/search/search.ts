import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MediaReadModel, MediaService, SearchMediaRequest } from '../services/media.service';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';

interface SearchState {
  pageSize: number;
  pageIndex: number;
  scrollPosition: number;
}

/**
 * SearchComponent that uses URL query parameters for search state management.
 * This allows for bookmarkable search results and proper browser navigation.
 * 
 * Query parameters correspond directly to SearchMediaRequest interface:
 * - keywords: string search term
 * - cast: array of cast member names
 * - genres: array of genre names  
 * - sort: sort field ('title' | 'createdOn' | 'duration' | 'userStarRating')
 * - descending: boolean for sort direction
 * - take: number of results per page
 * 
 * Example URLs:
 * /search?keywords=action&sort=createdOn&descending=true
 * /search?cast=John%20Doe&cast=Jane%20Smith&genres=Action
 */
@Component({
  selector: 'app-search',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './search.html',
  styleUrls: ['./search.css']
})
export class SearchComponent implements OnInit, OnDestroy, AfterViewInit {
  private cdr = inject(ChangeDetectorRef);
  private route = inject(ActivatedRoute);

  @ViewChild('searchResults', { static: false }) searchResultsElement!: ElementRef<HTMLElement>;
  
  // Search parameters that match SearchMediaRequest
  cast: string[] = [];
  genres: string[] = [];
  keywords: string = '';
  sort: 'title' | 'createdOn' | 'duration' | 'userStarRating' = 'title';
  descending: boolean = false;
  take: number = 25;
  skip: number = 0;
  
  // UI state
  results: MediaReadModel[] = [];
  isLoading: boolean = false;
  hasMoreResults: boolean = true;
  pageIndex: number = 0;

  /**
   * Helper method to create search query parameters for navigation
   * @param params Search parameters that match SearchMediaRequest interface
   * @returns Query parameters object for router navigation
   */
  static createSearchQueryParams(params: Partial<SearchMediaRequest>): { [key: string]: string | string[] } {
    const queryParams: { [key: string]: string | string[] } = {};
    
    if (params.keywords?.trim()) {
      queryParams['keywords'] = params.keywords.trim();
    }
    if (params.cast && params.cast.length > 0) {
      queryParams['cast'] = params.cast;
    }
    if (params.genres && params.genres.length > 0) {
      queryParams['genres'] = params.genres;
    }
    if (params.sort && params.sort !== 'title') {
      queryParams['sort'] = params.sort;
    }
    if (params.descending) {
      queryParams['descending'] = 'true';
    }
    if (params.take && params.take !== 25) {
      queryParams['take'] = params.take.toString();
    }
    
    return queryParams;
  }
  
  readonly sortOptions = [
    { value: 'title', label: 'Title' },
    { value: 'createdOn', label: 'Created On' },
    { value: 'duration', label: 'Duration' },
    { value: 'userStarRating', label: 'Rating' }
  ] as const;
  
  private readonly STORAGE_KEY = 'search-component-scroll';
  private mainContentElement: HTMLElement | null = null;
  private scrollHandler: ((event: Event) => void) | null = null;

  constructor(private router: Router, private mediaService: MediaService) {}

  onCardClick(): void {
    this.saveScrollPosition();
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
    // Load initial state from query parameters
    this.loadFromQueryParams();

    // Always perform search based on current parameters
    await this.onSearch();
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
    
    // Restore scroll position if available
    setTimeout(() => this.restoreScrollPosition(), 0);
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

  async onSearch(): Promise<void> {
    // Reset pagination for new search
    this.pageIndex = 0;
    this.skip = 0;
    this.results = [];
    this.hasMoreResults = true;
    
    // Update URL with current search parameters
    this.updateQueryParams();
    
    // Clear scroll position for new search
    if (this.mainContentElement) {
      this.mainContentElement.scrollTop = 0;
    }
    
    await this.loadResults(true);
  }

  toggleSortDirection(): void {
    this.descending = !this.descending;
    this.onSearch();
  }

  onSortChange(): void {
    this.onSearch();
  }

  onKeywordsChange(): void {
    this.onSearch();
  }

  async loadResults(isNewSearch: boolean = false): Promise<void> {
    if (this.isLoading || (!this.hasMoreResults && !isNewSearch)) {
      return;
    }

    try {
      this.isLoading = true;
      
      // Build search request matching SearchMediaRequest interface
      const searchRequest: SearchMediaRequest = {
        sort: this.sort,
        take: this.take,
        skip: this.skip
      };

      // Add optional parameters only if they have values
      if (this.keywords?.trim()) {
        searchRequest.keywords = this.keywords.trim();
      }
      if (this.cast.length > 0) {
        searchRequest.cast = [...this.cast];
      }
      if (this.genres.length > 0) {
        searchRequest.genres = [...this.genres];
      }
      if (this.descending) {
        searchRequest.descending = this.descending;
      }

      const response = await firstValueFrom(this.mediaService.search(searchRequest));
      
      if (isNewSearch) {
        this.results = [...response.results];
      } else {
        this.results = [...this.results, ...response.results];
      }

      // Check if we have more results
      this.hasMoreResults = response.results.length === this.take;
      
      // Increment for next load
      if (response.results.length > 0) {
        this.pageIndex++;
        this.skip = this.pageIndex * this.take;
      }
      
      this.cdr.detectChanges();
      
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

  private loadFromQueryParams(): void {
    const params = this.route.snapshot.queryParams;
    
    // Load search parameters from URL
    this.keywords = params['keywords'] || '';
    this.sort = params['sort'] || 'title';
    this.descending = params['descending'] === 'true';
    this.take = parseInt(params['take']) || 25;
    
    // Handle array parameters
    if (params['cast']) {
      this.cast = Array.isArray(params['cast']) ? params['cast'] : [params['cast']];
    } else {
      this.cast = [];
    }
    
    if (params['genres']) {
      this.genres = Array.isArray(params['genres']) ? params['genres'] : [params['genres']];
    } else {
      this.genres = [];
    }
  }

  private updateQueryParams(): void {
    const queryParams: any = {};
    
    // Only add parameters that have values to keep URL clean
    if (this.keywords?.trim()) {
      queryParams['keywords'] = this.keywords.trim();
    }
    if (this.cast.length > 0) {
      queryParams['cast'] = this.cast;
    }
    if (this.genres.length > 0) {
      queryParams['genres'] = this.genres;
    }
    if (this.sort !== 'title') {
      queryParams['sort'] = this.sort;
    }
    if (this.descending) {
      queryParams['descending'] = 'true';
    }
    if (this.take !== 25) {
      queryParams['take'] = this.take.toString();
    }

    // Update URL without triggering navigation
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'replace'
    });
  }

  private saveScrollPosition(scrollTop?: number): void {
    try {
      const position = scrollTop ?? this.getCurrentScrollPosition();
      const state: SearchState = {
        pageSize: this.take,
        pageIndex: this.pageIndex,
        scrollPosition: position
      };
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
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
    }
  }
}