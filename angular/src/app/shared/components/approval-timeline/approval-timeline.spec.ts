import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApprovalTimeline } from './approval-timeline';

describe('ApprovalTimeline', () => {
  let component: ApprovalTimeline;
  let fixture: ComponentFixture<ApprovalTimeline>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApprovalTimeline]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ApprovalTimeline);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
