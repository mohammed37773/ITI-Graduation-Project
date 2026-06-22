import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyChildren } from './my-children';

describe('MyChildren', () => {
  let component: MyChildren;
  let fixture: ComponentFixture<MyChildren>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyChildren],
    }).compileComponents();

    fixture = TestBed.createComponent(MyChildren);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
