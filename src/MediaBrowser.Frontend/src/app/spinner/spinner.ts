import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-spinner',
  imports: [CommonModule],
  templateUrl: './spinner.html',
  styleUrls: ['./spinner.css']
})
export class SpinnerComponent {
  @Input() isLoading: boolean = false;
}