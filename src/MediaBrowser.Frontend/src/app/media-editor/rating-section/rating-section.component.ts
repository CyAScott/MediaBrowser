import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-rating-section',
  imports: [CommonModule],
  templateUrl: './rating-section.html',
  styleUrls: ['../media-editor.css', './rating-section.css']
})
export class RatingSectionComponent {
  @Input() rating: number | null = null;

  @Output() ratingChange = new EventEmitter<number>();

  getStarClass(starNumber: number): string {
    return starNumber <= (this.rating ?? 0) ? 'fa-solid fa-star' : 'fa-regular fa-star';
  }

  setRating(rating: number): void {
    this.rating = rating;
    this.ratingChange.emit(rating);
  }
}