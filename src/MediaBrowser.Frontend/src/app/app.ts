import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { VerticalTabsComponent } from './vertical-tabs/vertical-tabs.component';
import { UsersService } from './services/users.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, VerticalTabsComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('render');
  protected usersService = inject(UsersService);
}
