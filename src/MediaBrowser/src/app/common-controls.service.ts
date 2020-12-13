import { ActivatedRoute, ActivationEnd, NavigationEnd, Router, RoutesRecognized } from '@angular/router';
import { EventEmitter, Injectable } from '@angular/core';
import { Modals, SelectionOption } from './modals/modals.component';
import { Pagination } from './pagination/pagination.component';
import { Search } from './search/search.component';
import { LoggerService } from './logger.service';
import { AppComponent } from './app.component';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CommonControlsService {
  
  private routeChangesEmitter : EventEmitter<RouteEvent> = new EventEmitter();

  public appComponent : AppComponent | undefined;
  public autoPlay : boolean = false;
  public modals : Modals | undefined;
  public page : Page | undefined;
  public pagination : Pagination | undefined;
  public routeChanges : Observable<RouteEvent>;
  public search : Search | undefined;

  public refresh(commands : any[] | undefined = undefined) {
    
    let query = { 
      filter: this.modals?.filterSelection?.value,
      keywords: this.search?.keywords?.length ? this.search.keywords : undefined,
      sort: this.modals?.sortSelection?.value
    };

    if (commands) {

      this.logger.debug(`New Route: ${JSON.stringify(commands)}, ${JSON.stringify(query)}`);

      this.router.navigate(commands, { queryParams: query, queryParamsHandling: 'merge' });
    } else {
      
      this.logger.debug(`Reload Route: ${JSON.stringify(query)}`);

      this.router.navigate([], { relativeTo: this.route, queryParams: query, queryParamsHandling: 'merge' });
    }
  }

  constructor(
    private logger : LoggerService,
    private router: Router,
    private route: ActivatedRoute) {

    this.routeChanges = this.routeChangesEmitter;

    this.router.events.subscribe(event => {

      if (event instanceof RoutesRecognized) {
        var eventRoute = new RouteEvent();

        eventRoute.routesRecognized = event as RoutesRecognized;

        this.routeChangesEmitter.emit(eventRoute);

        if (this.page) {
          this.page.unload();
        }

      } else if (event instanceof ActivationEnd) {
        var eventRoute = new RouteEvent();
        eventRoute.activationEnd = event as ActivationEnd;

        this.routeChangesEmitter.emit(eventRoute);

      } else if (event instanceof NavigationEnd) {
        var eventRoute = new RouteEvent();

        eventRoute.navigationEnd = event as NavigationEnd;

        this.routeChangesEmitter.emit(eventRoute);

        this.page?.init();
      }
    });
  }
  
  public toggleAutoplay() : boolean {
    this.autoPlay = !this.autoPlay;
    this.logger.debug(`Auto Play: ${this.autoPlay}`);
    return this.autoPlay;
  }
}

export abstract class Page {
  constructor(
    public readonly controls : CommonControlsService,
    public logger : LoggerService,
    public readonly name : string) {
    controls.page = this;
  }
  public init() : void {
    if (this.controls.pagination?.pageCount) {
      this.controls.pagination.pageCount = 1;
    }
    this.controls.modals?.toggleLoader();
  }

  public unload(): void {
  }
}

export abstract class PageSearchable extends Page {
  constructor(
    controls : CommonControlsService,
    logger : LoggerService,
    name : string,
    public readonly filterOptions : SelectionOption[],
    public readonly defaultFilter : SelectionOption | undefined,
    public readonly sortOptions : SelectionOption[],
    public readonly defaultSort : SelectionOption | undefined) {
    super(controls, logger, name);
  }

  public abstract add() : void;

  public abstract addEnabled() : boolean;

  public getSearchRequest() : SearchRequest {
    if (!this.controls.modals) {
      this.logger.error('Modals not set');
      throw new Error('Modals not set');
    }

    if (!this.controls.pagination) {
      this.logger.error('Pagination not set');
      throw new Error('Pagination not set');
    }

    if (!this.controls.search) {
      this.logger.error('Search not set');
      throw new Error('Search not set');
    }

    return new SearchRequest(
      this.controls.modals.sortAscending,
      this.controls.modals.filterSelection?.value ? this.controls.modals.filterSelection.value : this.defaultFilter?.value,
      this.controls.search.keywords,
      this.controls.pagination.countPerPage * (this.controls.pagination.pageSelection - 1),
      this.controls.modals.sortSelection?.value ? this.controls.modals.sortSelection.value : this.defaultSort?.value,
      this.controls.pagination.countPerPage);
  }
}

export class RouteEvent {
  public activationEnd : ActivationEnd | undefined;
  public navigationEnd : NavigationEnd | undefined;
  public routesRecognized : RoutesRecognized | undefined;
}

export class SearchRequest {
  constructor(
    public ascending: boolean,
    public filter : string | undefined,
    public keywords : string,
    public skip : number,
    public sort : string | undefined,
    public take : number) {
  }
}

export interface Unload {
  unload(): void;
}