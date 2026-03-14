import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavigationTabsComponent } from './navigation-tabs/navigation-tabs';
import { UsersService } from './services/users.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavigationTabsComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('render');
  protected usersService = inject(UsersService);
}
