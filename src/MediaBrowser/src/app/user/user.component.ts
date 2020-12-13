import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonControlsService, Page } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { UserReadModel, UsersService } from '../users.service';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.less']
})
export class UserComponent extends Page {

  constructor(
    controls: CommonControlsService,
    logger : LoggerService,
    route : ActivatedRoute,
    private users : UsersService) {
    super(controls, logger, 'User');
    this.controls.page = this;
    route.params.subscribe(params => this.id = params.id);
  }

  public id : string = '';
  
  public init() : void {
    this.users
      .get(this.id)
      .subscribe(user => {
        this.controls.modals?.toggleLoader();
        this.user = user;
      });
  }

  public user : UserReadModel | undefined;
}
