import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonControlsService, Page } from '../common-controls.service';
import { LoggerService } from '../logger.service';
import { MsgBoxType } from '../modals/modals.component';
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

  public firstName : string = '';

  public id : string = '';
  
  public init() : void {
    this.users
      .get(this.id)
      .subscribe(user => {
        this.controls.modals?.toggleLoader();

        this.firstName = user.firstName;
        this.lastName = user.lastName;
        this.userName = user.userName;
        this.roles = user.roles;
      });
  }

  public lastName : string = '';

  public password : string = '';

  public passwordConfirmed : string = '';

  public update(): void {
    try {
      if (!this.firstName || !this.firstName.length) {
        throw 'Please provide a first name.';
      }

      if (!this.lastName || !this.lastName.length) {
        throw 'Please provide a last name.';
      }

      if ((!this.password || !this.password.length) && (this.password !== this.passwordConfirmed)) {
        throw 'The passwords do not match.';
      }

      if (!this.userName || !this.userName.length) {
        throw 'Please provide a user name.';
      }
    } catch (error) {
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, error);
    }

    this.controls.modals?.toggleLoader();

    this.users.update(this.id, {
      firstName: this.firstName,
      lastName: this.lastName,
      password: this.password,
      userName: this.userName,
      roles: this.roles
    })
    .subscribe(response => {
      this.controls.modals?.toggleLoader();
      this.controls.refresh([ 'Users' ]);
    },
    error => {
      this.controls.modals?.toggleLoader();
      this.controls.modals?.getMsgBoxResult(MsgBoxType.Ok, 'Update user error.');
    });
  }

  public userName : string = '';

  public roles : string[] = [];
}
