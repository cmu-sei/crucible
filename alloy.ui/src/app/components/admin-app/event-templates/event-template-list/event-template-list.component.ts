/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, Input, OnDestroy, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { Observable } from 'rxjs/Observable';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs/Subject';
import {
  Directory,
  EventTemplate,
  ScenarioTemplate,
  View,
} from 'src/app/generated/alloy.api';
import { EventTemplatesService } from 'src/app/services/event-templates/event-templates.service';
import { EventTemplateEditComponent } from '../event-template-edit/event-template-edit.component';
import { ComnSettingsService } from '@crucible/common';

@Component({
  selector: 'app-event-template-list',
  templateUrl: './event-template-list.component.html',
  styleUrls: ['./event-template-list.component.scss'],
})
export class EventTemplateListComponent implements OnDestroy {
  @ViewChild(EventTemplateEditComponent, { static: true })
  eventTemplateEditComponent: EventTemplateEditComponent;
  @Input() viewList: Observable<View[]>;
  @Input() directoryList: Observable<Directory[]>;
  @Input() scenarioTemplateList: Observable<ScenarioTemplate[]>;
  displayedColumns: string[] = ['name', 'description', 'durationHours'];
  editEventTemplateText = 'Edit Event Template';
  searchControl: FormControl = this.eventTemplatesService.searchControl$;
  eventTemplates: EventTemplate[] = [];
  isLoading = true;
  sortValue = { active: 'dateCreated', direction: 'desc' };
  // MatPaginator Output
  defaultPageSize = 10;
  pageEvent: PageEvent;
  pageEvents$: Observable<PageEvent>;
  private unsubscribe$ = new Subject();
  topBarColor = '#719F94';
  topBarTextColor = '#FFFFFF';

  constructor(
    public eventTemplatesService: EventTemplatesService,
    private settingsService: ComnSettingsService
  ) {
    this.eventTemplatesService.eventTemplateList$
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((defs) => {
        this.eventTemplates = defs;
        this.sortData(this.sortValue);
      });
    this.isLoading = false;
    // Set the topbar color from config file
    this.topBarColor = this.settingsService.settings.AppTopBarHexColor ? this.settingsService.settings.AppTopBarHexColor : this.topBarColor;
    this.topBarTextColor = this.settingsService.settings.AppTopBarHexTextColor ? this.settingsService.settings.AppTopBarHexTextColor : this.topBarTextColor;
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

  clearFilter() {
    this.searchControl.setValue('');
  }

  sortData(value: any) {
    this.sortValue = value;
    const sortOrder = value.direction === 'desc' ? -1 : 1;
    if (value.active === 'dateCreated') {
      this.eventTemplates.sort((a, b) => {
        if (a.dateCreated < b.dateCreated) {
          return -1 * sortOrder;
        } else if (a.dateCreated > b.dateCreated) {
          return 1 * sortOrder;
        } else {
          return 0;
        }
      });
    } else {
      this.eventTemplates.sort(
        (a, b) => a.name.localeCompare(b.name) * sortOrder
      );
    }
  }

  addNewEventTemplate() {
    const eventTemplate = <EventTemplate>{
      name: 'New Event Template',
      description: 'Add description',
    };
    this.eventTemplatesService.addNew(eventTemplate);
  }

  togglePanel(eventTemplate: EventTemplate) {
    if (
      this.eventTemplatesService.selectedEventTemplateId === eventTemplate.id
    ) {
      this.eventTemplatesService.selectedEventTemplateId = undefined;
    } else {
      this.eventTemplatesService.selectedEventTemplateId = eventTemplate.id;
    }
  }
}
