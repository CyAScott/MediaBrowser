import { ChangeDetectorRef, Component, ElementRef, ViewChild } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { CommonControlsService, Page } from '../common-controls.service';
import { FilesService } from '../files.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';
import { Thumbnail, ThumbnailBuilderComponent } from '../thumbnail-builder/thumbnail-builder.component';
import { UsersService } from '../users.service';

@Component({
  selector: 'app-add-file',
  templateUrl: './add-file.component.html',
  styleUrls: ['./add-file.component.less']
})
export class AddFileComponent extends Page {

  constructor(
    controls : CommonControlsService,
    logger : LoggerService,
    private files : FilesService,
    private ref: ChangeDetectorRef,
    private sanitizer : DomSanitizer,
    public users : UsersService) {
    super(controls, logger, 'AddFile')
  }

  public add(input : any) : void {
    let contentType = (input.files[0].type as string)?.toLowerCase() || '';

    if (contentType.startsWith('audio/')) {
      this.type = 'audio';
    } else if (contentType.startsWith('image/')) {
      this.type = 'photo';
    } else if (contentType.startsWith('video/')) {
      this.type = 'video';
    } else {
      this.type = '';
    }
    
    this.src = this.type.length ? URL.createObjectURL(this.mediaFile = input.files[0]) : '';

    if (this.video?.nativeElement) {
      this.video.nativeElement.src = this.type == 'video' ? this.src : undefined;
    }
  }

  public description : string = '';

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

  public mediaFile : any;

  public name : string = '';

  public readRoles : string[] = [];

  public sanitize(url : string) : SafeUrl {
    return this.sanitizer.bypassSecurityTrustUrl(url);
  }
  
  public src : string = '';

  @ViewChild('thumbnailBuilder')
  public thumbnailBuilder : ThumbnailBuilderComponent | undefined;

  public thumbnails : Thumbnail[] = [];

  public type : string = '';

  public updateRoles : string[] = [];
  
  public upload() : void {
    if (!this.mediaFile) {
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Please select a media file to upload.');
      return;
    }

    this.controls.modals?.toggleLoader();

    let thumbnailFiles : any[] = this.thumbnails.map(it => it.file);

    this.files.upload({
      description: this.description,
      name: this.name,
      readRoles: this.readRoles,
      updateRoles: this.updateRoles
    }, this.mediaFile, thumbnailFiles)
    .subscribe(response => {
      this.controls.modals?.toggleLoader();
      this.controls.refresh([ 'Files' ]);
    },
    error => {
      this.controls.modals?.toggleLoader();
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Upload error.');
    });
  }

  @ViewChild('video')
  public video : ElementRef | undefined;
}
