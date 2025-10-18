import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface PeopleData {
  cast: string[];
  directors: string[];
  producers: string[];
  writers: string[];
}

@Component({
  selector: 'app-people-section',
  imports: [CommonModule, FormsModule],
  templateUrl: './people-section.html',
  styleUrls: ['../media-editor.css', './people-section.css']
})
export class PeopleSectionComponent {
  @Input() peopleData: PeopleData = {
    cast: [],
    directors: [],
    producers: [],
    writers: []
  };
  
  @Output() peopleDataChange = new EventEmitter<PeopleData>();

  addArrayItem(arrayName: keyof PeopleData): void {
    this.peopleData[arrayName].push('');
    this.onPeopleChange();
  }

  onPeopleChange(): void {
    this.peopleDataChange.emit(this.peopleData);
  }

  removeArrayItem(arrayName: keyof PeopleData, index: number): void {
    this.peopleData[arrayName].splice(index, 1);
    this.onPeopleChange();
  }

  trackByIndex(index: number): number {
    return index;
  }
}