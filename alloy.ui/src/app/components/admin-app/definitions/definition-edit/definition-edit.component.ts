/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, NgZone, Input } from '@angular/core';
import { ErrorStateMatcher } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { Definition } from '../../../../swagger-codegen/alloy.api';
import { DialogService } from '../../../../services/dialog/dialog.service';
import { DefinitionsService } from 'src/app/services/definitions/definitions.service';

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
    const isNotAnInteger = parseInt(control.value, 10).toString() !== control.value;
    if (isNotAnInteger) {
      control.setErrors({notAnInteger: true});
    }
    const isSubmitted = form && form.submitted;
    return !!(control && (control.invalid || isNotAnInteger) && (control.dirty || isSubmitted));
  }
}

@Component({
  selector: 'app-definition-edit',
  templateUrl: './definition-edit.component.html',
  styleUrls: ['./definition-edit.component.css']
})

export class DefinitionEditComponent implements OnInit {

  @Input() definition: Definition;

  public definitionNameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(4)
  ]);
  public descriptionFormControl = new FormControl('', [
    Validators.required
  ]);
  public durationHoursFormControl = new FormControl('', []);
  public exerciseIdFormControl = new FormControl('', []);
  public directoryIdFormControl = new FormControl('', []);
  public scenarioIdFormControl = new FormControl('', []);
  public matcher = new UserErrorStateMatcher();
  public notAnIntegerErrorState = new NotIntegerErrorStateMatcher();

  constructor(
    public definitionsService: DefinitionsService,
    public dialogService: DialogService,
    public zone: NgZone
  ) {}

  /**
   * Initialize component
   */
  ngOnInit() {
    this.definitionNameFormControl.setValue(this.definition.name);
    this.descriptionFormControl.setValue(this.definition.description);
    this.durationHoursFormControl.setValue(this.definition.durationHours);
    this.exerciseIdFormControl.setValue(this.definition.exerciseId);
    this.directoryIdFormControl.setValue(this.definition.directoryId);
    this.scenarioIdFormControl.setValue(this.definition.scenarioId);
  }

  /**
   * Delete a definition after confirmation
   */
  deleteDefinition(): void {
    this.dialogService.confirm('Delete Definition', 'Are you sure that you want to delete definition ' + this.definition.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.definitionsService.delete(this.definition.id);
        }
      });
  }

  /**
   * Saves the current definition
   */
  saveDefinition(changedField): void {
    let shouldUpdate = false;
    switch (changedField) {
      case 'name':
        if (!this.definitionNameFormControl.hasError('minlength') && !this.definitionNameFormControl.hasError('required')
            && this.definition.name !== this.definitionNameFormControl.value) {
          this.definition.name = this.definitionNameFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'description':
        if (this.definition.description !== this.descriptionFormControl.value) {
          this.definition.description = this.descriptionFormControl.value;
            shouldUpdate = true;
        }
        break;
      case 'durationHours':
        if (parseInt(this.durationHoursFormControl.value.toString(), 10).toString() === this.durationHoursFormControl.value.toString()
            && this.definition.durationHours !== this.durationHoursFormControl.value) {
          this.definition.durationHours = this.durationHoursFormControl.value;
            shouldUpdate = true;
        }
        break;
      case 'exerciseId':
        if (this.definition.exerciseId !== this.exerciseIdFormControl.value) {
          this.definition.exerciseId = this.exerciseIdFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'directoryId':
        if (this.definition.directoryId !== this.directoryIdFormControl.value) {
          this.definition.directoryId = this.directoryIdFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'scenarioId':
        if (this.definition.scenarioId !== this.scenarioIdFormControl.value) {
          this.definition.scenarioId = this.scenarioIdFormControl.value;
          shouldUpdate = true;
        }
        break;
      default:
        break;
    }
    if (shouldUpdate) {
      this.definitionsService.update(this.definition);
    }
  }

  /**
   * Clone a definition after confirmation
   */
  cloneDefinition(): void {
    this.dialogService.confirm('Clone Definition', 'Are you sure that you want to create a new definition from '
      + this.definition.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          const newDefinition = {
            name: this.definition.name + ' - clone',
            description: this.definition.description,
            durationHours: this.definition.durationHours,
            exerciseId: this.definition.exerciseId,
            directoryId: this.definition.directoryId,
            scenarioId: this.definition.scenarioId
          };
          this.definitionsService.addNew(newDefinition);
        }
      });
  }

} // End Class

