import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Navigation, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MediaReadModel, MediaService } from '../services';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { ReadonlyInfoSectionComponent } from '../media-editor/readonly-info-section/readonly-info-section.component';

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
  
  get hasHistory(): boolean {
    return window.history.length > 1;
  }
  get mediaSourceUrl(): string {
    if (!this.mediaData) {
      return '';
    }

    if (this.mediaData.start == null || this.mediaData.duration == null) {
      return this.mediaData.url;
    }

    const end = this.mediaData.start + this.mediaData.duration;
    return `${this.mediaData.url}#t=${this.mediaData.start},${end}`;
  }

  chapterEnd?: number;
  chapterPanelVisible: boolean = false;
  chapterStart?: number;
  headerVisible: boolean = true;
  hideTimeout?: number;
  isLoading: boolean = true;
  mediaData?: MediaReadModel;
  mediaId?: string;

  async loadMedia(): Promise<void> {
    try { 
      let mediaData = this.navigation?.extras.state?.['mediaData'];
      if (!mediaData || mediaData.id !== this.mediaId) {
        mediaData = await firstValueFrom(this.mediaService.get(this.mediaId!));
      }
      this.mediaData = mediaData;
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

  // navigation
  editMedia(): void {
    if (this.mediaId) {
      this.router.navigate(['/edit', this.mediaId],
        { state: { mediaData: this.mediaData } }
      );
    }
  }
  goBack(): void {
    this.location.back();
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
    const ffprobeDuration = this.mediaData?.ffprobe?.format?.duration;
    if (ffprobeDuration) {
      return parseFloat(ffprobeDuration);
    }
    const mediaElement = this.videoElement?.nativeElement ?? this.audioElement?.nativeElement;
    return mediaElement?.duration ?? this.mediaData?.duration;
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
    if (this.mediaData && this.canAddChapter) {
      const start = this.chapterStart!;
      const end = this.chapterEnd!;

      this.router.navigate(['/edit', this.mediaData.parentId ?? this.mediaData.id, start, end, 'chapter'], {
        state: {
          mediaData: this.mediaData.parentId ? undefined : this.mediaData
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

    if (typeof this.mediaData?.start === 'number') {
      // set the start to end of the current chapter so the user can easily add consecutive chapters
      this.chapterStart = this.mediaData.start! + this.mediaData.duration!;
    } else {
      // else set the start to the current position in the media so the user can easily create a chapter starting from the current position
      this.chapterStart = mediaElement?.currentTime ?? 0;
    }
  }
  toggleChapterPanel(): void {
    const duration = this.mediaDuration;
    if (this.mediaData && duration !== undefined && duration > 0) {      
      this.chapterPanelVisible = !this.chapterPanelVisible;
      this.setupChapterRange();
    }
  }

  // helper methods
  formatTimestamp(seconds?: number): string {
    return ReadonlyInfoSectionComponent.formatDuration(seconds ?? 0);
  }
}