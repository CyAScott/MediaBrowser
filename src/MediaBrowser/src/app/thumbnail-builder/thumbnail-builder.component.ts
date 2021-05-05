import { ChangeDetectorRef, Component, Input } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-thumbnail-builder',
  templateUrl: './thumbnail-builder.component.html',
  styleUrls: ['./thumbnail-builder.component.less']
})
export class ThumbnailBuilderComponent {

  constructor(
    private ref : ChangeDetectorRef,
    private sanitizer : DomSanitizer) { }

  public add(thumbnail : Thumbnail) : void {
    this.thumbnailIdCounts++;
    thumbnail.id = this.thumbnailIdCounts + '';
    this.thumbnails.push(thumbnail);
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

  public thumbnailIdCounts : number = 0;

  @Input()
  public thumbnails : Thumbnail[] = [];

  @Input()
  public thumbnailsToRemove : string[] = [];
}

export class Thumbnail {
  public unsaved : boolean = true;
  public id : string = '';
  public file : any;
  public name : string = '';
  public src : string = '';
}
