/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { ComnSettingsService, Theme, ComnAuthQuery } from '@crucible/common';
import { interval, Observable, Subject } from 'rxjs';
import { shareReplay, take, takeUntil, tap } from 'rxjs/operators';
import { DialogService } from 'src/app/services/dialog/dialog.service';
import { EventTemplatesService } from 'src/app/services/event-templates/event-templates.service';
import { EventsService } from 'src/app/services/events/events.service';
import { Event } from '../../../generated/alloy.api/model/event';
import { EventTemplate } from '../../../generated/alloy.api/model/event-template';

@Component({
  selector: 'app-event-info',
  templateUrl: './event-info.component.html',
  styleUrls: ['./event-info.component.scss'],
})
export class EventInfoComponent implements OnInit, OnDestroy {
  @Input() eventTemplateId: string;

  readonly ONE_HOUR = 1000 * 3600;

  public impsDataSource: MatTableDataSource<Event>;
  public displayedColumns: string[] = [
    'username',
    'status',
    'lastLaunchStatus',
    'lastEndStatus',
    'dateCreated',
    'endDate',
    'statusDate',
  ];

  public eventTemplate$: Observable<EventTemplate>;
  public events$: Observable<Event[]>;
  public currentEvent: Event;
  public isLoading: boolean;
  public eventStatus: string;
  public pollingIntervalMS: number;
  public remainingTime: string;
  public timeRunningLow: boolean;
  public redeploying: boolean;
  public failureMessage: string;
  public failureDate: Date;
  public timer$: Observable<number>;
  public destroyTimer$ = new Subject<boolean>();
  public Event = Event;
  public theme$: Observable<Theme>;

  private failedEvent: Event;
  private isNewLaunch = false;

  constructor(
    private settingsService: ComnSettingsService,
    private dialogService: DialogService,
    public eventTemplatesService: EventTemplatesService,
    public eventsService: EventsService,
    private authQuery: ComnAuthQuery
  ) {

    this.theme$ = this.authQuery.userTheme$;

    this.impsDataSource = new MatTableDataSource<Event>(new Array<Event>());
    this.isLoading = true;
    this.remainingTime = '';
    this.timeRunningLow = false;
    this.eventStatus = '';
    this.failureMessage = '';
    this.failedEvent = undefined;
    this.pollingIntervalMS = parseInt(
      this.settingsService.settings.PollingIntervalMS,
      10
    );

    this.eventTemplate$ = this.eventTemplatesService.currentEventTemplate$;

    this.events$ = this.eventsService.currentEvents$.pipe(
      tap((imps) => (this.eventStatus = this.determineEventStatus(imps))),
      shareReplay(1)
    );

    this.timer$ = interval(this.pollingIntervalMS).pipe(
      tap((tick) => this.eventsService.updateEvents(tick)),
      takeUntil(this.destroyTimer$)
    );
  }

  ngOnInit() {
    this.eventTemplatesService.getEventTemplate(this.eventTemplateId);
    this.timer$.subscribe();
    this.events$.subscribe();
    this.eventsService.updateEvents();
  }

  ngOnDestroy(): void {
    this.destroyTimer$.next();
    this.destroyTimer$.complete();
  }

  determineEventStatus(imps: Event[]): string {
    this.impsDataSource.data = imps.sort((d1, d2) => {
      return +new Date(d2.dateCreated) - +new Date(d1.dateCreated);
    });
    this.isLoading = false;
    // There are 3 states that an event can be in
    // EventReadyToLaunch
    // EventLaunchInProgress
    // EventActive
    let status = '';
    if (imps.length === 0) {
      // No events found
      status = 'EventReadyToLaunch';
      this.currentEvent = undefined;
      this.remainingTime = '';
    } else {
      const actives = imps.find((s) => s.status === Event.StatusEnum.Active);
      if (actives !== undefined) {
        // Active Lab exit now
        status = 'EventActive';
        this.currentEvent = actives;
        this.remainingTime = this.calculateRemainingTime(
          new Date(this.currentEvent.expirationDate)
        );
      } else {
        // No active Events, now check if anything is in progress
        const inProgress = imps.find(
          (s) =>
            s.status === Event.StatusEnum.Creating ||
            s.status === Event.StatusEnum.Planning ||
            s.status === Event.StatusEnum.Applying ||
            s.status === Event.StatusEnum.Ending
        );
        if (inProgress !== undefined) {
          status = 'EventLaunchInProgress';
          this.currentEvent = inProgress;
          this.remainingTime = '';
        } else {
          // At this point, the event is not active and not in progress
          // therefore is must be ready to be launched
          status = 'EventReadyToLaunch';
          this.currentEvent = undefined;
          this.remainingTime = '';
          if (this.isIframe()) {
            // At this point the app is shown within Player therefore the parent must moved to Alloy event page.
            window.top.location.href = window.location.href;
          }
        }
      }
      this.processFailureStatus(imps[0]);
    }

    return status;
  }

