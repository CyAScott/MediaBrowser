import { CommonModule } from '@angular/common';
import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { MediaService } from '../../services';
import { TypeaheadInputComponent } from './typeahead-input.component';

export interface PeopleData {
  cast: string[];
  directors: string[];
  genres: string[];
  producers: string[];
  writers: string[];
}

@Component({
  selector: 'app-people-section',
  imports: [CommonModule, FormsModule, TypeaheadInputComponent],
  templateUrl: './people-section.html',
  styleUrls: ['../media-editor.css', './people-section.css']
})
export class PeopleSectionComponent implements OnInit {
  private mediaService: MediaService = inject(MediaService);

  @Input() peopleData: PeopleData = {
    cast: [],
    directors: [],
    genres: [],
    producers: [],
    writers: []
  };
  
  @Output() peopleDataChange = new EventEmitter<PeopleData>();

  static allPeople: PeopleData = {
    cast: [],
    directors: [],
    genres: [],
    producers: [],
    writers: []
  };
  static readonly CACHE_KEY = 'cached-people';
  static clearCacheIfStale(peopleData: PeopleData): void {
    if (peopleData.cast.some(p => !PeopleSectionComponent.allPeople.cast.includes(p)) ||
        peopleData.directors.some(p => !PeopleSectionComponent.allPeople.directors.includes(p)) ||
        peopleData.genres.some(p => !PeopleSectionComponent.allPeople.genres.includes(p)) ||
        peopleData.producers.some(p => !PeopleSectionComponent.allPeople.producers.includes(p)) ||
        peopleData.writers.some(p => !PeopleSectionComponent.allPeople.writers.includes(p))) {
        localStorage.removeItem(PeopleSectionComponent.CACHE_KEY);
    }
  }

  addArrayItem(arrayName: keyof PeopleData): void {
    this.peopleData[arrayName].push('');
    this.onPeopleChange();
  }

  async ngOnInit(): Promise<void> {
    const cachedPeopleValue = localStorage.getItem(PeopleSectionComponent.CACHE_KEY);
    if (cachedPeopleValue) {
      PeopleSectionComponent.allPeople = JSON.parse(cachedPeopleValue);
    } else {
      PeopleSectionComponent.allPeople = {
        cast: await firstValueFrom(this.mediaService.getAllTags('cast')),
        directors: await firstValueFrom(this.mediaService.getAllTags('directors')),
        genres: await firstValueFrom(this.mediaService.getAllTags('genres')),
        producers: await firstValueFrom(this.mediaService.getAllTags('producers')),
        writers: await firstValueFrom(this.mediaService.getAllTags('writers')),
      };
      localStorage.setItem(PeopleSectionComponent.CACHE_KEY, JSON.stringify(PeopleSectionComponent.allPeople));
    }
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

  // Get suggestions for each field type
  getCastSuggestions(): string[] {
    return PeopleSectionComponent.allPeople.cast;
  }

  getDirectorsSuggestions(): string[] {
    return PeopleSectionComponent.allPeople.directors;
  }

  getGenresSuggestions(): string[] {
    return PeopleSectionComponent.allPeople.genres;
  }

  getProducersSuggestions(): string[] {
    return PeopleSectionComponent.allPeople.producers;
  }

  getWritersSuggestions(): string[] {
    return PeopleSectionComponent.allPeople.writers;
  }

  // Handle suggestion selection for updating array values
  onSuggestionSelected(arrayName: keyof PeopleData, index: number, selectedValue: string): void {
    this.peopleData[arrayName][index] = selectedValue;
    this.onPeopleChange();
  }
}