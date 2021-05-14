import { Component, OnInit, ViewChild } from '@angular/core';
import { NavigationEnd, Router, Scroll } from '@angular/router';
import { CommonControlsService, PageSearchable } from './common-controls.service';
import { LoggerService } from './logger.service';
import { UsersService } from './users.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent implements AppComponent, OnInit {
  
  private fullscreenchange() : void {
    this.fullScreen = !this.fullScreen;
    this.log.debug(`Full Screen: ${this.fullScreen}`);
  }

  constructor(
    public users : UsersService,
    private controls : CommonControlsService,
    private log : LoggerService,
    private router : Router) {

    controls.appComponent = this;
    document.onfullscreenchange = () => this.fullscreenchange();
  }

  public ngOnInit() : void {
    if (navigator?.serviceWorker?.register) {
      navigator.serviceWorker.register('/service-worker.js')
        .then((reg) => {
          this.log.debug(`Service Worker Registered: ${JSON.stringify(reg)}`);
        }).catch((error) => {
          this.log.error(`Service Worker Registration Failed: ${JSON.stringify(error)}`);
        });
    }
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.scrollToTop();
      }
    });
  }

  public addEnabled() : boolean {
    let page = this.controls.page instanceof PageSearchable ? this.controls.page as PageSearchable : null;
    return page && page.addEnabled() ? true : false;
  }
  public add() : void {
    let page = this.controls.page instanceof PageSearchable ? this.controls.page as PageSearchable : null;
    if (page && page.addEnabled()) {
      
      if (this.controls.modals?.filterSelection) {
        this.controls.modals.filterSelection = undefined;
      }

      if (this.controls.modals?.sortSelection) {
        this.controls.modals.sortSelection = undefined;
      }

      page.add();
    }
  }
  public pagesEnabled() : boolean {
    return this.controls.pagination?.pageCount !== undefined && this.controls.pagination.pageCount > 1;
  }

  @ViewChild('routerBody')
  public routerBody : any;
  public scrollToTop() : void {
    if (this.routerBody?.nativeElement) {
      this.routerBody.nativeElement.scrollLeft  = 0;
      this.routerBody.nativeElement.scrollTop  = 0;
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
  scrollToTop() : void;
  toggleFullScreen() : Promise<void>;
}
