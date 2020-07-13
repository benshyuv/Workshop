import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddStoreMinMaxPurchasePolicyComponent } from './add-store-min-max-purchase-policy.component';

describe('AddStoreMinMaxPurchasePolicyComponent', () => {
  let component: AddStoreMinMaxPurchasePolicyComponent;
  let fixture: ComponentFixture<AddStoreMinMaxPurchasePolicyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddStoreMinMaxPurchasePolicyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddStoreMinMaxPurchasePolicyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
