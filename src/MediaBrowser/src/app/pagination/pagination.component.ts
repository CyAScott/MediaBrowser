import { Component, HostListener, OnInit } from '@angular/core';
import { CommonControlsService } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.less']
})
export class PaginationComponent implements OnInit, Pagination {

  constructor(
    private controls : CommonControlsService,
    private logger : LoggerService) {

    controls.pagination = this;

    controls.routeChanges.subscribe(event => {
      if (!event.activationEnd) {
        return;
      }

      let activationEnd = event.activationEnd;
      let params = activationEnd.snapshot.params;

      if (!this.pageSelectionInput) {
        this.pageSelectionInput = document.getElementById('pageSelectionInput');
      }

      this.pageSelection = params.pageSelection ? params.pageSelection * 1 : 1;

      if (this.pageSelectionInput) {
        this.pageSelectionInput.value = this.pageSelection;
      }
    });


  }

  @HostListener('window:resize', ['$event'])
  onResize() {
    this.ngOnInit();
  }
  
  ngOnInit() : void {
    this.countPerPage = Math.ceil((window.innerWidth / 210) * (window.innerHeight / 210) * 2);
  }

  public countPerPage : number = 20;
  public canGoBack() : boolean {
    return this.pageSelection * 1 > 1;
  }
  public canGoForward() : boolean {
    return this.pageSelection * 1 < this.pageCount * 1;
  }
  public hasMultiplePages() : boolean {
    return this.pageCount * 1 > 1;
  }
  public setPageSelection(value : number) : void {
    if (!this.controls.page?.name) {
      return;
    }
    
    let pageCount = this.pageCount * 1;

    if (value >= 1 && value <= pageCount) {
      this.controls.refresh([ this.controls.page.name, value.toString() ]);
    }
    else {
      this.logger.error('Invalid Page Selected');
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Invalid Page Selected');
    }
  }
  public pageCount : number = 1;
  public pageSelection : number = 1;
  public pageSelectionChanged(target : any) : void {
    this.setPageSelection(target.value * 1);
  }
  public pageSelectionInput : any;
}

export interface Pagination {
  countPerPage : number;
  pageCount : number;
  pageSelection : number;
}
