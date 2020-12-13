import { Component } from '@angular/core';
import { Observable } from 'rxjs';
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
    new SelectionOption('Html5 Friendly', FileFilterOptions.html5Friendly),
    new SelectionOption('Audio Files', FileFilterOptions.AudioFiles),
    new SelectionOption('No Filter', FileFilterOptions.noFilter),
    new SelectionOption('Non Html5 Friendly', FileFilterOptions.nonHtml5Friendly),
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
    private files : FilesService) {
    super(controls, logger, 'Files',
      FilesComponent.filterOptions, FilesComponent.filterOptions[0],
      FilesComponent.sortOptions, FilesComponent.sortOptions[0])
  }

  public add() : void {
    this.controls.refresh([ 'AddFile' ]);
  }
  
  public addEnabled() : boolean {
    return true;
  }

  public init(): void {
    this.controls.autoPlay = false;
    this.files
      .search(new SearchFilesRequest(this.getSearchRequest()))
      .subscribe(response => {
        this.controls.modals?.toggleLoader();
        if (this.controls.pagination) {
          this.controls.pagination.pageCount = Math.ceil(response.count / this.controls.pagination.countPerPage);
        }
        this.response = response;
      });
  }

  public response : SearchFilesResponse | undefined;
}
