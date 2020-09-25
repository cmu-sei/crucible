/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {
  Component,
  EventEmitter,
  Input,
  NgZone,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import {
  FormControl,
  FormGroupDirective,
  NgForm,
  Validators,
} from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
  Directory,
  EventTemplate,
  ScenarioTemplate,
  View,
} from 'src/app/generated/alloy.api';
import { DialogService } from 'src/app/services/dialog/dialog.service';
import { EventTemplatesService } from 'src/app/services/event-templates/event-templates.service';

/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(
    control: FormControl | null,
    form: FormGroupDirective | NgForm | null
  ): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

/** Error when control isn't an integer */
export class NotIntegerErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(
    control: FormControl | null,
    form: FormGroupDirective | NgForm | null
  ): boolean {
    const isNotAnInteger =
      parseInt(control.value, 10).toString() !== control.value;
    if (isNotAnInteger) {
      control.setErrors({ notAnInteger: true });
    }
    const isSubmitted = form && form.submitted;
    return !!(
      control &&
      (control.invalid || isNotAnInteger) &&
      (control.dirty || isSubmitted)
    );
  }
}

@Component({
  selector: 'app-event-template-edit',
  templateUrl: './event-template-edit.component.html',
  styleUrls: ['./event-template-edit.component.scss'],
})
export class EventTemplateEditComponent implements OnInit, OnDestroy {
  @Input() eventTemplate: EventTemplate;
  @Input() viewList: Observable<View[]>;
  @Input() directoryList: Observable<Directory[]>;
  @Input() scenarioTemplateList: Observable<ScenarioTemplate[]>;
  @Output() closePanel = new EventEmitter<boolean>();

