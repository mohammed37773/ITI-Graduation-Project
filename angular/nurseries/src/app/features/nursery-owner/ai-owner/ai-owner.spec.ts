import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AiOwner } from './ai-owner';

describe('AiOwner', () => {
  let component: AiOwner;
  let fixture: ComponentFixture<AiOwner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AiOwner],
    }).compileComponents();

    fixture = TestBed.createComponent(AiOwner);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
