import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit, HostListener } from '@angular/core';
import { ActivatedRoute, Navigation, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MediaReadModel, MediaService, SearchResponse } from '../services';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { ReadonlyInfoSectionComponent } from '../media-editor/readonly-info-section/readonly-info-section.component';
import { SearchQueryParams } from '../search/search-query-params';

export interface PlayerNavigationState {
  mediaData?: MediaReadModel;
  searchContext?: {
    currentIndex: number;
    searchParams: SearchQueryParams;
  };
}

@Component({
  selector: 'app-player',
  imports: [CommonModule],
  templateUrl: './player.html',
  styleUrls: ['./player.css']
})
export class PlayerComponent implements OnInit, OnDestroy, AfterViewInit {
  private cdr = inject(ChangeDetectorRef);
  private location = inject(Location);
  private mediaService = inject(MediaService);
  private navigation: Navigation | null = null;
  private route = inject(ActivatedRoute);

  constructor(private router: Router) {
    this.navigation = this.router.currentNavigation();
  }
  
  @ViewChild('videoElement', { static: false }) videoElement!: ElementRef<HTMLVideoElement>;
  @ViewChild('audioElement', { static: false }) audioElement!: ElementRef<HTMLAudioElement>;
  @HostListener('document:keydown', ['$event'])
  onDocumentKeyDown(event: KeyboardEvent): void {
    if (!this.state?.mediaData?.mime.startsWith('image/') || !this.state.searchContext || this.isInteractiveTarget(event.target)) {
      return;
    }

    if (event.key === 'ArrowLeft' && this.hasPreviousItem) {
      event.preventDefault();
      void this.goToPrevious();
    }

    if (event.key === 'ArrowRight' && this.hasNextItem) {
      event.preventDefault();
      void this.goToNext();
    }
  }
  
  get mediaSourceUrl(): string {
    if (!this.state?.mediaData) {
      return '';
    }

    if (this.state.mediaData.start == null || this.state.mediaData.duration == null) {
      return this.state.mediaData.url;
    }

    const end = this.state.mediaData.start + this.state.mediaData.duration;
    return `${this.state.mediaData.url}#t=${this.state.mediaData.start},${end}`;
  }

  chapterEnd?: number;
  chapterPanelVisible: boolean = false;
  chapterStart?: number;
  headerVisible: boolean = true;
  hideTimeout?: number;
  isLoading: boolean = true;
  isSeekingMedia: boolean = false;
  lastPlaybackTime?: number;
  mediaId?: string;
  reachedSegmentEnd: boolean = false;
  searchResponse?: SearchResponse;
  state?: PlayerNavigationState;

  async loadMedia(): Promise<void> {
    try { 
      this.state = this.navigation?.extras.state as PlayerNavigationState | undefined;
      if (!this.state?.mediaData || this.state.mediaData.id !== this.mediaId) {
        this.state = { ...this.state, mediaData: await firstValueFrom(this.mediaService.get(this.mediaId!)) };
      }

      if (this.state.searchContext) {
        const params = this.route.snapshot.queryParams;
        SearchQueryParams.loadFromQueryParams(this.state.searchContext.searchParams, params);
        const skip = Math.max(this.state.searchContext.currentIndex - 1, 0);
        const searchRequest = SearchQueryParams.getSearchMediaRequest(this.state.searchContext.searchParams, skip, 3);
        this.searchResponse = await firstValueFrom(this.mediaService.search(searchRequest));
      }

      this.cdr.detectChanges();
    } catch (error) {
      console.error('Error loading media by ID:', error);
    }
  }
  ngAfterViewInit(): void {
    this.applySavedVolume();
  }
  ngOnDestroy(): void {
    this.clearHideTimer();
  }
  async ngOnInit(): Promise<void> {
    this.isLoading = true;
    this.resetPlaybackProgressTracking();
    this.mediaId = this.route.snapshot.paramMap.get('id')!;
    await this.loadMedia();
    this.startHideTimer();
  }
  onMediaMetadataLoaded(): void {
    this.isLoading = false;
    this.applySavedVolume();
  }

  // header visibility management
  readonly HEADER_HIDE_TIMEOUT = 5000; // Hide after 5 seconds
  clearHideTimer(): void {
    if (this.hideTimeout !== undefined) {
      window.clearTimeout(this.hideTimeout);
      this.hideTimeout = undefined;
    }
  }
  onMouseLeave(): void {
    this.startHideTimer();
  }
  onMouseMove(): void {
    if (!this.headerVisible) {
      this.headerVisible = true;
      this.cdr.detectChanges();
    }
    this.startHideTimer();
  }
  startHideTimer(): void {
    this.clearHideTimer();
    this.hideTimeout = window.setTimeout(() => {
      this.headerVisible = false;
      this.cdr.detectChanges();
    }, this.HEADER_HIDE_TIMEOUT);
  }

