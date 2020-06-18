/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/
import { Injectable } from '@angular/core';
import { EventService, Event } from 'src/app/generated/alloy.api';
import { Observable, BehaviorSubject, combineLatest, Subject } from 'rxjs';
import { take, switchMap, tap } from 'rxjs/operators';
import { EventTemplatesService } from '../event-templates/event-templates.service';

@Injectable({
  providedIn: 'root'
})
export class EventsService {

  public currentEvents$: Observable<Event[]>;

  private currentEventTemplateId: string;
  private updateTick$ = new Subject<number>();

  constructor(
    public eventTemplatesService: EventTemplatesService,
    public eventService: EventService
  ) {

    this.currentEvents$ = combineLatest(this.eventTemplatesService.currentEventTemplate$, this.updateTick$).pipe(
      switchMap(([def]) => {
        if (def) {
          this.currentEventTemplateId = def.id;
          return this.eventService.getMyEventTemplateEvents(def.id);
        }
      }),
    );
  }

  updateEvents(tickNum: number = -1) {
    console.log('Tick');
    this.updateTick$.next(tickNum);
  }

  launchEvent() {
    this.eventService.createEventFromEventTemplate(this.currentEventTemplateId).pipe(
      take(1),
      tap(() => this.updateEvents())
    ).subscribe();
  }

  endEvent(id: string) {
    this.eventService.endEvent(id).pipe(
      take(1),
      tap(() => this.updateEvents())
    ).subscribe();
  }

  redeployEvent(id: string) {
    this.eventService.redeployEvent(id).pipe(
      take(1),
      tap(() => this.updateEvents())
    ).subscribe();
  }
}
