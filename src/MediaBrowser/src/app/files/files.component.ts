import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonControlsService, PageSearchable } from '../common-controls.service';
import { FileFilterOptions, FileSortOptions, FilesService, SearchFilesRequest, SearchFilesResponse } from '../files.service';
import { LoggerService } from '../logger.service';
import { SelectionOption } from '../modals/modals.component';

@Component({
  selector: 'app-files',
  templateUrl: './files.component.html',
  styleUrls: ['./files.component.less']
})
export class FilesComponent extends PageSearchable {

  public static readonly filterOptions : SelectionOption[] = [
    new SelectionOption('No Filter', FileFilterOptions.noFilter),
    new SelectionOption('Cached Files', FileFilterOptions.cached),
    new SelectionOption('Audio Files', FileFilterOptions.audioFiles),
    new SelectionOption('Photos', FileFilterOptions.photos),
    new SelectionOption('Videos', FileFilterOptions.videos)
  ];
  public static readonly sortOptions : SelectionOption[] = [
    new SelectionOption('Created On', FileSortOptions.createdOn),
    new SelectionOption('Duration', FileSortOptions.duration),
    new SelectionOption('Name', FileSortOptions.name),
    new SelectionOption('Type', FileSortOptions.type)
  ];

  constructor(
    controls : CommonControlsService,
    logger : LoggerService,
    route : ActivatedRoute,
    private files : FilesService) {
    super(controls, logger, 'Files',
      FilesComponent.filterOptions, FilesComponent.filterOptions[0],
      FilesComponent.sortOptions, FilesComponent.sortOptions[0])
    route.params.subscribe(params => this.playlistId = params.playlistId);
  }

  public add() : void {
    this.controls.refresh([ 'AddFile' ]);
  }
  
  public addEnabled() : boolean {
    return !this.playlistId;
  }

  public init(): void {
    this.controls.autoPlay = false;
    this.files
      .search(new SearchFilesRequest(this.getSearchRequest()), this.playlistId && this.playlistId.length ? this.playlistId : undefined)
      .subscribe(response => {
        this.controls.modals?.toggleLoader();
        if (this.controls.pagination) {
          this.controls.pagination.pageCount = Math.ceil(response.count / this.controls.pagination.countPerPage);
        }
        this.response = response;
      });
  }

  public playlistId : string = '';

  public response : SearchFilesResponse | undefined;
}
