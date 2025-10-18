import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface TitleData {
  title: string;
  originalTitle: string;
  description: string;
}

@Component({
  selector: 'app-title-section',
  imports: [CommonModule, FormsModule],
  templateUrl: './title-section.html',
  styleUrls: ['../media-editor.css', './title-section.css']
})
export class TitleSectionComponent {
  @Input() titleData: TitleData = {
    title: '',
    originalTitle: '',
    description: ''
  };
  
  @Output() titleDataChange = new EventEmitter<TitleData>();

  onTitleChange(): void {
    this.titleDataChange.emit(this.titleData);
  }
}