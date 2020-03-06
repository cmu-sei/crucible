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
          Implementation, ImplementationService } from '../../../../swagger-codegen/alloy.api';
import { DialogService } from '../../../../services/dialog/dialog.service';

@Component({
  selector: 'app-implementation-edit',
  templateUrl: './implementation-edit.component.html',
  styleUrls: ['./implementation-edit.component.css']
})
export class ImplementationEditComponent implements OnInit {

  @Input() implementation: Implementation;
  @Output() editComplete = new EventEmitter<boolean>();

  public implementationNameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(4)
  ]);
  public descriptionFormControl = new FormControl('', [
    Validators.required
  ]);
  public launchDateFormControl = new FormControl('', []);
  public endDateFormControl = new FormControl('', []);
  public expirationDateFormControl = new FormControl('', []);
  public statusDateFormControl = new FormControl('', []);
  public userIdFormControl = new FormControl('', [
    Validators.required
  ]);
  public usernameFormControl = new FormControl('', [
    Validators.required
  ]);
  public statusFormControl = new FormControl('', [
    Validators.required
  ]);
  public internalStatusFormControl = new FormControl('', [
    Validators.required
  ]);
  public definitionIdFormControl = new FormControl('', []);
  public exerciseIdFormControl = new FormControl('', []);
  public workspaceIdFormControl = new FormControl('', []);
  public runIdFormControl = new FormControl('', []);
  public sessionIdFormControl = new FormControl('', []);

  public implementationStates = Object.values(Implementation.StatusEnum);
  public matcher = new UserErrorStateMatcher();
  public exercises: Exercise[];
  public selectedExerciseId: any;
  public changesWereMade = false;

  constructor(
    public implementationService: ImplementationService,
    public dialogService: DialogService,
    public zone: NgZone,
    public playerService: PlayerService
  ) {

  }

  /**
   * Initialize component
   */
  ngOnInit() {
    this.playerService.getExercises().subscribe(exercises => {
      this.exercises = exercises.sort((x1, x2) => {
        return (x1.name > x2.name) ? 1 : ((x1.name < x2.name) ? -1 : 0);
      });
    },
    error => {
      console.log('The Player API is not responding.  ' + error.message);
    });
    this.usernameFormControl.disable();
    this.userIdFormControl.disable();
    this.statusFormControl.disable();
    this.internalStatusFormControl.disable();
    this.definitionIdFormControl.disable();
    this.exerciseIdFormControl.disable();
    this.workspaceIdFormControl.disable();
    this.runIdFormControl.disable();
    this.sessionIdFormControl.disable();
    this.launchDateFormControl.disable();
    this.endDateFormControl.disable();
    this.statusDateFormControl.disable();
    if (this.implementation.status === 'Ended' || this.implementation.status === 'Failed') {
      this.implementationNameFormControl.disable();
      this.descriptionFormControl.disable();
      this.expirationDateFormControl.disable();
    }
  }

  /**
   * Closes the edit screen
   */
  returnToImplementationList(changesWereMade: boolean): void {
    this.editComplete.emit(changesWereMade || this.changesWereMade);
  }


  /**
   * Delete a implementation after confirmation
   */
  deleteImplementation(): void {
    this.dialogService.confirm('Delete Implementation', 'Are you sure that you want to delete implementation ' + this.implementation.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.implementationService.deleteImplementation(this.implementation.id)
            .subscribe(deleted => {
              console.log('successfully deleted implementation');
              this.returnToImplementationList(true);
            });
        }
      });
  }

  /**
   * End a implementation
   */
  endImplementation(): void {
    this.dialogService.confirm('End Implementation Now', 'Are you sure that you want to end implementation ' + this.implementation.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.implementationService.endImplementation(this.implementation.id)
            .subscribe(implementation => {
              console.log('successfully ended implementation ' + implementation.id);
              this.returnToImplementationList(true);
            });
        }
      });
  }

  /**
   * Saves the current implementation
   */
  saveImplementation(changedField): void {
    let shouldUpdate = false;
    switch (changedField) {
      case 'name':
        if (!this.implementationNameFormControl.hasError('minlength') && !this.implementationNameFormControl.hasError('required')
            && this.implementation.name !== this.implementationNameFormControl.value) {
          this.implementation.name = this.implementationNameFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'description':
        if (!this.descriptionFormControl.hasError('required') && this.implementation.description !== this.descriptionFormControl.value) {
          this.implementation.description = this.descriptionFormControl.value;
            shouldUpdate = true;
        }
        break;
      case 'exerciseId':
        if (!this.exerciseIdFormControl.hasError('required') && this.implementation.exerciseId !== this.exerciseIdFormControl.value) {
          this.implementation.exerciseId = this.exerciseIdFormControl.value;
            shouldUpdate = true;
        }
        break;
      case 'launchDate':
        if (this.implementation.launchDate.toLocaleDateString() !== this.launchDateFormControl.value) {
          if (this.launchDateFormControl.value > '') {
            this.implementation.launchDate = new Date(this.launchDateFormControl.value);
          } else {
            this.implementation.launchDate = null;
          }
          shouldUpdate = true;
        }
        break;
      case 'endDate':
        if (this.implementation.endDate.toLocaleDateString() !== this.endDateFormControl.value) {
          if (this.endDateFormControl.value > '') {
            this.implementation.endDate = new Date(this.endDateFormControl.value);
          } else {
            this.implementation.endDate = null;
          }
          shouldUpdate = true;
        }
        break;
      case 'expirationDate':
        if (this.implementation.expirationDate.toLocaleDateString() !== this.expirationDateFormControl.value) {
          if (this.expirationDateFormControl.value > '') {
            this.implementation.expirationDate = new Date(this.expirationDateFormControl.value);
          } else {
            this.implementation.expirationDate = null;
          }
          shouldUpdate = true;
        }
        break;
      case 'statusDate':
        if (this.implementation.statusDate.toLocaleDateString() !== this.statusDateFormControl.value) {
          if (this.statusDateFormControl.value > '') {
            this.implementation.statusDate = new Date(this.statusDateFormControl.value);
          } else {
            this.implementation.statusDate = null;
          }
          shouldUpdate = true;
        }
        break;
      default:
        break;
    }
    if (shouldUpdate) {
      this.changesWereMade = true;
      this.implementationService.updateImplementation(this.implementation.id, this.implementation).subscribe(updatedImplementation => {
        updatedImplementation.launchDate = !updatedImplementation.launchDate ? null : new Date(updatedImplementation.launchDate);
        updatedImplementation.endDate = !updatedImplementation.endDate ? null : new Date(updatedImplementation.endDate);
        updatedImplementation.expirationDate = !updatedImplementation.expirationDate ? null : new Date(updatedImplementation.expirationDate);
        updatedImplementation.statusDate = !updatedImplementation.statusDate ? null : new Date(updatedImplementation.statusDate);
        this.implementation = updatedImplementation;
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

