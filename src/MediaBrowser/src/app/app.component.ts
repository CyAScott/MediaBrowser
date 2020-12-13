import { Component } from '@angular/core';
import { CommonControlsService, PageSearchable } from './common-controls.service';
import { LoggerService } from './logger.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent implements AppComponent {
  
  private fullscreenchange() : void {
    this.fullScreen = !this.fullScreen;
    this.log.debug(`Full Screen: ${this.fullScreen}`);
  }

  constructor(
    private controls : CommonControlsService,
    private log : LoggerService) {
    controls.appComponent = this;
    document.onfullscreenchange = () => this.fullscreenchange();
  }

  public addEnabled() : boolean {
    let page = this.controls.page instanceof PageSearchable ? this.controls.page as PageSearchable : null;
    return page ? true : false;
  }
  public add() : void {
    let page = this.controls.page instanceof PageSearchable ? this.controls.page as PageSearchable : null;
    if (page) {
      
      if (this.controls.modals?.filterSelection) {
        this.controls.modals.filterSelection = undefined;
      }

      if (this.controls.modals?.sortSelection) {
        this.controls.modals.sortSelection = undefined;
      }

      page.add();
    }
  }
  
  public fullScreen : boolean = false;
  public menuVisible : boolean = false;
  public toggleMenu(onlyIfVisible: boolean = false) : void {
    if (this.menuVisible) {
      this.menuVisible = false;
    } else if (!onlyIfVisible) {
      this.menuVisible = true;
    }
  }
  
  public async toggleFullScreen() : Promise<void> {
    if (!document.fullscreenElement) {
      await document.documentElement.requestFullscreen();
    } else {
      if (document.exitFullscreen) {
        await document.exitFullscreen();
      }
    }
  }
}

export interface AppComponent {
  fullScreen : boolean;
  menuVisible : boolean;
  toggleFullScreen() : Promise<void>;
}
