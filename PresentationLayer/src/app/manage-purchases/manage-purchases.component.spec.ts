import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ManagePurchasesComponent } from './manage-purchases.component';

describe('ManagePurchasesComponent', () => {
  let component: ManagePurchasesComponent;
  let fixture: ComponentFixture<ManagePurchasesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ManagePurchasesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ManagePurchasesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
