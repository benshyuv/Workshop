import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DiscountsEditPageComponent } from './discounts-edit-page.component';

describe('DiscountsEditPageComponent', () => {
  let component: DiscountsEditPageComponent;
  let fixture: ComponentFixture<DiscountsEditPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DiscountsEditPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DiscountsEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
