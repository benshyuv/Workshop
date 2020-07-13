import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageTypesOfPurchasePolicyComponent } from './manage-types-of-purchase-policy.component';

describe('ManageTypesOfPurchasePolicyComponent', () => {
  let component: ManageTypesOfPurchasePolicyComponent;
  let fixture: ComponentFixture<ManageTypesOfPurchasePolicyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ManageTypesOfPurchasePolicyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ManageTypesOfPurchasePolicyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
