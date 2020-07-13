import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddStoreConditionalDiscountComponent } from './add-store-conditional-discount.component';

describe('AddStoreConditionalDiscountComponent', () => {
  let component: AddStoreConditionalDiscountComponent;
  let fixture: ComponentFixture<AddStoreConditionalDiscountComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddStoreConditionalDiscountComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddStoreConditionalDiscountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
