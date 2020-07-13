import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditItemAmountInCartComponent } from './edit-item-amount-in-cart.component';

describe('EditItemAmountInCartComponent', () => {
  let component: EditItemAmountInCartComponent;
  let fixture: ComponentFixture<EditItemAmountInCartComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditItemAmountInCartComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditItemAmountInCartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
