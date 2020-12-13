import { Component } from '@angular/core';
import { CommonControlsService, PageSearchable } from '../common-controls.service';
import { LoggerService } from '../logger.service';

@Component({
  selector: 'app-modals',
  templateUrl: './modals.component.html',
  styleUrls: ['./modals.component.less']
})
export class ModalsComponent implements Modals {

  constructor(
    controls : CommonControlsService,
    private logger : LoggerService) {

    controls.modals = this;

    controls.routeChanges.subscribe(event => {

      if (event.routesRecognized) {
        this.toggleLoader();
      }

      if (!event.activationEnd) {
        return;
      }

      let activationEnd = event.activationEnd;
      let queryParams = activationEnd.snapshot.queryParams;
      
      let page = controls.page instanceof PageSearchable ? controls.page as PageSearchable : undefined;
      
      this.filterOptions = page && page?.filterOptions ? page.filterOptions : [];
      this.filterSelection = queryParams.filter && page ? this.filterOptions.find(it => it?.value === queryParams.filter) : page?.defaultFilter;

      this.sortOptions = page && page?.sortOptions ? page.sortOptions : [];
      this.sortSelection = queryParams.sort && page ? this.sortOptions.find(it => it?.value === queryParams.sort) : page?.defaultSort;
    });
  }

  public closeModal() : void {
    if (this.msgBoxVisible || this.loaderVisible) {
        return;
    }

    this.logger.debug('Modal Closed');

    if (this.filterSelectionDone) {
      this.filterSelectionDone(this.filterSelection);
    }

    if (this.sortSelectionDone) {
      this.sortSelectionDone(this.sortSelection);
    }
  }

  public isVisible() : boolean {
    return this.filterVisible || this.loaderVisible || this.msgBoxVisible || this.sortVisible;
  }

  public filterOptions : SelectionOption[] = [];
  public filterSelection : SelectionOption | undefined;
  public filterSelectionDone : any;
  public filterVisible : boolean = false;

  public getFilterSelection() : Promise<SelectionOption> {

    this.logger.debug(`Filter Options: ${JSON.stringify(this.filterOptions)}, ${JSON.stringify(this.filterSelection)}`);

    this.filterVisible = true;

    return new Promise<SelectionOption>((resolve) =>  this.filterSelectionDone = (filterSelection : SelectionOption) => {
      this.filterSelection = filterSelection;
      this.filterSelectionDone = null;
      this.filterVisible = false;
      this.logger.debug(`Filter Selection: ${JSON.stringify(filterSelection)}`);
      resolve(filterSelection);
    });
  }
  
  
  public msgBoxButtons : Button[] = [ new Button(), new Button(), new Button() ];
  public msgBoxDone : any;
  public msgBoxText : String = '';
  public msgBoxVisible : boolean = false;

  public getMsgBoxResult(type : MsgBoxType, msg : String) : Promise<MsgBoxResult> {
    if (this.msgBoxDone) {
      const error = 'Dislog already displayed.';
      this.logger.error(error);
      throw error;
    }

    for (let index = 0; index < this.msgBoxButtons.length; index++) {
      this.msgBoxButtons[index].clear();
    }

    switch (type) {
      case MsgBoxType.Ok:
        this.msgBoxButtons[2].set(MsgBoxResult.Ok, 'Ok');
        break;
      case MsgBoxType.OkCancel:
        this.msgBoxButtons[1].set(MsgBoxResult.Cancel, 'Cancel');
        this.msgBoxButtons[2].set(MsgBoxResult.Ok, 'Ok');
        break;
      case MsgBoxType.YesNo:
        this.msgBoxButtons[1].set(MsgBoxResult.No, 'No');
        this.msgBoxButtons[2].set(MsgBoxResult.Yes, 'Yes');
        break;
      case MsgBoxType.YesNoCancel:
        this.msgBoxButtons[0].set(MsgBoxResult.Cancel, 'Cancel');
        this.msgBoxButtons[1].set(MsgBoxResult.No, 'No');
        this.msgBoxButtons[2].set(MsgBoxResult.Yes, 'Yes');
        break;
    }

    this.msgBoxText = msg;
    this.msgBoxVisible = true;
    
    this.logger.debug(`${msg} (${this.msgBoxButtons.filter(it => it.visible).map(it => it.text).join(', ')})`);

    return new Promise<MsgBoxResult>((resolve) =>  this.msgBoxDone = (button: Button) => {
      this.msgBoxDone = null;
      this.msgBoxVisible = false;
      this.logger.debug(button.text);
      resolve(button.result);
    });
  }

  public loaderVisible : boolean = false;
  public toggleLoader() : void {
    this.loaderVisible = !this.loaderVisible;
  }

  public sortAscending : boolean = true;
  public sortOptions : SelectionOption[] = [];
  public sortSelection : SelectionOption | undefined;
  public sortSelectionDone : any;
  public sortVisible : boolean = false;

  public getSortSelection() : Promise<SelectionOption> {

    this.logger.debug(`Sort Options: ${JSON.stringify(this.sortOptions)}, ${JSON.stringify(this.sortSelection)}`);
    
    this.sortVisible = true;

    return new Promise<SelectionOption>((resolve) =>  this.sortSelectionDone = (sortSelection : SelectionOption) => {
      this.sortSelection = sortSelection;
      this.sortSelectionDone = null;
      this.sortVisible = false;
      this.logger.debug(`Sort Selection: ${JSON.stringify(sortSelection)}`);
      resolve(sortSelection);
    });
  }
}

class Button {
  public clear() : void {
    this.result = MsgBoxResult.Ok;
    this.text = '';
    this.visible = false;
  }

  public result : MsgBoxResult = MsgBoxResult.Ok;

  public set(result : MsgBoxResult, text : String) : void {
    this.result = result;
    this.text = text;
    this.visible = true;
  }

  public text : String = '';

  public visible : boolean = false;
}

export class SelectionOption {
  constructor(name: string, value: string) { 
    this.name = name;
    this.value = value;
  }
  public name : string = '';
  public value : string = '';
}

export enum MsgBoxResult {
  Cancel,
  Yes,
  No,
  Ok
}

export enum MsgBoxType {
  Ok,
  OkCancel,
  YesNo,
  YesNoCancel
}

export interface Modals {

  filterSelection : SelectionOption | undefined;

  getFilterSelection() : Promise<SelectionOption>;

  getMsgBoxResult(type : MsgBoxType, msg : String) : Promise<MsgBoxResult>;
  
  getSortSelection() : Promise<SelectionOption>;

  sortAscending : boolean;

  sortSelection : SelectionOption | undefined;

  toggleLoader() : void;
}
