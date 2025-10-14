import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { VerticalTabsComponent } from './vertical-tabs/vertical-tabs.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, VerticalTabsComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('render');
}
