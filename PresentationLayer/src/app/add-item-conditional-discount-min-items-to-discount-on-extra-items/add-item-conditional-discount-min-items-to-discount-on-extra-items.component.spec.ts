import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddItemConditionalDiscountMinItemsToDiscountOnExtraItemsComponent } from './add-item-conditional-discount-min-items-to-discount-on-extra-items.component';

describe('AddItemConditionalDiscountMinItemsToDiscountOnExtraItemsComponent', () => {
  let component: AddItemConditionalDiscountMinItemsToDiscountOnExtraItemsComponent;
  let fixture: ComponentFixture<AddItemConditionalDiscountMinItemsToDiscountOnExtraItemsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddItemConditionalDiscountMinItemsToDiscountOnExtraItemsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddItemConditionalDiscountMinItemsToDiscountOnExtraItemsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
