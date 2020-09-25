/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Observable } from 'rxjs/Observable';
import { of } from 'rxjs/observable/of';
import { map } from 'rxjs/operators';
import { Subject } from 'rxjs/Subject';
import {
  fromMatPaginator,
  fromMatSort,
  paginateRows,
  sortRows,
} from 'src/app/datasource-utils';
import { Event, EventService } from 'src/app/generated/alloy.api';
import { DialogService } from 'src/app/services/dialog/dialog.service';
import { EventEditComponent } from '../event-edit/event-edit.component';
import { ComnSettingsService } from '@crucible/common';

export interface Action {
  Value: string;
  Text: string;
}

@Component({
  selector: 'app-admin-event-list',
  templateUrl: './event-list.component.html',
  styleUrls: ['./event-list.component.scss'],
})
export class AdminEventListComponent implements OnInit {
  displayedColumns: string[] = [
    'name',
    'username',
    'status',
    'statusDate',
    'launchDate',
    'expirationDate',
  ];
  filterString: string;

  editEventText = 'Edit Event';
  eventToEdit: Event;
  eventDataSource = new MatTableDataSource<Event>(new Array<Event>());
  activeEvents = new Array<Event>();
  failedEvents = new Array<Event>();
  endedEvents = new Array<Event>();
  showActive = true;
  showFailed = false;
  showEnded = false;
  topBarColor = '#719F94';
  topBarTextColor = '#FFFFFF';
  // MatPaginator Output
  defaultPageSize = 10;
  pageEvent: PageEvent;
  isLoading: Boolean;
  displayedRows$: Observable<Event[]>;
  totalRows$: Observable<number>;
  sortEvents$: Observable<Sort>;
  pageEvents$: Observable<PageEvent>;

  @Input() refresh: Subject<boolean>;
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  @ViewChild(EventEditComponent, { static: true })
  eventEditComponent: EventEditComponent;

  constructor(
    private eventService: EventService,
    public dialogService: DialogService,
    private dialog: MatDialog,
    private settingsService: ComnSettingsService
  ) {
    // Set the topbar color from config file
    this.topBarColor = this.settingsService.settings.AppTopBarHexColor ? this.settingsService.settings.AppTopBarHexColor : this.topBarColor;
    this.topBarTextColor = this.settingsService.settings.AppTopBarHexTextColor ? this.settingsService.settings.AppTopBarHexTextColor : this.topBarTextColor;
  }

  /**
   * Initialization
   */
  ngOnInit() {
    this.sortEvents$ = fromMatSort(this.sort);
    this.pageEvents$ = fromMatPaginator(this.paginator);
    // this.eventDataSource.filterPredicate = (data: Event, filterString: string) => {
    //   return this.customEventFilter(data, filterString);
    // };
    this.refresh.subscribe((shouldRefresh) => {
      if (shouldRefresh) {
        this.refreshEvents();
      }
    });
    this.refreshEvents();
  }

  /**
   * Defines the custom filterPredicate to filter the events
   */
  // customEventFilter(data: Event, filterString: string) {
  //   return data.status.toLowerCase().includes(filterString.toLowerCase());
  // }

  /**
   * Called by UI to add a filter to the viewDataSource
   * @param filterValue
   */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.eventDataSource.filter = filterValue;
    this.filterAndSort();
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

  /**
   * Refreshes the events list and updates the mat table control
   */
  refreshEvents() {
    this.isLoading = true;
    this.eventToEdit = undefined;
    this.eventService.getEvents().subscribe((events) => {
      this.activeEvents.length = 0;
      this.endedEvents.length = 0;
      this.failedEvents.length = 0;
      events.forEach((event) => {
        event.launchDate = !event.launchDate
          ? null
          : new Date(event.launchDate + 'Z');
        event.endDate = !event.endDate ? null : new Date(event.endDate + 'Z');
        event.expirationDate = !event.expirationDate
          ? null
          : new Date(event.expirationDate + 'Z');
        event.statusDate = !event.statusDate
          ? null
          : new Date(event.statusDate + 'Z');
        switch (event.status) {
          case 'Failed': {
            this.failedEvents.push(event);
            break;
          }
          case 'Ended':
          case 'Expired': {
            this.endedEvents.push(event);
            break;
          }
          default: {
            this.activeEvents.push(event);
            break;
          }
        }
      });
      this.filterAndSort();
      this.isLoading = false;
    });
  }

  /**
   * filters and sorts the displayed rows
   */
  filterAndSort() {
    this.eventDataSource.data = this.selectEvents();
    const rows$ = of(this.eventDataSource.filteredData);
    this.totalRows$ = rows$.pipe(map((rows) => rows.length));
    this.displayedRows$ = rows$.pipe(
      sortRows(this.sortEvents$),
      paginateRows(this.pageEvents$)
    );
  }

  /**
   * filters the events by status (active, ended, failed)
   */
  selectEvents() {
    let selectedEvents = new Array<Event>();
    if (this.showActive) {
      selectedEvents = selectedEvents.concat(this.activeEvents);
    }
    if (this.showEnded) {
      selectedEvents = selectedEvents.concat(this.endedEvents);
    }
    if (this.showFailed) {
      selectedEvents = selectedEvents.concat(this.failedEvents);
    }
    return selectedEvents;
  }

  /**
   * Executes an action menu item
   * @param action: action string to case from
   * @param eventGuid: The guid for event
   */
  executeEventAction(action: string, eventGuid: string) {
    switch (action) {
      case 'edit': {
        // Edit event
        this.eventService.getEvent(eventGuid).subscribe((event) => {
          const dialogRef = this.dialog.open(EventEditComponent);
          dialogRef.afterOpened().subscribe((r) => {
            event.launchDate = new Date(event.launchDate + 'Z');
            event.endDate = new Date(event.endDate + 'Z');
            dialogRef.componentInstance.event = event;
          });

          dialogRef.componentInstance.editComplete.subscribe((newEvent) => {
            dialogRef.close();
            this.refreshEvents();
            if (!!newEvent) {
              this.executeEventAction('edit', newEvent.id);
            }
          });
        });
        break;
      }
      default: {
        alert('Unknown Action');
        break;
      }
    }
  }

  /**
   * Adds a new event
   */
  addNewEvent() {
    const startDate = new Date();
    startDate.setDate(startDate.getDate() + 1);
    startDate.setHours(8, 0, 0, 0);
    const endDate = new Date(startDate);
    endDate.setMonth(startDate.getMonth() + 1);
    const event = {
      name: 'New Event',
      description: 'Add description',
      status: 'ready',
      startDate: startDate,
      endDate: endDate,
    };
    this.eventService.createEvent(<Event>event).subscribe((ex) => {
      this.refreshEvents();
      this.executeEventAction('edit', ex.id);
    });
  }
}