  private _viewList: View[] = [];
  private _directoryList: Directory[] = [];
  private _scenarioTemplateList: ScenarioTemplate[] = [];
  private _viewFilter = '';
  private _directoryFilter = '';
  private _scenarioTemplateFilter = '';
  public filteredViewList = new BehaviorSubject<View[]>([]);
  public filteredDirectoryList = new BehaviorSubject<Directory[]>([]);
  public filteredScenarioTemplateList = new BehaviorSubject<ScenarioTemplate[]>(
    []
  );
  public eventTemplateNameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(4),
  ]);
  public descriptionFormControl = new FormControl('', [Validators.required]);
  public durationHoursFormControl = new FormControl('', []);
  public viewIdFormControl = new FormControl('', []);
  public directoryIdFormControl = new FormControl('', []);
  public scenarioTemplateIdFormControl = new FormControl('', []);
  public isPublishedFormControl = new FormControl('', []);
  public useDynamicHostFormControl = new FormControl('', []);
  public matcher = new UserErrorStateMatcher();
  public notAnIntegerErrorState = new NotIntegerErrorStateMatcher();
  public viewSearchControl = new FormControl('', []);
  public directorySearchControl = new FormControl('', []);
  public scenarioTemplateSearchControl = new FormControl('', []);
  private unsubscribe$ = new Subject();

  constructor(
    public eventTemplatesService: EventTemplatesService,
    public dialogService: DialogService,
    public zone: NgZone
  ) {}

  /**
   * Initialize component
   */
  ngOnInit() {
    this.viewList.pipe(takeUntil(this.unsubscribe$)).subscribe((views) => {
      this._viewList = views;
      this.viewSearchControl.setValue(this.viewSearchControl.value);
    });
    this.directoryList
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((directories) => {
        this._directoryList = directories;
        this.directorySearchControl.setValue(this.directorySearchControl.value);
      });
    this.scenarioTemplateList
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((scenarioTemplates) => {
        this._scenarioTemplateList = scenarioTemplates;
        this.scenarioTemplateSearchControl.setValue(
          this.scenarioTemplateSearchControl.value
        );
      });
    this.viewSearchControl.valueChanges
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((filterTerm) => {
        this._viewFilter = filterTerm;
        this.filterViews();
      });
    this.directorySearchControl.valueChanges
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((filterTerm) => {
        this._directoryFilter = filterTerm;
        this.filterDirectories();
      });
    this.scenarioTemplateSearchControl.valueChanges
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((filterTerm) => {
        this._scenarioTemplateFilter = filterTerm;
        this.filterScenarioTemplates();
      });
    this.eventTemplateNameFormControl.setValue(this.eventTemplate.name);
    this.descriptionFormControl.setValue(this.eventTemplate.description);
    this.durationHoursFormControl.setValue(this.eventTemplate.durationHours);
    this.viewIdFormControl.setValue(this.eventTemplate.viewId);
    this.directoryIdFormControl.setValue(this.eventTemplate.directoryId);
    this.scenarioTemplateIdFormControl.setValue(
      this.eventTemplate.scenarioTemplateId
    );
    this.isPublishedFormControl.setValue(this.eventTemplate.isPublished);
    this.useDynamicHostFormControl.setValue(this.eventTemplate.useDynamicHost);
  }

  /**
   * Delete an event template after confirmation
   */
  deleteEventTemplate(): void {
    this.dialogService
      .confirm(
        'Delete Event Template',
        'Are you sure that you want to delete Event Template ' +
          this.eventTemplate.name +
          '?'
      )
      .subscribe((result) => {
        if (result['confirm']) {
          this.eventTemplatesService.delete(this.eventTemplate.id);
          this.closePanel.emit(true);
        }
      });
  }

  /**
   * Saves the current event template Ids
   */
  saveEventIds(event, changedField): void {
    let shouldUpdate = false;
    switch (changedField) {
      case 'viewId':
        if (this.eventTemplate.viewId !== event.option.value) {
          this.eventTemplate.viewId = event.option.value;
          shouldUpdate = true;
        }
        this.viewSearchControl.setValue('');
        break;
      case 'directoryId':
        if (this.eventTemplate.directoryId !== event.option.value) {
          this.eventTemplate.directoryId = event.option.value;
          shouldUpdate = true;
        }
        this.directorySearchControl.setValue('');
        break;
      case 'scenarioTemplateId':
        if (this.eventTemplate.scenarioTemplateId !== event.option.value) {
          this.eventTemplate.scenarioTemplateId = event.option.value;
          shouldUpdate = true;
        }
        this.scenarioTemplateSearchControl.setValue('');
        break;
      default:
        break;
    }
    if (shouldUpdate) {
      this.eventTemplatesService.update(this.eventTemplate);
    }
  }

  /**
   * Saves the current event template
   */
  saveEventTemplate(changedField): void {
    let shouldUpdate = false;
    switch (changedField) {
      case 'name':
        if (
          !this.eventTemplateNameFormControl.hasError('minlength') &&
          !this.eventTemplateNameFormControl.hasError('required') &&
          this.eventTemplate.name !== this.eventTemplateNameFormControl.value
        ) {
          this.eventTemplate.name = this.eventTemplateNameFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'description':
        if (
          this.eventTemplate.description !== this.descriptionFormControl.value
        ) {
          this.eventTemplate.description = this.descriptionFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'durationHours':
        if (
          parseInt(
            this.durationHoursFormControl.value.toString(),
            10
          ).toString() === this.durationHoursFormControl.value.toString() &&
          this.eventTemplate.durationHours !==
            this.durationHoursFormControl.value
        ) {
          this.eventTemplate.durationHours = this.durationHoursFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'isPublished':
        if (
          this.eventTemplate.isPublished !== this.isPublishedFormControl.value
        ) {
          this.eventTemplate.isPublished = this.isPublishedFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'useDynamicHost':
        if (
          this.eventTemplate.useDynamicHost !==
          this.useDynamicHostFormControl.value
        ) {
          this.eventTemplate.useDynamicHost = this.useDynamicHostFormControl.value;
          shouldUpdate = true;
        }
        break;
      default:
        break;
    }
    if (shouldUpdate) {
      this.eventTemplatesService.update(this.eventTemplate);
    }
  }

  filterViews() {
    const filteredList = this._viewList
      .sort((a: View, b: View) =>
        a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1
      )
      .filter(
        (item) =>
          item.name.toLowerCase().includes(this._viewFilter.toLowerCase()) ||
          item.id.toLowerCase().includes(this._viewFilter.toLowerCase())
      );
    this.filteredViewList.next(filteredList);
  }

  filterDirectories() {
    const filteredList = this._directoryList
      .sort((a: Directory, b: Directory) =>
        a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1
      )
      .filter(
        (item) =>
          item.name
            .toLowerCase()
            .includes(this._directoryFilter.toLowerCase()) ||
          item.id.toLowerCase().includes(this._directoryFilter.toLowerCase())
      );
    this.filteredDirectoryList.next(filteredList);
  }

  filterScenarioTemplates() {
    const filteredList = this._scenarioTemplateList
      .sort((a: View, b: View) =>
        a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1
      )
      .filter(
        (item) =>
          item.name
            .toLowerCase()
            .includes(this._scenarioTemplateFilter.toLowerCase()) ||
          item.id
            .toLowerCase()
            .includes(this._scenarioTemplateFilter.toLowerCase())
      );
    this.filteredScenarioTemplateList.next(filteredList);
  }

  get selectedViewName() {
    return (selectedId) => {
      if (!selectedId) {
        selectedId = this.viewIdFormControl.value;
        if (!selectedId) {
          // no selected view
          return '';
        }
      }
      if (this._viewList.some((v) => v.id === selectedId)) {
        // selected view is in the list
        return this._viewList.find((v) => v.id === selectedId).name;
      } else {
        // selected view is not in the current list
        return selectedId;
      }
    };
  }

  get selectedDirectoryName() {
    return (selectedId) => {
      if (!selectedId) {
        selectedId = this.directoryIdFormControl.value;
        if (!selectedId) {
          // no selected directory
          return '';
        }
      }
      if (this._directoryList.some((v) => v.id === selectedId)) {
        // selected directory is in the list
        return this._directoryList.find((v) => v.id === selectedId).name;
      } else {
        // selected directory is not in the current list
        return selectedId;
      }
    };
  }

  get selectedScenarioTemplateName() {
    return (selectedId) => {
      if (!selectedId) {
        selectedId = this.scenarioTemplateIdFormControl.value;
        if (!selectedId) {
          // no selected scenarioTemplate template
          return '';
        }
      }
      if (this._scenarioTemplateList.some((v) => v.id === selectedId)) {
        // selected scenarioTemplate template is in the list
        return this._scenarioTemplateList.find((v) => v.id === selectedId).name;
      } else {
        // selected scenarioTemplate template is not in the current list
        return selectedId;
      }
    };
  }

  /**
   * Clone an event template after confirmation
   */
  cloneEventTemplate(): void {
    this.dialogService
      .confirm(
        'Clone Event Template',
        'Are you sure that you want to create a new Event Template from ' +
          this.eventTemplate.name +
          '?'
      )
      .subscribe((result) => {
        if (result['confirm']) {
          const newEventTemplate = {
            name: this.eventTemplate.name + ' - clone',
            description: this.eventTemplate.description,
            durationHours: this.eventTemplate.durationHours,
            viewId: this.eventTemplate.viewId,
            directoryId: this.eventTemplate.directoryId,
            scenarioTemplateId: this.eventTemplate.scenarioTemplateId,
          };
          this.eventTemplatesService.addNew(newEventTemplate);
          this.closePanel.emit(true);
        }
      });
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
} // End Class
