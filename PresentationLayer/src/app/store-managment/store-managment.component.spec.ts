import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StoreManagmentComponent } from './store-managment.component';

describe('StoreManagmentComponent', () => {
  let component: StoreManagmentComponent;
  let fixture: ComponentFixture<StoreManagmentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StoreManagmentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StoreManagmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
