import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Navigation, Router } from '@angular/router';
import { Location } from '@angular/common';
import { AddChapterRequest, MediaReadModel, MediaService, UpdateMediaRequest } from '../services';
import { firstValueFrom } from 'rxjs';
import { ImportService } from '../services/import.service';
import { SpinnerComponent } from '../spinner/spinner';
import { TitleSectionComponent, TitleData } from './title-section/title-section.component';
import { RatingSectionComponent } from './rating-section/rating-section.component';
import { PeopleSectionComponent, PeopleData } from './people-section/people-section.component';
import { ReadonlyInfoSectionComponent, MediaReadOnlyData } from './readonly-info-section/readonly-info-section.component';
import { ThumbnailSectionComponent, ThumbnailData, MediaThumbnailData } from './thumbnail-section/thumbnail-section.component';
import { ImportComponent } from '../import/import';
import { SearchComponent } from '../search/search';

export enum MediaEditorMode {
  Edit = 'Edit',
  Import = 'Import',
  AddChapter = 'AddChapter'
}

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
  
  get hasNavigationHistory(): boolean {
    return this.navigation?.previousNavigation != null;
  }
  get readWriteInProgress(): boolean {
    return this.isCreatingThumbnail || this.isLoading || this.isSaving;
  }

  chapterDuration?: number;
  chapterStart?: number;
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
  filename?: string;
  isCreatingThumbnail: boolean = false;
  isLoading: boolean = false;
  isSaving: boolean = false;
  mediaData?: MediaReadModel;
  mediaId?: string;
  mode: MediaEditorMode = MediaEditorMode.Edit;
  thumbnail?: ThumbnailData;

  // init component based on route params and load media data if needed
  async ngOnInit(): Promise<void> {
    try {
      this.isLoading = true;

      if (this.route.snapshot.paramMap.has('fileName')) {
        this.mode = MediaEditorMode.Import;

        // set route params
        this.chapterStart = undefined;
        this.chapterDuration = undefined;
        this.filename = this.route.snapshot.paramMap.get('fileName')!;
        this.mediaId = undefined;
      } else if (this.route.snapshot.paramMap.has('start') && this.route.snapshot.paramMap.has('end')) {
        this.mode = MediaEditorMode.AddChapter;
        
        // set route params
        this.chapterStart = Number(this.route.snapshot.paramMap.get('start'));
        this.chapterDuration = Number(this.route.snapshot.paramMap.get('end')) - this.chapterStart;
        this.filename = undefined;
        this.mediaId = this.route.snapshot.paramMap.get('id')!;
      } else {
        this.mode = MediaEditorMode.Edit;

        // set route params
        this.chapterStart = undefined;
        this.chapterDuration = undefined;
        this.filename = undefined;
        this.mediaId = this.route.snapshot.paramMap.get('id')!;
      }

      // read the media data based on the mode and route params
      await this.loadMedia();

      this.setEditableData();
      this.setThumbnail();
    } catch (error) {
      console.error('Error during initialization:', error);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }
  async loadMedia(): Promise<void> {
    this.mediaData = this.navigation?.extras.state?.['mediaData'];

    if (this.mode === MediaEditorMode.Import) {
      this.mediaData ??= ImportComponent.convertToMediaReadModel(await firstValueFrom(this.importService.readFileInfo(this.filename!)));
    } else if (this.mediaData?.id !== this.mediaId) {
      this.mediaData = await firstValueFrom(this.mediaService.get(this.mediaId!));
    }
  }
  setThumbnail(): void {
    if (this.mode === MediaEditorMode.AddChapter) {
      this.thumbnail = {
        selectedImageFile: null,
        thumbnail: this.chapterStart!,
        thumbnailPreviewUrl: ''
      };
    } else if (this.mode === MediaEditorMode.Edit) {
      this.thumbnail = {
        selectedImageFile: null,
        thumbnail: this.mediaData!.thumbnail!,
        thumbnailPreviewUrl: this.mediaData!.thumbnailUrl || ''
      };
    } else {
      this.thumbnail = {
        selectedImageFile: null,
        thumbnail: 0,
        thumbnailPreviewUrl: ''
      };
    }
    console.log('Initial thumbnail set:', this.thumbnail);
  }
  setEditableData(): void {
    this.editableData = {
      cast: [...this.mediaData!.cast],
      description: this.mediaData!.description,
      directors: [...this.mediaData!.directors],
      genres: [...this.mediaData!.genres],
      originalTitle: this.mediaData!.originalTitle,
      producers: [...this.mediaData!.producers],
      title: this.mediaData!.title,
      userStarRating: this.mediaData!.userStarRating,
      writers: [...this.mediaData!.writers],
    };
  }

  // save and cancel methods
  async saveChanges(): Promise<void> {
    if (!this.mediaData) {
      return;
    }

    this.isSaving = true;

    try {
      let createdChapter: MediaReadModel | undefined;

      if (this.mode === MediaEditorMode.AddChapter) {

        let thumbnailData: number | undefined;

        if (typeof this.thumbnail?.thumbnail === 'number') {
          thumbnailData = this.thumbnail.thumbnail;
        } else if (this.mediaData?.mime.startsWith('video/')) {
          thumbnailData = this.chapterStart!;
        }

        const chapterRequest: AddChapterRequest = {
          ...this.editableData,
          duration: this.chapterDuration!,
          start: this.chapterStart!,
          thumbnail: thumbnailData
        };

        createdChapter = await firstValueFrom(this.mediaService.addChapter(this.mediaId!, chapterRequest));
      } else if (this.mode === MediaEditorMode.Edit) {
        this.mediaData = await firstValueFrom(this.mediaService.update(this.mediaId!, {
          ...this.editableData
        }));
      } else if (this.mode === MediaEditorMode.Import) {

        let thumbnail = this.thumbnail?.thumbnail ?? undefined;
        if (!this.thumbnail?.selectedImageFile 
          && typeof thumbnail !== 'number'
          && this.mediaData.mime.startsWith('video/')) {
          thumbnail = 0;
        }

        const media = await firstValueFrom(this.importService.import(this.filename!, {
          ...this.editableData,
          thumbnail: thumbnail
        }));

        if (this.thumbnail?.selectedImageFile) {
          await firstValueFrom(this.mediaService.updateThumbnail(media.id, this.thumbnail.selectedImageFile));
        }
      }

      SearchComponent.clearCachedResults();
      PeopleSectionComponent.clearCacheIfStale(this.peopleData);

      if (this.mode === MediaEditorMode.AddChapter && createdChapter) {
        await this.router.navigate(['/player', createdChapter.id], {
          state: { mediaData: createdChapter }
        });
      } else {
        this.cancel();
      }
    } catch (error) {
      console.error('Error saving media changes:', error);
    } finally {
      this.isSaving = false;
      this.cdr.detectChanges();
    }
  }
  cancel(): void {
    if (this.hasNavigationHistory) {
      this.location.back();
    } else {
      this.router.navigate(['/search'], { queryParams: { sort: SearchComponent.DEFAULT_SORT } });
    }
  }

  // Component data getters
  get titleData(): TitleData {
    return {
      title: this.editableData.title,
      originalTitle: this.editableData.originalTitle,
      description: this.editableData.description
    };
  }
  get peopleData(): PeopleData {
    return {
      cast: this.editableData.cast,
      directors: this.editableData.directors,
      genres: this.editableData.genres,
      producers: this.editableData.producers,
      writers: this.editableData.writers
    };
  }
  get readOnlyData(): MediaReadOnlyData {
    return {
      ctimeMs: this.mediaData!.ctimeMs,
      duration: this.mediaData!.duration,
      height: this.mediaData!.height,
      id: this.mediaData!.id,
      md5: this.mediaData!.md5,
      mime: this.mediaData!.mime,
      mtimeMs: this.mediaData!.mtimeMs,
      parentId: this.mediaData!.parentId,
      published: this.mediaData!.published,
      size: this.mediaData!.size,
      start: this.mediaData!.start,
      width: this.mediaData!.width,
    };
  }
  get saveButtonText(): string {
    if (this.mode === MediaEditorMode.AddChapter) {
      return 'Add Chapter';
    }
    if (this.mode === MediaEditorMode.Import) {
      return 'Import File';
    }
    return 'Save Changes';
  }
  get thumbnailMediaData(): MediaThumbnailData {
    return {
      mime: this.mediaData!.mime,
      start: this.chapterStart ?? this.mediaData!.start,
      url: this.mediaData!.url
    };
  }
  get headerTitle(): string {
    if (this.mode === MediaEditorMode.AddChapter) {
      return 'Add Chapter';
    }
    if (this.mode === MediaEditorMode.Import) {
      return 'Import Media';
    }
    return 'Edit Media';
  }

  // Component event handlers
  onPeopleDataChange(peopleData: PeopleData): void {
    this.editableData.cast = peopleData.cast;
    this.editableData.directors = peopleData.directors;
    this.editableData.genres = peopleData.genres;
    this.editableData.producers = peopleData.producers;
    this.editableData.writers = peopleData.writers;
  }
  onRatingChange(rating: number): void {
    this.editableData.userStarRating = rating;
  }
  async onSaveThumbnail(): Promise<void> {
    if (!this.mediaId) {
      return;
    }

    try {
      this.isCreatingThumbnail = true;

      if (this.thumbnail?.selectedImageFile) {
        await firstValueFrom(this.mediaService.updateThumbnail(this.mediaId, this.thumbnail.selectedImageFile));
      } else if (this.thumbnail?.thumbnail !== null) {
        await firstValueFrom(this.mediaService.updateThumbnail(this.mediaId, { at: this.thumbnail!.thumbnail }));
      }
    } catch (error) {
      console.error('Error creating thumbnail:', error);
    } finally {
      this.isCreatingThumbnail = false;
      this.cdr.detectChanges();
    }
  }
  onSetThumbnailPreview(): void {
    // The thumbnail component handles the preview generation internally
    // This method can be used for any additional logic if needed
  }
  onThumbnailChange(thumbnail: ThumbnailData): void {
    this.thumbnail = thumbnail;
  }
  onTitleDataChange(titleData: TitleData): void {
    this.editableData.title = titleData.title;
    this.editableData.originalTitle = titleData.originalTitle;
    this.editableData.description = titleData.description;
  }
}