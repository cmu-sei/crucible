/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, EventEmitter, Output, Inject } from '@angular/core';
import { DialogService } from 'src/app/services/dialog/dialog.service';
import { MAT_DIALOG_DATA, ErrorStateMatcher } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { Scenario } from 'src/app/swagger-codegen/dispatcher.api';

/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

@Component({
  selector: 'app-scenario-edit-dialog',
  templateUrl: './scenario-edit-dialog.component.html',
  styleUrls: ['./scenario-edit-dialog.component.css']
})

export class ScenarioEditDialogComponent {

  @Output() editComplete = new EventEmitter<any>();

  public scenarioNameFormControl = new FormControl(this.data.scenario.name, [
    Validators.required,
    Validators.minLength(4)
  ]);
  public descriptionFormControl = new FormControl(this.data.scenario.description ? this.data.scenario.description : ' ', [
    Validators.required,
    Validators.minLength(4)
  ]);
  public startDateFormControl = new FormControl('', []);
  public startTimeFormControl = new FormControl('', []);
  public endDateFormControl = new FormControl('', []);
  public endTimeFormControl = new FormControl('', []);
  public matcher = new UserErrorStateMatcher();

  constructor(
    public dialogService: DialogService,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  errorFree() {
    return !(this.scenarioNameFormControl.hasError('required')
              || this.scenarioNameFormControl.hasError('minlength')
              || this.descriptionFormControl.hasError('required')
              || this.descriptionFormControl.hasError('minlength')
              || !this.data.scenario.exerciseId);
  }

  trimInitialDescription() {
    if (this.descriptionFormControl.value && this.descriptionFormControl.value.toString()[0] === ' ') {
      this.descriptionFormControl.setValue(this.descriptionFormControl.value.toString().trim());
    }
  }

  /**
   * Closes the edit screen
   */
  handleEditComplete(saveChanges: boolean): void {
    if (!saveChanges) {
      this.editComplete.emit({saveChanges: false, scenario: null});
    } else {
      this.data.scenario.name = this.scenarioNameFormControl.value.toString().trim();
      this.data.scenario.description = this.descriptionFormControl.value.toString().trim();
      if (this.errorFree) {
        this.editComplete.emit({saveChanges: saveChanges, scenario: this.data.scenario});
      }
    }
  }

  /**
   * Saves the current scenario
   */
  saveScenario(changedField): void {
    switch (changedField) {
      case 'name':
        this.data.scenario.name = this.scenarioNameFormControl.value.toString();
        break;
      case 'description':
        this.data.scenario.description = this.descriptionFormControl.value.toString();
        break;
      case 'view':
        const view = this.data.views.find(v => v.id === this.data.scenario.exerciseId);
        this.data.scenario.exercise = view ? view.name : '';
        break;
      case 'startDate':
        const newStart = new Date(this.startDateFormControl.value);
        const oldStart = new Date(this.data.scenario.startDate);
        newStart.setHours(oldStart.getHours());
        newStart.setMinutes(oldStart.getMinutes());
        this.data.scenario.startDate = newStart;
        const newEndDate = this.data.scenario.endDate.getTime() + (newStart.getTime() - oldStart.getTime());
        this.data.scenario.endDate.setTime(newEndDate);
        break;
      case 'startTime':
        if ( this.startTimeFormControl.value.length === 5
            && (this.data.scenario.startDate.getHours() !== this.startTimeFormControl.value.substr(0, 2)
                || this.data.scenario.startDate.getMinutes() !== this.startTimeFormControl.value.substr(2, 2))) {
          const timeParts = this.startTimeFormControl.value.split(':');
          const oldDate = new Date(this.data.scenario.startDate);
          this.data.scenario.startDate.setHours(timeParts[0]);
          this.data.scenario.startDate.setMinutes(timeParts[1]);
          const endDate = this.data.scenario.endDate.getTime() + (this.data.scenario.startDate.getTime() - oldDate.getTime());
          this.data.scenario.endDate.setTime(endDate);
        }
        break;
      case 'endDate':
        const newEnd = new Date(this.endDateFormControl.value);
        const oldEnd = new Date(this.data.scenario.endDate);
        newEnd.setHours(oldEnd.getHours());
        newEnd.setMinutes(oldEnd.getMinutes());
        this.data.scenario.endDate = newEnd;
        break;
      case 'endTime':
        if ( this.endTimeFormControl.value.length === 5
            && (this.data.scenario.endDate.getHours() !== this.endTimeFormControl.value.substr(0, 2)
                || this.data.scenario.endDate.getMinutes() !== this.endTimeFormControl.value.substr(2, 2))) {
          const timeParts = this.endTimeFormControl.value.split(':');
          this.data.scenario.endDate.setHours(timeParts[0]);
          this.data.scenario.endDate.setMinutes(timeParts[1]);
        }
        break;
      default:
        break;
    }
  }

}
