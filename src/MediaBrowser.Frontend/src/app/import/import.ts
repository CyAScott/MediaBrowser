import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ImportFileInfo, ImportService } from '../services/import.service';
import { firstValueFrom } from 'rxjs';
import { MediaReadModel } from '../services';

@Component({
  selector: 'app-import',
  imports: [CommonModule, RouterModule],
  templateUrl: './import.html',
  styleUrls: ['./import.css']
})
export class ImportComponent implements OnInit {
  private cdr = inject(ChangeDetectorRef);
  private importService = inject(ImportService);
  private readonly allowedExtensions = new Set([
    '.aac',
    '.adts',
    '.aif',
    '.aifc',
    '.aiff',
    '.bmp',
    '.cdda',
    '.f4v',
    '.flac',
    '.gif',
    '.jpg',
    '.jpge',
    '.m1v',
    '.m2v',
    '.mod',
    '.mov',
    '.mp2',
    '.mp2v',
    '.mp3',
    '.mp4',
    '.mp4v',
    '.mpa',
    '.mpe',
    '.mpeg',
    '.mpg',
    '.mpv2',
    '.mqv',
    '.oga',
    '.ogg',
    '.ogv',
    '.opus',
    '.png',
    '.qt',
    '.spx',
    '.tif',
    '.tiff',
    '.vbk',
    '.wav',
    '.wave',
    '.webm',
    '.webp'
  ]);

  files: MediaReadModel[] = [];
  isDragOver = false;
  isUploading = false;
  uploadError: string | null = null;

  static convertToMediaReadModel(file: ImportFileInfo): MediaReadModel {
    return {
      cast: [],
      createdOn: file.createdOn,
      ctimeMs: file.ctimeMs.toString(),
      description: '',
      directors: [],
      ffprobe: {},
      genres: [],
      id: '',
      md5: '',
      mime: file.mime,
      mtimeMs: file.mtimeMs.toString(),
      originalTitle: file.name,
      path: '',
      producers: [],
      published: '',
      rating: 0,
      title: file.name,
      updatedOn: file.updatedOn,
      url: file.url,
      userStarRating: 0,
      writers: [],
    };
  }

  private async scanDirectory(): Promise<void> {
    try {
      const files = await firstValueFrom(this.importService.files());
      this.files = files.map(ImportComponent.convertToMediaReadModel);
    } catch (error) {
      console.error('Error scanning directory:', error);
      this.files = [];
    } finally {
      this.cdr.detectChanges();
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  async onDrop(event: DragEvent): Promise<void> {
    event.preventDefault();
    this.isDragOver = false;
    const file = event.dataTransfer?.files.item(0);
    if (!file) {
      return;
    }

    await this.uploadMediaFile(file);
  }

  async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.item(0);
    input.value = '';

    if (!file) {
      return;
    }

    await this.uploadMediaFile(file);
  }

  private async uploadMediaFile(file: File): Promise<void> {
    this.uploadError = null;

    if (!this.isSupportedMediaFile(file)) {
      this.uploadError = 'Only audio and video files are allowed.';
      this.cdr.detectChanges();
      return;
    }

    this.isUploading = true;

    try {
      await firstValueFrom(this.importService.uploadFile(file));
      await this.scanDirectory();
    } catch (error) {
      console.error('Error uploading file:', error);
      this.uploadError = 'Failed to upload file. Please try again.';
      this.cdr.detectChanges();
    } finally {
      this.isUploading = false;
      this.cdr.detectChanges();
    }
  }

  private isSupportedMediaFile(file: File): boolean {
    if (file.type.startsWith('video/') || file.type.startsWith('audio/')) {
      return true;
    }

    const lastDot = file.name.lastIndexOf('.');
    if (lastDot < 0) {
      return false;
    }

    const extension = file.name.slice(lastDot).toLowerCase();
    return this.allowedExtensions.has(extension);
  }

  ngOnInit(): Promise<void> {
    return this.scanDirectory();
  }
}