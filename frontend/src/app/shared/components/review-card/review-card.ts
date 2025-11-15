import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { Review } from '../../../core/services/review.service';

@Component({
  selector: 'app-review-card',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    TranslateModule
  ],
  templateUrl: './review-card.html',
  styleUrl: './review-card.scss'
})
export class ReviewCard {
  @Input() review!: Review;
  @Input() showServiceInfo = false;
  @Input() showActions = true;
  @Input() canRespond = false;
  @Input() canEdit = false;
  @Input() canDelete = false;
  @Input() isProvider = false;

  @Output() helpful = new EventEmitter<string>();
  @Output() respond = new EventEmitter<string>();
  @Output() edit = new EventEmitter<string>();
  @Output() delete = new EventEmitter<string>();
  @Output() viewImages = new EventEmitter<string[]>();

  get ratingArray(): number[] {
    return Array(5).fill(0).map((_, i) => i + 1);
  }

  onHelpful(): void {
    this.helpful.emit(this.review.id);
  }

  onRespond(): void {
    this.respond.emit(this.review.id);
  }

  onEdit(): void {
    this.edit.emit(this.review.id);
  }

  onDelete(): void {
    this.delete.emit(this.review.id);
  }

  onViewImages(): void {
    if (this.review.imageUrls && this.review.imageUrls.length > 0) {
      this.viewImages.emit(this.review.imageUrls);
    }
  }
}
