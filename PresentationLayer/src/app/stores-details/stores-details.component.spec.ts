import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StoresDetailsComponent } from './stores-details.component';

describe('StoresDetailsComponent', () => {
  let component: StoresDetailsComponent;
  let fixture: ComponentFixture<StoresDetailsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StoresDetailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StoresDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
