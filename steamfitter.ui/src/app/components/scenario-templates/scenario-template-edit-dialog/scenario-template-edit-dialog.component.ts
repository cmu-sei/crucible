/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, EventEmitter, Output, Inject, ChangeDetectorRef } from '@angular/core';
import { DialogService } from 'src/app/services/dialog/dialog.service';
import { MAT_DIALOG_DATA, ErrorStateMatcher } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';

/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

/** Error when control isn't an integer */
export class NotIntegerErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const hours = parseInt(control.value, 10);
    let isNotAnInteger = hours === NaN || hours <= 0;
    if (!isNotAnInteger && !!control.value) {
      isNotAnInteger = hours.toString() !== control.value.toString();
    }
    if (isNotAnInteger) {
      control.setErrors({notAnInteger: true});
    }
    const isSubmitted = form && form.submitted;
    return !!(control && (control.invalid || isNotAnInteger) && (control.dirty || isSubmitted));
  }
}

@Component({
  selector: 'app-scenario-template-edit-dialog',
  templateUrl: './scenario-template-edit-dialog.component.html',
  styleUrls: ['./scenario-template-edit-dialog.component.css']
})

export class ScenarioTemplateEditDialogComponent {

  @Output() editComplete = new EventEmitter<any>();

  scenarioTemplateNameFormControl = new FormControl(this.data.scenarioTemplate.name, [
    Validators.required,
    Validators.minLength(4)
  ]);
  descriptionFormControl = new FormControl(this.data.scenarioTemplate.description ? this.data.scenarioTemplate.description : ' ', [
    Validators.required,
    Validators.minLength(4)
  ]);
  durationHoursFormControl = new FormControl(this.data.scenarioTemplate.durationHours, [
    Validators.required
  ]);
  matcher = new UserErrorStateMatcher();
  notAnIntegerErrorState = new NotIntegerErrorStateMatcher();

  constructor(
    public dialogService: DialogService,
    private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
  }

  errorFree() {
    const hasError = this.scenarioTemplateNameFormControl.hasError('required')
              || this.scenarioTemplateNameFormControl.hasError('minlength')
              || this.descriptionFormControl.hasError('required')
              || this.descriptionFormControl.hasError('minlength')
              || this.durationHoursFormControl.hasError('required')
              || this.durationHoursFormControl.hasError('notAnInteger');
    return !hasError;
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
      this.editComplete.emit({saveChanges: false, scenarioTemplate: null});
    } else {
      const modifiedScenarioTemplate = { ...this.data.scenarioTemplate, id: this.data.scenarioTemplate.id };
      modifiedScenarioTemplate.name = this.scenarioTemplateNameFormControl.value.toString().trim();
      modifiedScenarioTemplate.description = this.descriptionFormControl.value.toString().trim();
      modifiedScenarioTemplate.durationHours = this.durationHoursFormControl.value.toString().trim();
      if (this.errorFree) {
        this.editComplete.emit({saveChanges: saveChanges, scenarioTemplate: modifiedScenarioTemplate});
      }
    }
  }

}
