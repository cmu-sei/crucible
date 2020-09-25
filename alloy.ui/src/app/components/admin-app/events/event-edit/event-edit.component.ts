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
import {
  Event,
  EventService,
  PlayerService,
  View,
} from '../../../../generated/alloy.api';
import { DialogService } from '../../../../services/dialog/dialog.service';

@Component({
  selector: 'app-event-edit',
  templateUrl: './event-edit.component.html',
  styleUrls: ['./event-edit.component.scss'],
})
export class EventEditComponent implements OnInit {
  @Input() event: Event;
  @Output() editComplete = new EventEmitter<boolean>();

  public eventNameFormControl: FormControl;
  public descriptionFormControl: FormControl;
  public launchDateFormControl: FormControl;
  public endDateFormControl: FormControl;
  public expirationDateFormControl: FormControl;
  public statusDateFormControl: FormControl;
  public userIdFormControl: FormControl;
  public usernameFormControl: FormControl;
  public statusFormControl: FormControl;
  public internalStatusFormControl: FormControl;
  public eventTemplateIdFormControl: FormControl;
  public viewIdFormControl: FormControl;
  public workspaceIdFormControl: FormControl;
  public runIdFormControl: FormControl;
  public scenarioIdFormControl: FormControl;

  public eventStates = Object.values(Event.StatusEnum);
  public matcher = new UserErrorStateMatcher();
  public views: View[];
  public selectedViewId: any;
  public changesWereMade = false;

  constructor(
    public eventService: EventService,
    public dialogService: DialogService,
    public zone: NgZone,
    public playerService: PlayerService
  ) {}

  /**
   * Initialize component
   */
  ngOnInit() {
    this.initForm();

    console.log(this.event);
    this.playerService.getViews().subscribe(
      (views) => {
        this.views = views.sort((x1, x2) => {
          return x1.name > x2.name ? 1 : x1.name < x2.name ? -1 : 0;
        });
      },
      (error) => {
        console.log('The Player API is not responding.  ' + error.message);
      }
    );

    this.setFormDisabled();
  }

  private initForm() {
    this.eventNameFormControl = new FormControl(this.event.name, [
      Validators.required,
      Validators.minLength(4),
    ]);
    this.descriptionFormControl = new FormControl(this.event.description, [
      Validators.required,
    ]);
    this.launchDateFormControl = new FormControl(this.event.launchDate, []);
    this.endDateFormControl = new FormControl(this.event.endDate, []);
    this.expirationDateFormControl = new FormControl(
      this.event.expirationDate,
      []
    );
    this.statusDateFormControl = new FormControl(this.event.statusDate, []);
    this.userIdFormControl = new FormControl(this.event.userId, [
      Validators.required,
    ]);
    this.usernameFormControl = new FormControl(this.event.username, [
      Validators.required,
    ]);
    this.statusFormControl = new FormControl(this.event.status, [
      Validators.required,
    ]);
    this.internalStatusFormControl = new FormControl(
      this.event.internalStatus,
      [Validators.required]
    );
    this.eventTemplateIdFormControl = new FormControl(
      this.event.eventTemplateId,
      []
    );
    this.viewIdFormControl = new FormControl(this.event.viewId, []);
    this.workspaceIdFormControl = new FormControl(this.event.workspaceId, []);
    this.runIdFormControl = new FormControl(this.event.runId, []);
    this.scenarioIdFormControl = new FormControl(this.event.scenarioId, []);
  }

  private setFormDisabled() {
    this.usernameFormControl.disable();
    this.userIdFormControl.disable();
    this.statusFormControl.disable();
    this.internalStatusFormControl.disable();
    this.eventTemplateIdFormControl.disable();
    this.viewIdFormControl.disable();
    this.workspaceIdFormControl.disable();
    this.runIdFormControl.disable();
    this.scenarioIdFormControl.disable();
    this.launchDateFormControl.disable();
    this.endDateFormControl.disable();
    this.statusDateFormControl.disable();
    if (this.event.status === 'Ended' || this.event.status === 'Failed') {
      this.eventNameFormControl.disable();
      this.descriptionFormControl.disable();
      this.expirationDateFormControl.disable();
    }
  }

