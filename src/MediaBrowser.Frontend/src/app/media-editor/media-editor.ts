import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Navigation, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MediaReadModel, MediaService, UpdateMediaRequest } from '../services';
import { firstValueFrom } from 'rxjs';
import { ImportService } from '../services/import.service';
import { SpinnerComponent } from '../spinner/spinner';
import { TitleSectionComponent, TitleData } from './title-section/title-section.component';
import { RatingSectionComponent } from './rating-section/rating-section.component';
import { PeopleSectionComponent, PeopleData } from './people-section/people-section.component';
import { ReadonlyInfoSectionComponent, MediaReadOnlyData } from './readonly-info-section/readonly-info-section.component';
import { ThumbnailSectionComponent, ThumbnailData, MediaThumbnailData } from './thumbnail-section/thumbnail-section.component';

@Component({
  selector: 'app-media-editor',
  imports: [
    CommonModule, 
    FormsModule, 
    SpinnerComponent,
    TitleSectionComponent,
    RatingSectionComponent,
    PeopleSectionComponent,
    ReadonlyInfoSectionComponent,
    ThumbnailSectionComponent
  ],
  templateUrl: './media-editor.html',
  styleUrl: './media-editor.css'
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
  selectedImageFile: File | null = null;
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
    const navigationState = this.navigation?.extras.state;

    if (!navigationState) {
      throw new Error('No navigation state found');
    }

    this.filename = navigationState['filename'];
    this.mediaData = navigationState?.['mediaData'];
    this.thumbnail = this.mediaData?.mime.startsWith('video/') ? 0 : null;
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

        let thumbnail = this.thumbnail ?? undefined;
        if (!this.thumbnail && !this.selectedImageFile && this.mediaData.mime.startsWith('video/')) {
          thumbnail = 0;
        }

        const media = await firstValueFrom(this.importService.import(this.filename, {
          ...this.editableData,
          thumbnail: thumbnail
        }));

        if (this.selectedImageFile) {
          await firstValueFrom(this.mediaService.updateThumbnail(media.id, this.selectedImageFile));
        }
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

  // Component data getters
  getTitleData(): TitleData {
    return {
      title: this.editableData.title,
      originalTitle: this.editableData.originalTitle,
      description: this.editableData.description
    };
  }

  getPeopleData(): PeopleData {
    return {
      cast: this.editableData.cast,
      directors: this.editableData.directors,
      genres: this.editableData.genres,
      producers: this.editableData.producers,
      writers: this.editableData.writers
    };
  }

  getReadOnlyData(): MediaReadOnlyData {
    return {
      id: this.mediaData!.id,
      duration: this.mediaData!.duration,
      size: this.mediaData!.size,
      md5: this.mediaData!.md5,
      ctimeMs: this.mediaData!.ctimeMs,
      mtimeMs: this.mediaData!.mtimeMs,
      width: this.mediaData!.width,
      height: this.mediaData!.height,
      mime: this.mediaData!.mime,
      rating: this.mediaData!.rating,
      published: this.mediaData!.published
    };
  }

  getThumbnailData(): ThumbnailData {
    return {
      thumbnail: this.thumbnail,
      thumbnailPreviewUrl: this.thumbnailPreviewUrl,
      selectedImageFile: this.selectedImageFile
    };
  }

  getThumbnailMediaData(): MediaThumbnailData {
    return {
      mime: this.mediaData!.mime,
      url: this.mediaData!.url
    };
  }

  // Component event handlers
  onTitleDataChange(titleData: TitleData): void {
    this.editableData.title = titleData.title;
    this.editableData.originalTitle = titleData.originalTitle;
    this.editableData.description = titleData.description;
  }

  onRatingChange(rating: number): void {
    this.editableData.userStarRating = rating;
  }

  onPeopleDataChange(peopleData: PeopleData): void {
    this.editableData.cast = peopleData.cast;
    this.editableData.directors = peopleData.directors;
    this.editableData.genres = peopleData.genres;
    this.editableData.producers = peopleData.producers;
    this.editableData.writers = peopleData.writers;
  }

  onThumbnailDataChange(thumbnailData: ThumbnailData): void {
    this.thumbnail = thumbnailData.thumbnail;
    this.thumbnailPreviewUrl = thumbnailData.thumbnailPreviewUrl;
    this.selectedImageFile = thumbnailData.selectedImageFile;
  }

  onSetThumbnailPreview(): void {
    // The thumbnail component handles the preview generation internally
    // This method can be used for any additional logic if needed
  }

  async onSaveThumbnail(): Promise<void> {
    if (!this.mediaId) {
      return;
    }

    try {
      this.isCreatingThumbnail = true;

      if (this.selectedImageFile) {
        await firstValueFrom(this.mediaService.updateThumbnail(this.mediaId, this.selectedImageFile));
      } else if (this.thumbnail !== null) {
        await firstValueFrom(this.mediaService.updateThumbnail(this.mediaId, { at: this.thumbnail }));
      }
    } catch (error) {
      console.error('Error creating thumbnail:', error);
    } finally {
      this.isCreatingThumbnail = false;
      this.cdr.detectChanges();
    }
  }
}