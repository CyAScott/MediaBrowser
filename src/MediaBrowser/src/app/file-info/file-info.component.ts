import { ChangeDetectorRef, Component, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonControlsService, Page } from '../common-controls.service';
import { FileReadModel, FilesService } from '../files.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';
import { Thumbnail, ThumbnailBuilderComponent } from '../thumbnail-builder/thumbnail-builder.component';
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
    public users : UsersService) {
    super(controls, logger, 'File')
    route.params.subscribe(params => this.id = params.id);
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
        let thumbnail = new Thumbnail();
  
        thumbnail.file = blob;
        thumbnail.name = '';
        thumbnail.src = URL.createObjectURL(blob);

        this.thumbnailBuilder?.add(thumbnail);

        this.ref.detectChanges();
      }, 'image/jpeg', 0.75);

    } catch (error) {
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, error);
    }
  }

  public name : string = '';

  public readRoles : string[] = [];

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

  @ViewChild('thumbnailBuilder')
  public thumbnailBuilder : ThumbnailBuilderComponent | undefined;

  public thumbnails : Thumbnail[] = [];

  public thumbnailsToRemove : string[] = [];

  public type : string = '';

  public updateRoles : string[] = [];

  @ViewChild('video')
  public video : ElementRef | undefined;
}