  /**
   * Closes the edit screen
   */
  returnToEventList(changesWereMade: boolean): void {
    this.editComplete.emit(changesWereMade || this.changesWereMade);
  }

  /**
   * Delete an event after confirmation
   */
  deleteEvent(): void {
    this.dialogService
      .confirm(
        'Delete Event',
        'Are you sure that you want to delete event ' + this.event.name + '?'
      )
      .subscribe((result) => {
        if (result['confirm']) {
          this.eventService.deleteEvent(this.event.id).subscribe((deleted) => {
            console.log('successfully deleted event');
            this.returnToEventList(true);
          });
        }
      });
  }

  /**
   * End an event
   */
  endEvent(): void {
    this.dialogService
      .confirm(
        'End Event Now',
        'Are you sure that you want to end event ' + this.event.name + '?'
      )
      .subscribe((result) => {
        if (result['confirm']) {
          this.eventService.endEvent(this.event.id).subscribe((event) => {
            console.log('successfully ended event ' + event.id);
            this.returnToEventList(true);
          });
        }
      });
  }

  /**
   * Saves the current event
   */
  saveEvent(changedField): void {
    let shouldUpdate = false;
    switch (changedField) {
      case 'name':
        if (
          !this.eventNameFormControl.hasError('minlength') &&
          !this.eventNameFormControl.hasError('required') &&
          this.event.name !== this.eventNameFormControl.value
        ) {
          this.event.name = this.eventNameFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'description':
        if (
          !this.descriptionFormControl.hasError('required') &&
          this.event.description !== this.descriptionFormControl.value
        ) {
          this.event.description = this.descriptionFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'viewId':
        if (
          !this.viewIdFormControl.hasError('required') &&
          this.event.viewId !== this.viewIdFormControl.value
        ) {
          this.event.viewId = this.viewIdFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'launchDate':
        if (
          this.event.launchDate.toLocaleDateString() !==
          this.launchDateFormControl.value
        ) {
          if (this.launchDateFormControl.value > '') {
            this.event.launchDate = new Date(this.launchDateFormControl.value);
          } else {
            this.event.launchDate = null;
          }
          shouldUpdate = true;
        }
        break;
      case 'endDate':
        if (
          this.event.endDate.toLocaleDateString() !==
          this.endDateFormControl.value
        ) {
          if (this.endDateFormControl.value > '') {
            this.event.endDate = new Date(this.endDateFormControl.value);
          } else {
            this.event.endDate = null;
          }
          shouldUpdate = true;
        }
        break;
      case 'expirationDate':
        if (
          this.event.expirationDate.toLocaleDateString() !==
          this.expirationDateFormControl.value
        ) {
          if (this.expirationDateFormControl.value > '') {
            this.event.expirationDate = new Date(
              this.expirationDateFormControl.value
            );
          } else {
            this.event.expirationDate = null;
          }
          shouldUpdate = true;
        }
        break;
      case 'statusDate':
        if (
          this.event.statusDate.toLocaleDateString() !==
          this.statusDateFormControl.value
        ) {
          if (this.statusDateFormControl.value > '') {
            this.event.statusDate = new Date(this.statusDateFormControl.value);
          } else {
            this.event.statusDate = null;
          }
          shouldUpdate = true;
        }
        break;
      default:
        break;
    }
    if (shouldUpdate) {
      this.changesWereMade = true;
      this.eventService
        .updateEvent(this.event.id, this.event)
        .subscribe((updatedEvent) => {
          updatedEvent.launchDate = !updatedEvent.launchDate
            ? null
            : new Date(updatedEvent.launchDate);
          updatedEvent.endDate = !updatedEvent.endDate
            ? null
            : new Date(updatedEvent.endDate);
          updatedEvent.expirationDate = !updatedEvent.expirationDate
            ? null
            : new Date(updatedEvent.expirationDate);
          updatedEvent.statusDate = !updatedEvent.statusDate
            ? null
            : new Date(updatedEvent.statusDate);
          this.event = updatedEvent;
        });
    }
  }
} // End Class

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
