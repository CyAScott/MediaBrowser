import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MediaService, MediaTagType } from '../services';
import { SearchComponent } from '../search/search';
import { firstValueFrom, Subscription } from 'rxjs';
import { SpinnerComponent } from '../spinner/spinner';

interface MetaMember {
  name: string;
  imageUrl: string;
  queryParams: { [key: string]: string | string[] };
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
  private router = inject(Router);

  @ViewChild('metaGrid', { static: false }) metaGrid!: ElementRef<HTMLDivElement>;

  metaMembers: MetaMember[] = [];
  isLoading: boolean = false;
  type: string = '';
  uploadingMembers = new Set<string>();
  private scrollPosition: number = 0;
  private readonly SCROLL_KEY = '-scroll-position';
  private scrollListener?: (event: Event) => void;
  private routeSubscription?: Subscription;
  private readonly routePrefixMap: Record<MediaTagType, string> = {
    cast: 'cast',
    directors: 'director',
    genres: 'genre',
    producers: 'producer',
    writers: 'writer'
  };

  private isSupportedTagType(tagType: string): tagType is MediaTagType {
    return tagType in this.routePrefixMap;
  }

  private getImageUrl(tagType: MediaTagType, name: string, cacheBust?: number): string {
    const baseUrl = `/api/media/${this.routePrefixMap[tagType]}/${encodeURIComponent(name)}/thumbnail`;
    return cacheBust ? `${baseUrl}?t=${cacheBust}` : baseUrl;
  }

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

      let results: string[] = [];

      if (this.isSupportedTagType(this.type)) {
        const tagType = this.type as MediaTagType;
        results = await firstValueFrom(this.mediaService.getAllTags(tagType));
      }

      this.metaMembers = results.map(name => ({
        name,
        imageUrl: this.getImageUrl(this.type as MediaTagType, name),
        queryParams: { [this.type]: [name], sort: SearchComponent.DEFAULT_SORT }
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

  clearPagePositionState(): void {
    SearchComponent.clearPagePositionState();
  }

  isUploading(name: string): boolean {
    return this.uploadingMembers.has(name);
  }

  openThumbnailUpload(event: MouseEvent, input: HTMLInputElement): void {
    event.preventDefault();
    event.stopPropagation();
    input.click();
  }

  async onThumbnailSelected(event: Event, metaMember: MetaMember): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file || !this.isSupportedTagType(this.type)) {
      return;
    }

    const tagType = this.type;
    this.uploadingMembers.add(metaMember.name);

    try {
      await firstValueFrom(this.mediaService.setThumbnailForTag(tagType, metaMember.name, file));
      metaMember.imageUrl = this.getImageUrl(tagType, metaMember.name, Date.now());
    } catch (error) {
      console.error('Thumbnail upload error:', error);
    } finally {
      this.uploadingMembers.delete(metaMember.name);
      input.value = '';
      this.cdr.detectChanges();
    }
  }
}