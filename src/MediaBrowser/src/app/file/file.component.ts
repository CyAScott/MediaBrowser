import { Component, HostListener } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonControlsService, Page } from '../common-controls.service';
import { FileReadModel, FilesService, SearchFilesRequest } from '../files.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';

@Component({
  selector: 'app-file',
  templateUrl: './file.component.html',
  styleUrls: ['./file.component.less']
})
export class FileComponent extends Page {

  constructor(
    controls: CommonControlsService,
    logger : LoggerService,
    route : ActivatedRoute,
    private files : FilesService) {
    super(controls, logger, 'File')
    route.params.subscribe(params => this.id = params.id);
  }

  public backToPlaylist() : void {
    this.controls.refresh([ '/Playlist/' + this.files.lastSearchPlaylistId + '/Files' ]);
  }

  public cache() : void {
    if (!this.file) {
      return;
    }

    this.controls.modals?.toggleLoader();

    var obserable = this.file.cached ? this.files.uncache(this.file) : this.files.cache(this.file);

    obserable.subscribe({
      next: file => {
        if (this.file) {
          this.file = file;
        }
        this.controls.modals?.toggleLoader();
      },
      error: msg => {
        this.controls.modals?.toggleLoader();
        this.logger.error(msg);
        this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, `Failed to ${this.file?.cached ? 'uncache' : 'cache'} file.`);
      }
    });
  }

  public edit() : void {
    this.controls.refresh([ '/File/' + (this.file?.id || '') + '/Info' ]);
  }

  public endOfFile() : void {
    this.endOfFileReached = true;
    this.nextTimeout();
  }

  public endOfFileReached : boolean = false;

  public file : FileReadModel | undefined;

  public id : string = '';

  public infoVisible : boolean = false;
  
  public init() : void {
    if (this.controls.pagination?.pageCount) {
      this.controls.pagination.pageCount = 1;
    }
    this.files
      .get(this.id)
      .subscribe(file => {
        this.controls.modals?.toggleLoader();
        this.file = file;
        this.endOfFileReached = file.type === 'Photo';
        if (this.endOfFileReached) {
          this.nextTimeout();
        }
      });
  }

  @HostListener('window:keyup', ['$event'])
  public keyEvent(event: KeyboardEvent) : void {
    if (event.key === 'ArrowRight') {
      this.next();
    }
    else if (event.key === 'ArrowLeft') {
      this.previous();
    }
  }

  public next() : void {
    if (!this.files.lastSearch || !this.files.lastSearchResponse) {
      return;
    }

    let query = new SearchFilesRequest(this.files.lastSearch);

    let resultIndex = this.files.lastSearchResponse.results.findIndex(it => it.id === this.file?.id);

    query.skip += resultIndex + 1;

    if (query.skip === this.files.lastSearchResponse.count) {
      query.skip = 0;
    }

    query.take = 1;
    
    this.controls.modals?.toggleLoader();

    this.files.search(query, this.files.lastSearchPlaylistId).subscribe(response => {
      this.controls.modals?.toggleLoader();

      if (response.results && response.results.length) {
        this.controls.refresh([ '/File/' + response.results[0].id + '/View' ]);
      }
    });
  }

  public nextTimeout() : void {
    if (this.controls.autoPlay) {
      this.waitForNext = setTimeout(() => this.next(), 5000);
    }
  }

  public previous() : void {
    if (!this.files.lastSearch || !this.files.lastSearchResponse) {
      return;
    }

    let query = new SearchFilesRequest(this.files.lastSearch);

    let resultIndex = this.files.lastSearchResponse.results.findIndex(it => it.id === this.file?.id);

    query.skip += resultIndex - 1;

    if (query.skip < 0) {
      query.skip = this.files.lastSearchResponse.count - 1;
    }
    
    query.take = 1;
    
    this.controls.modals?.toggleLoader();

    this.files.search(query, this.files.lastSearchPlaylistId).subscribe(response => {
      this.controls.modals?.toggleLoader();

      if (response.results && response.results.length) {
        this.controls.refresh([ '/File/' + response.results[0].id + '/View' ]);
      }
    });
  }

  public toggleAutoplay() : void {
    if (this.controls.toggleAutoplay()) {
      if (this.endOfFileReached) {
        this.next();
      }
    } else if (this.waitForNext) {
      clearTimeout(this.waitForNext);
      this.waitForNext = undefined;
    }
  }

  public toggleFullScreen() : void {
    if (this.controls.appComponent) {
      this.infoVisible = false;
      this.controls.appComponent.toggleFullScreen();
    }
  }

  public unload(): void {
    clearTimeout(this.waitForNext);
    this.waitForNext = undefined;
  }

  public viewInfo() : void {
    this.infoVisible = !this.infoVisible;
  }

  public waitForNext : any;
}
