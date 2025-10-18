import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Navigation, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MediaReadModel, MediaService, UpdateMediaRequest } from '../services';
import { firstValueFrom } from 'rxjs';
import { ImportService } from '../services/import.service';
import { SpinnerComponent } from '../spinner/spinner';

@Component({
  selector: 'app-media-editor',
  imports: [CommonModule, FormsModule, SpinnerComponent],
  templateUrl: './media-editor.html',
  styleUrls: ['./media-editor.css']
})
export class MediaEditorComponent implements OnInit {
  private cdr = inject(ChangeDetectorRef);
  private importService = inject(ImportService);
  private location = inject(Location);
  private mediaService = inject(MediaService);
  private navigation: Navigation | null = null;
  private route = inject(ActivatedRoute);

  constructor(private router: Router) {
    this.navigation = this.router.currentNavigation();
  }
  
  filename: string | null = null;
  isCreatingThumbnail: boolean = false;
  isLoading: boolean = false;
  isSaving: boolean = false;
  mediaData: MediaReadModel | null = null;
  mediaId: string | null = null;
  thumbnail: number | null = null;
  thumbnailPreviewUrl: string = '';

  // Form fields for editable properties
  editableData: UpdateMediaRequest = {
    cast: [],
    directors: [],
    description: '',
    genres: [],
    originalTitle: '',
    producers: [],
    title: '',
    writers: [],
  };

  async ngOnInit(): Promise<void> {
    try {
      this.mediaId = this.route.snapshot.paramMap.get('id');
      if (this.mediaId) {
        await this.loadMediaById(this.mediaId);
      } else {
        await this.loadMediaByState();
      }
    } catch (error) {
      console.error('Error during initialization:', error);
    }
  }

  async loadMediaById(id: string): Promise<void> {
    console.log('Loading media by ID:', id);

    this.isLoading = true;

    this.filename = null;
    this.mediaData = await firstValueFrom(this.mediaService.get(id));
    this.setEditableData(this.mediaData);
    this.thumbnail = this.mediaData.thumbnail ?? 0;
    this.thumbnailPreviewUrl = this.mediaData.thumbnailUrl || '';

    this.isLoading = false;

    this.cdr.detectChanges();
  }

  async loadMediaByState(): Promise<void> {
    console.log('Loading media by state');

    const navigationState = this.navigation?.extras.state;

    if (!navigationState) {
      throw new Error('No navigation state found');
    }

    this.filename = navigationState['filename'];
    this.mediaData = navigationState?.['mediaData'];
    this.thumbnail = 0;
    this.thumbnailPreviewUrl = '';

    if (!this.mediaData || !this.filename) {
      throw new Error('No media data or filename found');
    }

    this.setEditableData(this.mediaData);
  }

  setEditableData(mediaData: MediaReadModel): void {
    this.editableData = {
      cast: [...mediaData.cast],
      description: mediaData.description,
      directors: [...mediaData.directors],
      genres: [...mediaData.genres],
      originalTitle: mediaData.originalTitle,
      producers: [...mediaData.producers],
      title: mediaData.title,
      userStarRating: mediaData.userStarRating,
      writers: [...mediaData.writers],
    };
  }

  async saveChanges(): Promise<void> {
    if (!this.mediaData) {
      return;
    }

    this.isSaving = true;
    try {
      if (this.filename) {
        await firstValueFrom(this.importService.import(this.filename, {
          ...this.editableData,
          thumbnail: this.thumbnail ?? 0
        }));
      } else if (this.mediaId) {
        await firstValueFrom(this.mediaService.update(this.mediaId, {
          ...this.editableData
        }));
      }
      this.location.back();
    } catch (error) {
      console.error('Error saving media changes:', error);
    } finally {
      this.isSaving = false;
      this.cdr.detectChanges();
    }
  }

  cancel(): void {
    this.location.back();
  }

  // Helper methods for formatted display
  formatDuration(seconds?: number): string {
    seconds ??= 0;

    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const remainingSeconds = (seconds % 60).toFixed(3);
    
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  formatDateTime(epochMs: string): string {
    const date = new Date(parseInt(epochMs));
    return date.toLocaleString();
  }

  formatFileSize(bytes?: number): string {
    const mb = (bytes ?? 0) / (1024 * 1024);
    return `${mb.toFixed(2)} MB`;
  }

  // Array manipulation methods
  addArrayItem(arrayName: keyof Pick<typeof this.editableData, 'cast' | 'directors' | 'producers' | 'writers'>): void {
    this.editableData[arrayName].push('');
  }

  removeArrayItem(arrayName: keyof Pick<typeof this.editableData, 'cast' | 'directors' | 'producers' | 'writers'>, index: number): void {
    this.editableData[arrayName].splice(index, 1);
  }

  trackByIndex(index: number): number {
    return index;
  }

  // Star rating methods
  setRating(rating: number): void {
    this.editableData.userStarRating = rating;
  }

  getStarClass(starNumber: number): string {
    return starNumber <= (this.editableData.userStarRating ?? 0) ? 'fa-solid fa-star' : 'fa-regular fa-star';
  }

  // Video player and thumbnail methods
  setThumbnailPreview(): void {
    const videoElement = document.querySelector('#video-player') as HTMLVideoElement;
    if (!videoElement) {
      console.error('Video player not found');
      return;
    }

    this.thumbnail = videoElement.currentTime;
    this.thumbnailPreviewUrl = this.generateThumbnailPreview(videoElement);
  }

  private generateThumbnailPreview(videoElement: HTMLVideoElement): string {
    try {
      // Create a canvas element to capture the video frame
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');
      
      if (!ctx) {
        console.error('Could not get canvas context');
        return '';
      }

      // Set canvas dimensions to match video
      canvas.width = videoElement.videoWidth;
      canvas.height = videoElement.videoHeight;

      // Draw the current video frame to the canvas
      ctx.drawImage(videoElement, 0, 0, canvas.width, canvas.height);

      // Convert canvas to base64 data URL
      return canvas.toDataURL('image/jpeg', 0.8);
    } catch (error) {
      console.error('Error generating thumbnail preview:', error);
      return '';
    }
  }

  async createThumbnailFromVideo(): Promise<void> {
    const videoElement = document.querySelector('#video-player') as HTMLVideoElement;
    if (!videoElement) {
      console.error('Video player not found');
      return;
    }

    this.isCreatingThumbnail = true;
    try {
      this.thumbnail = videoElement.currentTime;

      if (this.mediaId) {
        await firstValueFrom(this.mediaService.updateThumbnail(this.mediaId, { at: this.thumbnail }));
      }
    } catch (error) {
      console.error('Error creating thumbnail:', error);
    } finally {
      this.isCreatingThumbnail = false;
    }
    this.cdr.detectChanges();
  }

  getVideoUrl(): string {
    return this.mediaData?.url || '';
  }

  onVideoMetadataLoaded(event: Event): void {
    if (this.thumbnail !== null) {
      (event.target as HTMLVideoElement).currentTime = this.thumbnail;
    }
  }
}