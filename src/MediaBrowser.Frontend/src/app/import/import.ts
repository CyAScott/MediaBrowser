import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ImportFileInfo, ImportService } from '../services/import.service';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { MediaReadModel } from '../services';

@Component({
  selector: 'app-import',
  imports: [CommonModule],
  templateUrl: './import.html',
  styleUrls: ['./import.css']
})
export class ImportComponent implements OnInit {
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);
  private importService = inject(ImportService);

  files: ImportFileInfo[] = [];

  ngOnInit(): Promise<void> {
    return this.scanDirectory();
  }

  private async scanDirectory(): Promise<void> {
    try {
      this.files = await firstValueFrom(this.importService.files());
    } catch (error) {
      console.error('Error scanning directory:', error);
      this.files = [];
    } finally {
      this.cdr.detectChanges();
    }
  }

  async importFile(file: ImportFileInfo): Promise<void> {
    try {

      /* Create a temporary MediaReadModel for the file to pass to the editor 
       * The actual import will populate the non-editable fields like id, createdOn, etc.
       */
      const mediaData: MediaReadModel = {
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

      const state = { 
        mediaData,
        filename: file.name
      };

      this.router.navigate(['/edit'], { state });
    } catch (error) {
      console.error('Error importing file:', error);
    }
  }
}