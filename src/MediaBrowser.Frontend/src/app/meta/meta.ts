import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MediaService } from '../services';
import { SearchComponent } from '../search/search';
import { firstValueFrom, Subscription } from 'rxjs';
import { SpinnerComponent } from '../spinner/spinner';

interface MetaMember {
  name: string;
  imageUrl: string;
  searchPath: string;
  searchParams: { [key: string]: string | string[] };
}

@Component({
  selector: 'app-meta',
  imports: [CommonModule, RouterModule, SpinnerComponent],
  templateUrl: './meta.html',
  styleUrls: ['./meta.css']
})
export class MetaComponent implements OnInit, AfterViewInit, OnDestroy {
  private cdr = inject(ChangeDetectorRef);
  private mediaService = inject(MediaService);
  private route = inject(ActivatedRoute);

  @ViewChild('metaGrid', { static: false }) metaGrid!: ElementRef<HTMLDivElement>;

  metaMembers: MetaMember[] = [];
  isLoading: boolean = false;
  type: string = '';
  private scrollPosition: number = 0;
  private readonly SCROLL_KEY = '-scroll-position';
  private scrollListener?: (event: Event) => void;
  private routeSubscription?: Subscription;

  async ngOnInit(): Promise<void> {
    this.routeSubscription = this.route.paramMap.subscribe(async (params) => {
      const newType = params.get('type')?.toLowerCase() ?? '';
      
      // Only reload if type has actually changed
      if (newType !== this.type) {
        this.type = newType;
        const savedScrollPosition = sessionStorage.getItem(this.type + this.SCROLL_KEY);
        if (savedScrollPosition) {
          this.scrollPosition = parseInt(savedScrollPosition, 10);
        } else {
          this.scrollPosition = 0;
        }
        await this.loadMetaInfo();
      }
    });
  }

  ngAfterViewInit(): void {
    // Create bound scroll listener for proper cleanup
    this.scrollListener = this.onScroll.bind(this);
    
    // Add scroll event listener to save scroll position
    if (this.metaGrid) {
      this.metaGrid.nativeElement.addEventListener('scroll', this.scrollListener);
      
      // Restore scroll position if data is already loaded
      if (this.scrollPosition > 0 && this.metaMembers.length > 0) {
        requestAnimationFrame(() => {
          this.metaGrid.nativeElement.scrollTop = this.scrollPosition;
        });
      }
    }
  }

  ngOnDestroy(): void {
    // Remove scroll event listener
    if (this.metaGrid && this.scrollListener) {
      this.metaGrid.nativeElement.removeEventListener('scroll', this.scrollListener);
    }
    
    // Unsubscribe from route parameter changes
    if (this.routeSubscription) {
      this.routeSubscription.unsubscribe();
    }
  }

  private onScroll(event: Event): void {
    const element = event.target as HTMLDivElement;
    this.scrollPosition = element.scrollTop;
    // Save scroll position to sessionStorage
    sessionStorage.setItem(this.type + this.SCROLL_KEY, this.scrollPosition.toString());
  }

  clearScrollPosition(): void {
    this.scrollPosition = 0;
    sessionStorage.removeItem(this.type + this.SCROLL_KEY);
    if (this.metaGrid) {
      this.metaGrid.nativeElement.scrollTop = 0;
    }
  }

  async loadMetaInfo(): Promise<void> {
    this.isLoading = true;
    
    try {

      let routePreFix = '';
      let results: string[] = [];

      switch (this.type) {
        case 'cast':
          results = await firstValueFrom(this.mediaService.getAllCast());
          routePreFix = 'cast';
          break;
        case 'directors':
          results = await firstValueFrom(this.mediaService.getAllDirectors());
          routePreFix = 'director';
          break;
        case 'genres':
          results = await firstValueFrom(this.mediaService.getAllGenres());
          routePreFix = 'genre';
          break;
        case 'producers':
          results = await firstValueFrom(this.mediaService.getAllProducers());
          routePreFix = 'producer';
          break;
        case 'writers':
          results = await firstValueFrom(this.mediaService.getAllWriters());
          routePreFix = 'writer';
          break;
      }

      this.metaMembers = results.map(name => ({
        name,
        imageUrl: `/api/media/${encodeURIComponent(routePreFix)}/${encodeURIComponent(name)}/thumbnail`,
        searchPath: '/search',
        searchParams: SearchComponent.createSearchQueryParams({ [this.type]: [name] }, 0)
      }));
      
      // Restore scroll position after content is loaded and rendered
      if (this.metaGrid && this.scrollPosition > 0) {
        // Use requestAnimationFrame to ensure DOM is fully updated
        requestAnimationFrame(() => {
          this.metaGrid.nativeElement.scrollTop = this.scrollPosition;
        });
      }
    } catch (error) {
      console.error('Load error:', error);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }
}