  // volume management
  readonly VOLUME_STORAGE_KEY = 'mediaBrowser_volume';
  get savedVolume(): number {
    const savedVolume = localStorage.getItem(this.VOLUME_STORAGE_KEY);
    return savedVolume ? parseFloat(savedVolume) : 1.0;
  }
  set savedVolume(volume: number) {
    localStorage.setItem(this.VOLUME_STORAGE_KEY, volume.toString());
  }
  applySavedVolume(): void {
    const savedVolume = this.savedVolume;
    
    // Apply to video element if it exists
    if (this.videoElement?.nativeElement) {
      this.videoElement.nativeElement.volume = savedVolume;
    }
    
    // Apply to audio element if it exists
    if (this.audioElement?.nativeElement) {
      this.audioElement.nativeElement.volume = savedVolume;
    }
  }
  onVolumeChange(event: Event): void {
    const element = event.target as HTMLVideoElement | HTMLAudioElement;
    this.savedVolume = element.volume;
  }

  // media progress tracking
  readonly SEGMENT_END_TOLERANCE = 0.2;
  readonly MAX_NATURAL_PROGRESS_STEP = 1.5;
  get segmentEndTime(): number | undefined {
    if (this.state?.mediaData?.start === undefined || this.state.mediaData.duration === undefined) {
      return undefined;
    }

    return this.state.mediaData.start + this.state.mediaData.duration;
  }
  onMediaSeeked(): void {
    this.isSeekingMedia = false;
  }
  onMediaSeeking(): void {
    this.isSeekingMedia = true;
    this.lastPlaybackTime = undefined;
  }
  async onMediaTimeUpdate(event: Event): Promise<void> {
    const segmentEnd = this.segmentEndTime;
    if (segmentEnd === undefined || this.reachedSegmentEnd || this.isSeekingMedia) {
      return;
    }

    const element = event.target as HTMLVideoElement | HTMLAudioElement;
    const previousTime = this.lastPlaybackTime;
    const currentTime = element.currentTime;
    this.lastPlaybackTime = currentTime;

    if (previousTime === undefined) {
      return;
    }

    if (element.paused || element.playbackRate <= 0) {
      return;
    }

    const maxStep = this.MAX_NATURAL_PROGRESS_STEP * Math.max(element.playbackRate, 1);
    const crossedSegmentEndNaturally =
      previousTime < segmentEnd - this.SEGMENT_END_TOLERANCE &&
      currentTime >= segmentEnd - this.SEGMENT_END_TOLERANCE &&
      currentTime - previousTime <= maxStep;

    if (crossedSegmentEndNaturally) {
      this.reachedSegmentEnd = true;
      await this.onMediaEnded();
    }
  }
  resetPlaybackProgressTracking(): void {
    this.isSeekingMedia = false;
    this.lastPlaybackTime = undefined;
    this.reachedSegmentEnd = false;
  }

  // navigation
  get hasHistory(): boolean {
    return window.history.length > 1;
  }
  get hasNextItem(): boolean {
    const currentIndex = this.currentSearchWindowIndex;
    return currentIndex !== undefined && currentIndex < this.searchResponse!.results.length - 1;
  }
  get hasPreviousItem(): boolean {
    const currentIndex = this.currentSearchWindowIndex;
    return currentIndex !== undefined && currentIndex > 0;
  }
  get currentSearchWindowIndex(): number | undefined {
    const results = this.searchResponse?.results;
    const searchContext = this.state?.searchContext;
    if (!results || !searchContext) {
      return undefined;
    }

    const currentMediaId = this.state?.mediaData?.id;
    if (currentMediaId) {
      const indexById = results.findIndex(media => media.id === currentMediaId);
      if (indexById >= 0) {
        return indexById;
      }
    }

    const windowStartIndex = Math.max(searchContext.currentIndex - 1, 0);
    const indexBySearchContext = searchContext.currentIndex - windowStartIndex;
    return indexBySearchContext >= 0 && indexBySearchContext < results.length
      ? indexBySearchContext
      : undefined;
  }
  editMedia(): void {
    if (this.mediaId) {
      this.router.navigate(['/edit', this.mediaId],
        { state: { mediaData: this.state?.mediaData } }
      );
    }
  }
  goBack(): void {
    this.location.back();
  }
  async goToNext(): Promise<void> {
    if (!this.hasNextItem) {
      return;
    }

    const currentIndex = this.currentSearchWindowIndex!;
    this.state!.searchContext!.currentIndex++;
    const mediaData = this.searchResponse!.results[currentIndex + 1];
    await this.router.navigate(['/player', mediaData.id], {
      state: { mediaData, searchContext: this.state?.searchContext },
      queryParams: this.state?.searchContext?.searchParams ? SearchQueryParams.getQueryParams(this.state.searchContext.searchParams) : undefined
    });
    await this.ngOnInit();
  }
  async goToPrevious(): Promise<void> {
    if (!this.hasPreviousItem) {
      return;
    }

    const currentIndex = this.currentSearchWindowIndex!;
    this.state!.searchContext!.currentIndex--;
    const mediaData = this.searchResponse!.results[currentIndex - 1];
    await this.router.navigate(['/player', mediaData.id], {
      state: { mediaData, searchContext: this.state?.searchContext },
      queryParams: this.state?.searchContext?.searchParams ? SearchQueryParams.getQueryParams(this.state.searchContext.searchParams) : undefined
    });
    await this.ngOnInit();
  }
  async onMediaEnded(): Promise<void> {
    this.reachedSegmentEnd = true;
    await this.goToNext();
  }

