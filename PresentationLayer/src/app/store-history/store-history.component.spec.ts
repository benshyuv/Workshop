import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StoreHistoryComponent } from './store-history.component';

describe('StoreHistoryComponent', () => {
  let component: StoreHistoryComponent;
  let fixture: ComponentFixture<StoreHistoryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StoreHistoryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StoreHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
