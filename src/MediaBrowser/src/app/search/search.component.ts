import { Component } from '@angular/core';
import { CommonControlsService, PageSearchable } from '../common-controls.service';
import { LoggerService } from '../logger.service';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.less']
})
export class SearchComponent implements Search {

  constructor(
    private controls : CommonControlsService,
    private logger : LoggerService) {  

    controls.search = this;
    
    controls.routeChanges.subscribe(event => {
      if (!event.activationEnd) {
        return;
      }

      let activationEnd = event.activationEnd;
      let queryParams = activationEnd.snapshot.queryParams;

      let page = controls.page instanceof PageSearchable ? controls.page as PageSearchable : null;

      this.filterOptionsEnabled = page?.filterOptions?.length ? true : false;
      this.keywords = queryParams.keywords || '';
      this.searchVisible = page ? true : false;
      this.sortOptionsEnabled = page?.sortOptions?.length ? true : false;
    });
  }

  public filterOptionsEnabled: boolean = false;

  public keywords: string = '';

  public refresh(keywords : any = null) : void {

    if (keywords && keywords === this.keywords) {
      return;
    }

    this.keywords = keywords || '';

    this.controls.refresh();
  }

  public searchVisible: boolean = false;

  public selectFilter() : void {
    this.controls.modals?.getFilterSelection();
  }

  public selectSort() : void {
    this.controls.modals?.getSortSelection();
  }

  public sortOptionsEnabled: boolean = false;
}

export interface Search {
  filterOptionsEnabled : boolean;
  keywords : string;
  searchVisible : boolean;
  sortOptionsEnabled : boolean;
}