  // chapter management
  readonly RANGE_STEP: number = 1; // 1 second step for chapter range selection
  get canAddChapter(): boolean {
    const start = this.chapterStart;
    const end = this.chapterEnd;
    return end !== undefined && start !== undefined && end - start >= this.RANGE_STEP;
  }
  get endPercent(): number {
    const duration = this.mediaDuration;
    const end = this.chapterEnd;
    return end === undefined || duration === undefined || duration <= 0 ? 100 : Math.min(100, (end / duration) * 100);
  }
  get mediaDuration(): number | undefined {
    const ffprobeDuration = this.state?.mediaData?.ffprobe?.format?.duration;
    if (ffprobeDuration) {
      return parseFloat(ffprobeDuration);
    }
    const mediaElement = this.videoElement?.nativeElement ?? this.audioElement?.nativeElement;
    return mediaElement?.duration ?? this.state?.mediaData?.duration;
  }
  get selectedRangeWidth(): number {
    const duration = this.mediaDuration;
    const start = this.chapterStart;
    const end = this.chapterEnd;
    return start === undefined || end === undefined || duration === undefined || duration <= 0
      ? 0 : Math.min(100, ((end - start) / duration) * 100);
  }
  get startPercent(): number {
    const duration = this.mediaDuration;
    const start = this.chapterStart;
    return start === undefined || duration === undefined || duration <= 0 ? 0 : Math.min(100, (start / duration) * 100);
  }
  async goToAddChapter(): Promise<void> {
    if (this.state?.mediaData && this.canAddChapter) {
      const start = this.chapterStart!;
      const end = this.chapterEnd!;

      this.router.navigate(['/edit', this.state.mediaData.parentId ?? this.state.mediaData.id, start, end, 'chapter'], {
        state: {
          mediaData: this.state.mediaData.parentId ? undefined : this.state.mediaData
        }
      });
    }
  }
  onRangeInput(type: 'start' | 'end', event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);

    if (type === 'start') {
      this.chapterStart = Math.min(value, this.chapterEnd ?? 0);
      this.seekTo(this.chapterStart);
    } else {
      this.chapterEnd = Math.max(value, this.chapterStart ?? 0);
      this.seekTo(this.chapterEnd);
    }
  }
  seekTo(position: number): void {
    const mediaElement = this.videoElement?.nativeElement ?? this.audioElement?.nativeElement;
    if (mediaElement && !Number.isNaN(position)) {
      mediaElement.currentTime = Math.max(position, 0);
    }
  }
  setupChapterRange(): void {
    if (!this.chapterPanelVisible) {
      this.chapterEnd = undefined;
      this.chapterStart = undefined;
      return;
    }

    const mediaElement = this.videoElement?.nativeElement ?? this.audioElement?.nativeElement;
    mediaElement!.pause();

    this.chapterEnd = this.mediaDuration!;

    if (typeof this.state?.mediaData?.start === 'number') {
      // set the start to end of the current chapter so the user can easily add consecutive chapters
      this.chapterStart = this.state.mediaData.start! + this.state.mediaData.duration!;
    } else {
      // else set the start to the current position in the media so the user can easily create a chapter starting from the current position
      this.chapterStart = mediaElement?.currentTime ?? 0;
    }
  }
  toggleChapterPanel(): void {
    const duration = this.mediaDuration;
    if (this.state?.mediaData && duration !== undefined && duration > 0) {      
      this.chapterPanelVisible = !this.chapterPanelVisible;
      this.setupChapterRange();
    }
  }

  // helper methods
  isInteractiveTarget(target: EventTarget | null): boolean {
    const element = target as HTMLElement | null;
    if (!element) {
      return false;
    }

    const tagName = element.tagName;
    return ['INPUT', 'TEXTAREA', 'SELECT', 'BUTTON'].includes(tagName) || element.isContentEditable;
  }
  formatTimestamp(seconds?: number): string {
    return ReadonlyInfoSectionComponent.formatDuration(seconds ?? 0);
  }
}