import { Component, Input } from '@angular/core';
import { NurseryListItem } from '../../../core/models/parent-nursery.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-nursery-card',
  imports: [CommonModule],
  templateUrl: './nursery-card.html',
  styleUrl: './nursery-card.css',
})
export class NurseryCard {
  @Input({ required: true }) nursery!: NurseryListItem;
}
