import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddOpenDiscountComponent } from './add-open-discount.component';

describe('AddOpenDiscountComponent', () => {
  let component: AddOpenDiscountComponent;
  let fixture: ComponentFixture<AddOpenDiscountComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddOpenDiscountComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddOpenDiscountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