  launchEvent() {
    this.eventStatus = 'EventLaunchInProgress';
    this.failedEvent = undefined;
    this.failureMessage = '';
    this.eventsService.launchEvent();
    this.isNewLaunch = true;
  }

  rejoinEvent() {
    console.log('Opening ' + this.currentEvent.name + ' inside Player!!!');
    window.location.href =
      this.settingsService.settings.PlayerUIAddress +
      '/view/' +
      this.currentEvent.viewId;
  }

  endEvent() {
    if (this.currentEvent) {
      this.dialogService
        .confirm('End Event', 'Are you sure that you want to end this event?')
        .pipe(take(1))
        .subscribe((result) => {
          if (result['confirm']) {
            this.isNewLaunch = true;
            console.log('Ending ' + this.currentEvent.name);
            this.eventStatus = 'EventLaunchInProgress';
            this.eventsService.endEvent(this.currentEvent.id);
          }
        });
    }
  }

  redeployEvent() {
    if (this.currentEvent) {
      this.dialogService
        .confirm(
          'Redeploy Event',
          'Are you sure that you want to redeploy this event?'
        )
        .pipe(take(1))
        .subscribe((result) => {
          if (result['confirm']) {
            console.log('Redeploying ' + this.currentEvent.name);
            this.isNewLaunch = true;
            this.redeploying = true;
            this.eventsService.redeployEvent(this.currentEvent.id);
          }
        });
    }
  }

  isIframe(): boolean {
    if (window.location !== window.parent.location) {
      // The page is in an iframe
      return true;
    } else {
      // The page is not in an iframe
      return false;
    }
  }

  calculateRemainingTime(expirationDate: Date): string {
    let timeLeft = '';

    if (expirationDate !== undefined) {
      const now = new Date();
      // Note:  A C# date time is different and when parsing againt the timezone must be added.
      const exp = new Date(
        Date.parse(expirationDate.toLocaleString()).valueOf() -
          now.getTimezoneOffset() * 60 * 1000
      );
      let diffInMs: number =
        exp.valueOf() - Date.parse(now.toISOString()).valueOf();
      if (diffInMs < 0) {
        diffInMs = 0; // Force to zero.  Do not display a negative time.
      }
      const modHrs = (diffInMs / this.ONE_HOUR) % 1;
      const diffInHrs: number = diffInMs / (1000 * 3600) - modHrs;
      const modMins = Math.floor(modHrs * 60);
      timeLeft =
        'Time Remaining:  ' +
        diffInHrs.toString() +
        ' hrs ' +
        modMins +
        ' mins';

      this.timeRunningLow = diffInMs < this.ONE_HOUR;
    }
    return timeLeft;
  }

  processFailureStatus(imp: Event) {
    if (imp) {
      if (
        (imp.status === Event.StatusEnum.Failed ||
          imp.lastLaunchInternalStatus) &&
        !this.failedEvent
      ) {
        // Failed event and endEvent not sent yet
        this.failureDate = imp.dateCreated;
        if (imp.lastLaunchInternalStatus) {
          this.failureMessage = imp.lastLaunchInternalStatus
            .toString()
            .replace(/([A-Z])/g, ' $1')
            .trim();
        } else {
          this.failureMessage = imp.internalStatus
            .toString()
            .replace(/([A-Z])/g, ' $1')
            .trim();
        }
        this.failedEvent = imp;
      }
    } else {
      this.failureMessage = '';
      this.failureDate = undefined;
    }
  }
}
