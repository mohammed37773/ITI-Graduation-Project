import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RatingStars } from './rating-stars';

describe('RatingStars', () => {
  let component: RatingStars;
  let fixture: ComponentFixture<RatingStars>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RatingStars],
    }).compileComponents();

    fixture = TestBed.createComponent(RatingStars);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
