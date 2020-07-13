import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DiscountsManagementComponent } from './discounts-management.component';

describe('DiscountsManagementComponent', () => {
  let component: DiscountsManagementComponent;
  let fixture: ComponentFixture<DiscountsManagementComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DiscountsManagementComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DiscountsManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
