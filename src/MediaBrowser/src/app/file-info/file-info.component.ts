import { ChangeDetectorRef, Component, ElementRef, ViewChild } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { CommonControlsService, Page } from '../common-controls.service';
import { FileReadModel, FilesService } from '../files.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';
import { UsersService } from '../users.service';

@Component({
  selector: 'app-file-info',
  templateUrl: './file-info.component.html',
  styleUrls: ['./file-info.component.less']
})
export class FileInfoComponent extends Page {

  constructor(
    controls: CommonControlsService,
    logger : LoggerService,
    route : ActivatedRoute,
    private files : FilesService,
    private ref: ChangeDetectorRef,
    private sanitizer : DomSanitizer,
    public users : UsersService) {
    super(controls, logger, 'File')
    route.params.subscribe(params => this.id = params.id);
  }

  public addThumbnail(input : any) : void {
    this.thumbnailIdCounts++;

    let thumbnail = new Thumbnail();

    thumbnail.id = this.thumbnailIdCounts + '';
    thumbnail.file = input.files[0];
    thumbnail.name = input.files[0].name;
    thumbnail.src = URL.createObjectURL(input.files[0]);

    this.thumbnails.push(thumbnail);
  }

  public description : string = '';
  
  public file : FileReadModel | undefined;

  public id : string = '';

  public init() : void {
    if (this.controls.pagination?.pageCount) {
      this.controls.pagination.pageCount = 1;
    }
    this.files
      .get(this.id)
      .subscribe(file => {
        this.controls.modals?.toggleLoader();
        this.file = file;

        this.description = file.description;
        this.name = file.name;
        this.type = file.type;
        this.readRoles = file.readRoles;
        this.src = file.url;
        this.updateRoles = file.updateRoles;
        
        if (this.video?.nativeElement) {
          this.video.nativeElement.src = this.type == 'Video' ? this.src : undefined;
        }

        if (file.thumbnails) {
          this.thumbnails = file.thumbnails.map(it => {
            let thumbnail = new Thumbnail();

            thumbnail.file = undefined;
            thumbnail.id = it.md5;
            thumbnail.name = '';
            thumbnail.src = it.url;

            return thumbnail;
          });
        }
      });
  }

  public makeThumbnail(video : any) : void {

    try {

      let canvas = document.createElement('canvas');
      canvas.height = video.videoHeight;
      canvas.width = video.videoWidth;
  
      let ctx = canvas.getContext('2d');
      ctx?.drawImage(video, 0, 0, canvas.width, canvas.height);
  
      canvas.toBlob(blob => {

        this.thumbnailIdCounts++;

        let thumbnail = new Thumbnail();
  
        thumbnail.id = this.thumbnailIdCounts + '';
        thumbnail.file = blob;
        thumbnail.name = '';
        thumbnail.src = URL.createObjectURL(blob);

        this.thumbnails.push(thumbnail);

        this.ref.detectChanges();
      });

    } catch (error) {
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, error);
    }
  }

  public name : string = '';

  public readRoles : string[] = [];

  public removeThumbnail(thumbnail : Thumbnail) : void {
    let index = this.thumbnails.findIndex(it => it.id === thumbnail.id);
    if (index >= 0) {
      if (!thumbnail.file) {
        this.thumbnailsToRemove.push(thumbnail.id);
      }
      this.thumbnails.splice(index, 1);
      this.ref.detectChanges();
    }
  }

  public sanitize(url : string) : SafeUrl {
    return this.sanitizer.bypassSecurityTrustUrl(url);
  }

  public save() : void {
    this.controls.modals?.toggleLoader();

    let thumbnailFiles : any[] = this.thumbnails.map(it => it.file);

    this.files.updateWithThumbnails(this.id, {
      description: this.description,
      name: this.name,
      readRoles: this.readRoles,
      thumbnailsToRemove: this.thumbnailsToRemove,
      updateRoles: this.updateRoles
    }, thumbnailFiles)
    .subscribe(response => {
      this.controls.modals?.toggleLoader();
      this.controls.refresh([ 'Files' ]);
    },
    error => {
      this.controls.modals?.toggleLoader();
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Update error.');
    });
  }

  public src : string = '';

  public thumbnailIdCounts : number = 0;

  public thumbnails : Thumbnail[] = [];

  public thumbnailsToRemove : string[] = [];

  public type : string = '';

  public updateRoles : string[] = [];

  @ViewChild('video')
  public video : ElementRef | undefined;
}

class Thumbnail {
  public unsaved : boolean = true;
  public id : string = '';
  public file : any;
  public name : string = '';
  public src : string = '';
}
