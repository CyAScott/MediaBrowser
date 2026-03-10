import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ImportFileInfo, ImportService } from '../services/import.service';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
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

  files: MediaReadModel[] = [];

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

  ngOnInit(): Promise<void> {
    return this.scanDirectory();
  }
}