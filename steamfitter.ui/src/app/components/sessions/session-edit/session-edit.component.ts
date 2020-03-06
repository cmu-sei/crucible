/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, EventEmitter, Input, Output, NgZone, ViewChild } from '@angular/core';
import { ErrorStateMatcher } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { Exercise, PlayerService,
          Session, SessionService } from '../../../swagger-codegen/dispatcher.api';
import { DialogService } from '../../../services/dialog/dialog.service';
import { TaskTreeComponent, DispatchTaskSourceType, DispatchTaskSource } from '../../tasks/task-tree/task-tree.component';
import { Subject, ReplaySubject } from 'rxjs';

@Component({
  selector: 'app-session-edit',
  templateUrl: './session-edit.component.html',
  styleUrls: ['./session-edit.component.css']
})
export class SessionEditComponent implements OnInit {

  @Input() session: Session;
  @Input() openedAsDialog: boolean;
  @Output() editComplete = new EventEmitter<boolean>();
  @ViewChild(SessionEditComponent) child: SessionEditComponent;
  @ViewChild(TaskTreeComponent) taskTree: TaskTreeComponent;

  public sessionNameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(4)
  ]);
  public descriptionFormControl = new FormControl('', [
    Validators.required
  ]);
  public startDateFormControl = new FormControl('', []);
  public startTimeFormControl = new FormControl('', []);
  public endDateFormControl = new FormControl('', []);
  public endTimeFormControl = new FormControl('', []);

  public sessionStates = Object.values(Session.StatusEnum);
  public matcher = new UserErrorStateMatcher();
  public exercises: Exercise[];
  public selectedExerciseId: any;
  public dispatchTaskSource: DispatchTaskSource;
  public changesWereMade = false;

  constructor(
    public sessionService: SessionService,
    public dialogService: DialogService,
    public zone: NgZone,
    public playerService: PlayerService
  ) {

  }

  /**
   * Initialize component
   */
  ngOnInit() {
    this.session.startDate = new Date(this.session.startDate);
    this.session.endDate = new Date(this.session.endDate);
    if (this.session.status === 'active') {
      this.dispatchTaskSource = {'type': DispatchTaskSourceType.SessionActive, 'id': this.session.id};
    } else if (this.session.status === 'ended') {
      this.dispatchTaskSource = {'type': DispatchTaskSourceType.SessionEnded, 'id': this.session.id};
    } else {
      this.dispatchTaskSource = {'type': DispatchTaskSourceType.Session, 'id': this.session.id};
    }
    this.playerService.getExercises().subscribe(exercises => {
      this.exercises = exercises.sort((x1, x2) => {
        return (x1.name > x2.name) ? 1 : ((x1.name < x2.name) ? -1 : 0);
      });
    },
    error => {
      console.log('The Player API is not responding.  ' + error.message);
    });
  }

  /**
   * Closes the edit screen
   */
  returnToSessionList(changesWereMade: boolean): void {
    this.editComplete.emit(changesWereMade || this.changesWereMade);
  }


  /**
   * Delete a session after confirmation
   */
  deleteSession(): void {
    this.dialogService.confirm('Delete Session', 'Are you sure that you want to delete session ' + this.session.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.sessionService.deleteSession(this.session.id)
            .subscribe(deleted => {
              console.log('successfully deleted session');
              this.returnToSessionList(true);
            });
        }
      });
  }

  /**
   * Copy a session after confirmation
   */
  copySession(): void {
    this.dialogService.confirm('Copy Session', 'Are you sure that you want to copy session ' + this.session.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.sessionService.copySession(this.session.id)
            .subscribe(newSession => {
              console.log('successfully copied session');
              this.returnToSessionList(true);
            });
        }
      });
  }

  /**
   * Start a session
   */
  startSession(): void {
    this.dialogService.confirm('Start Session Now', 'Are you sure that you want to start session ' + this.session.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.sessionService.startSession(this.session.id)
            .subscribe(session => {
              console.log('successfully started session ' + session.id);
              this.returnToSessionList(true);
            });
        }
      });
  }

  /**
   * End a session
   */
  endSession(): void {
    this.dialogService.confirm('End Session Now', 'Are you sure that you want to end session ' + this.session.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.sessionService.endSession(this.session.id)
            .subscribe(session => {
              console.log('successfully ended session ' + session.id);
              this.returnToSessionList(true);
            });
        }
      });
  }

  /**
   * Saves the current session
   */
  saveSession(changedField): void {
    let shouldUpdate = true;
    switch (changedField) {
      case 'name':
        if (!this.sessionNameFormControl.hasError('minlength') && !this.sessionNameFormControl.hasError('required')
            && this.session.name !== this.sessionNameFormControl.value) {
          this.session.name = this.sessionNameFormControl.value;
        } else {
          shouldUpdate = false;
        }
        break;
      case 'description':
        if (!this.descriptionFormControl.hasError('required') && this.session.description !== this.descriptionFormControl.value) {
          this.session.description = this.descriptionFormControl.value;
        } else {
            shouldUpdate = false;
        }
        break;
      case 'exercise':
        const exercise = this.exercises.find(ex => ex.id === this.session.exerciseId);
        this.session.exercise = exercise ? exercise.name : '';
        break;
      case 'startDate':
        if (this.session.startDate.toLocaleDateString() !== this.startDateFormControl.value) {
          const newDate = new Date(this.startDateFormControl.value);
          const oldDate = new Date(this.session.startDate);
          newDate.setHours(oldDate.getHours());
          newDate.setMinutes(oldDate.getMinutes());
          this.session.startDate = newDate;
          const newEndDate = this.session.endDate.getTime() + (newDate.getTime() - oldDate.getTime());
          this.session.endDate.setTime(newEndDate);
        } else {
          shouldUpdate = false;
        }
        break;
      case 'startTime':
        if ( this.startTimeFormControl.value.length === 5
            && (this.session.startDate.getHours() !== this.startTimeFormControl.value.substr(0, 2)
                || this.session.startDate.getMinutes() !== this.startTimeFormControl.value.substr(2, 2))) {
          const timeParts = this.startTimeFormControl.value.split(':');
          const oldDate = new Date(this.session.startDate);
          this.session.startDate.setHours(timeParts[0]);
          this.session.startDate.setMinutes(timeParts[1]);
          const newEndDate = this.session.endDate.getTime() + (this.session.startDate.getTime() - oldDate.getTime());
          this.session.endDate.setTime(newEndDate);
        } else {
          shouldUpdate = false;
        }
        break;
      case 'endDate':
        if (this.session.endDate.toLocaleDateString() !== this.endDateFormControl.value) {
          const newDate = new Date(this.endDateFormControl.value);
          const oldDate = new Date(this.session.endDate);
          newDate.setHours(oldDate.getHours());
          newDate.setMinutes(oldDate.getMinutes());
          this.session.endDate = newDate;
        } else {
          shouldUpdate = false;
        }
        break;
      case 'endTime':
        if ( this.endTimeFormControl.value.length === 5
            && (this.session.endDate.getHours() !== this.endTimeFormControl.value.substr(0, 2)
                || this.session.endDate.getMinutes() !== this.endTimeFormControl.value.substr(2, 2))) {
          const timeParts = this.endTimeFormControl.value.split(':');
          this.session.endDate.setHours(timeParts[0]);
          this.session.endDate.setMinutes(timeParts[1]);
        } else {
          shouldUpdate = false;
        }
        break;
      default:
        break;
    }
    if (shouldUpdate) {
      this.changesWereMade = true;
      this.sessionService.updateSession(this.session.id, this.session).subscribe(updatedSession => {
        updatedSession.startDate = new Date(updatedSession.startDate);
        updatedSession.endDate = new Date(updatedSession.endDate);
        this.session = updatedSession;
      });

    }
  }


} // End Class


/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

