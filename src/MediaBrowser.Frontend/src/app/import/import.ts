import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ImportService } from '../services/import.service';
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

  directoryPath: string | null = null;
  files: string[] = [];

  ngOnInit(): Promise<void> {
    return this.scanDirectory();
  }

  private async scanDirectory(): Promise<void> {
    try {
      this.files = await firstValueFrom(this.importService.files());
      this.cdr.detectChanges();
    } catch (error) {
      console.error('Error scanning directory:', error);
      this.files = [];
      this.cdr.detectChanges();
    }
  }

  async importFile(filename: string): Promise<void> {
    try {

      /* Create a temporary MediaReadModel for the file to pass to the editor 
       * The actual import will populate the non-editable fields like id, createdOn, etc.
       */
      const mediaData: MediaReadModel = {
        cast: [],
        createdOn: new Date(),
        ctimeMs: '',
        description: '',
        directors: [],
        ffprobe: {},
        genres: [],
        id: '',
        md5: '',
        mime: '',
        mtimeMs: '',
        originalTitle: filename,
        path: '',
        producers: [],
        published: '',
        rating: 0,
        title: filename,
        updatedOn: new Date(),
        url: `/api/import/file/${encodeURIComponent(filename)}`,
        userStarRating: 0,
        writers: [],
      };

      const state = { 
        mediaData,
        filename
      };

      this.router.navigate(['/edit'], { state });
    } catch (error) {
      console.error('Error importing file:', error);
    }
  }
}