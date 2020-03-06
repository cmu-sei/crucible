/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, EventEmitter, Output, NgZone, ViewChild, Input } from '@angular/core';
import { ErrorStateMatcher } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { Scenario, ScenarioService, SessionService } from '../../../swagger-codegen/dispatcher.api';
import { DialogService } from '../../../services/dialog/dialog.service';
import { DispatchTaskSourceType, DispatchTaskSource } from '../../tasks/task-tree/task-tree.component';
import { TasksComponent } from '../../tasks/tasks.component';
import { Subject, ReplaySubject } from 'rxjs';

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
  selector: 'app-scenario-edit',
  templateUrl: './scenario-edit.component.html',
  styleUrls: ['./scenario-edit.component.css']
})

export class ScenarioEditComponent implements OnInit {

  @Input() scenario: Scenario;
  @Input() openedAsDialog: boolean;
  @Output() editComplete = new EventEmitter<boolean>();
  @ViewChild(ScenarioEditComponent) child;
  @ViewChild(TasksComponent) tasks: TasksComponent;

  public scenarioNameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(4)
  ]);
  public descriptionFormControl = new FormControl('', [
    Validators.required
  ]);
  public durationHoursFormControl = new FormControl('', []);

  private _scenario: Scenario;
  public dispatchTaskSource: DispatchTaskSource;
  public changesWereMade = false;

  public matcher = new UserErrorStateMatcher();
  public notAnIntegerErrorState = new NotIntegerErrorStateMatcher();

  constructor(
    public scenarioService: ScenarioService,
    public sessionService: SessionService,
    public dialogService: DialogService,
    public zone: NgZone
  ) {}

  /**
   * Initialize component
   */
  ngOnInit() {
    this.dispatchTaskSource = {'type': DispatchTaskSourceType.Scenario, 'id': this.scenario.id};
  }

  /**
   * Closes the edit screen
   */
  returnToScenarioList(changesWereMade: boolean): void {
    this.editComplete.emit(changesWereMade || this.changesWereMade);
  }

  /**
   * Delete a scenario after confirmation
   */
  deleteScenario(): void {
    this.dialogService.confirm('Delete Scenario', 'Are you sure that you want to delete scenario ' + this.scenario.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.scenarioService.deleteScenario(this.scenario.id)
            .subscribe(deleted => {
              console.log('successfully deleted scenario');
              this.returnToScenarioList(true);
            });
        }
      });
  }

  /**
   * Saves the current scenario
   */
  saveScenario(changedField): void {
    let shouldUpdate = true;
    switch (changedField) {
      case 'name':
        if (!this.scenarioNameFormControl.hasError('minlength') && !this.scenarioNameFormControl.hasError('required')
            && this.scenario.name !== this.scenarioNameFormControl.value) {
          this.scenario.name = this.scenarioNameFormControl.value;
        } else {
          shouldUpdate = false;
        }
        break;
      case 'description':
        if (!this.descriptionFormControl.hasError('required') && this.scenario.description !== this.descriptionFormControl.value) {
          this.scenario.description = this.descriptionFormControl.value;
        } else {
            shouldUpdate = false;
        }
        break;
      case 'durationHours':
        if (parseInt(this.durationHoursFormControl.value.toString(), 10).toString() === this.durationHoursFormControl.value.toString()
            && this.scenario.durationHours !== this.durationHoursFormControl.value) {
          this.scenario.durationHours = this.durationHoursFormControl.value;
        } else {
            shouldUpdate = false;
        }
        break;
      default:
        break;
    }
    if (shouldUpdate) {
      this.scenarioService.updateScenario(this.scenario.id, this.scenario).subscribe(updatedScenario => {
        this.scenario = updatedScenario;
        this.changesWereMade = true;
      });

    }
  }

  /**
   * Copy a scenario after confirmation
   */
  copyScenario(): void {
    this.dialogService.confirm('Copy Scenario', 'Are you sure that you want to create a new scenario from ' + this.scenario.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.scenarioService.copyScenario(this.scenario.id)
            .subscribe(newScenario => {
              console.log('successfully created scenario');
              this.returnToScenarioList(true);
            });
        }
      });
  }

  /**
   * Create a session after confirmation
   */
  createSession(): void {
    this.dialogService.confirm('Create Session', 'Are you sure that you want to create a session from ' + this.scenario.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.sessionService.createSessionFromScenario(this.scenario.id)
            .subscribe(newSession => {
              console.log('successfully created session from scenario');
              this.returnToScenarioList(true);
            });
        }
      });
  }

} // End Class

