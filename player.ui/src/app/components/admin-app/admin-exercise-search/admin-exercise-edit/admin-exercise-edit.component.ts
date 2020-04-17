/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, EventEmitter, Output, NgZone, ViewChild } from '@angular/core';
import { ErrorStateMatcher, MatStepper } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { Exercise, Team, TeamService, ExerciseService, UserService } from '../../../../swagger-codegen/s3.player.api';
import { TeamForm, ApplicationTemplate, ApplicationService, Application } from '../../../../swagger-codegen/s3.player.api';
import { User } from '../../../../swagger-codegen/s3.player.api';
import { DialogService } from '../../../../services/dialog/dialog.service';
import { take } from 'rxjs/operators';
import { ExerciseApplicationsSelectComponent } from '../../exercise-applications-select/exercise-applications-select.component';

/** Team node with related user and application information */
export class TeamUserApp {
  constructor(
    public name: string,
    public team: Team,
    public users: Array<User>
  ) { }
}


@Component({
  selector: 'app-admin-exercise-edit',
  templateUrl: './admin-exercise-edit.component.html',
  styleUrls: ['./admin-exercise-edit.component.css']
})
export class AdminExerciseEditComponent implements OnInit {

  @Output() editComplete = new EventEmitter<boolean>();
  @ViewChild(ExerciseApplicationsSelectComponent, { static: false }) exerciseApplicationsSelectComponent: ExerciseApplicationsSelectComponent;
  @ViewChild(AdminExerciseEditComponent, { static: false }) child;
  @ViewChild('stepper', { static: false }) stepper: MatStepper;

  public exerciseNameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(4)
  ]);

  public teamNameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(3)
  ]);

  public descriptionFormControl = new FormControl('', [
    Validators.required
  ]);

  public matcher = new UserErrorStateMatcher();
  public exerciseStates = Object.values(Exercise.StatusEnum);
  public isLinear = false;

  public exercise: Exercise;
  public teams: Array<TeamUserApp>;
  public currentTeam: TeamUserApp;

  public isLoadingTeams: Boolean;

  public applicationTemplates: Array<ApplicationTemplate>;
  public BLANK_TEMPLATE = <ApplicationTemplate>{
    name: 'New Application',
    exerciseId: '',
  };

  constructor(
    public exerciseService: ExerciseService,
    public teamService: TeamService,
    public dialogService: DialogService,
    public userService: UserService,
    public applicationService: ApplicationService,
    public zone: NgZone
  ) {

  }


  /**
   * Initialize component
   */
  ngOnInit() {
    this.isLoadingTeams = false;
    this.exercise = undefined;
    this.teams = new Array<TeamUserApp>();
}


  /**
   * Updates the list of available app templates
   */
  updateApplicationTemplates() {
    this.applicationService.getApplicationTemplates().subscribe(templates => {
      this.applicationTemplates = templates;
    });
  }


  /**
   * Updates the contents of the current exercise
   */
  updateExerciseTeams(): void {
    if (this.exercise !== undefined && this.exercise.id !== undefined) {
      // Update the teams arrays
      this.isLoadingTeams = true;
      this.teams = new Array<TeamUserApp>();
      this.teamService.getExerciseTeams(this.exercise.id).subscribe(tms => {
        const userTeams = new Array<TeamUserApp>();
        tms.forEach(tm => {
          this.userService.getTeamUsers(tm.id).subscribe(usrs => {
            this.teams.push(new TeamUserApp(tm.name, tm, usrs));
            this.teams.sort((t1, t2) => {
              if (t1.name === null || t2.name === null) { return 0; }
              if (t1.name.toLowerCase() < t2.name.toLowerCase()) { return -1; }
              if (t1.name.toLowerCase() > t2.name.toLowerCase()) { return 1; }
              return 0;
            });
            if (this.teams.length === tms.length) {
              this.isLoadingTeams = false;
            }
          });
        });
      });
    }
  }

    /**
   * Updates the contents of the current exercise
   */
  updateExercise(): void {
    if (this.exercise !== undefined && this.exercise.id !== undefined) {

      this.exerciseApplicationsSelectComponent.exercise = this.exercise;
      this.exerciseApplicationsSelectComponent.updateApplications();
      this.exerciseApplicationsSelectComponent.currentApp = undefined;
    }
  }


  /**
   * Returns the stepper to zero index
   */
  resetStepper() {
    if (this.stepper) {
      this.stepper.selectedIndex = 0;
      this.exercise = undefined;
    }
  }


  /**
   * Closes the edit screen
   */
  returnToExerciseSearch(): void {
    this.currentTeam = undefined;
    this.editComplete.emit(true);
  }


  addExerciseApplication(template: ApplicationTemplate) {
    console.log(template);
    let app = <Application>{};
    if (template.id == null) {
      app = <Application> {
        name: template.name,
        exerciseId: this.exercise.id,
      };
    } else {
      app = <Application>{
        exerciseId: this.exercise.id,
        applicationTemplateId: template.id
      };
    }

    console.log(app);
    this.applicationService.createApplication(this.exercise.id, app).subscribe(rslt => {
      console.log('Application added');
      this.exerciseApplicationsSelectComponent.updateApplications();
      this.exerciseApplicationsSelectComponent.currentApp = rslt;
    });
  }

  /**
   * Delete an exercise after confirmation
   */
  deleteExercise(): void {
    this.dialogService.confirm('Delete Exercise', 'Are you sure that you want to delete exercise ' + this.exercise.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.exerciseService.deleteExercise(this.exercise.id)
            .subscribe(deleted => {
              console.log('successfully deleted exercise');
              this.returnToExerciseSearch();
            });
        }
      });
  }

  /**
   * Saves the current exercise
   */
  saveExercise(): void {
    if (!this.exerciseNameFormControl.hasError('minlength') && !this.exerciseNameFormControl.hasError('required')) {
      if (this.exercise.name !== this.exerciseNameFormControl.value) {
        this.exercise.name = this.exerciseNameFormControl.value;
        this.exerciseService.updateExercise(this.exercise.id, this.exercise).subscribe(updatedExercise => {
          this.exercise = updatedExercise;
        });
      }
    }

    if (!this.descriptionFormControl.hasError('required')) {
      if (this.exercise.description !== this.descriptionFormControl.value) {
        this.exercise.description = this.descriptionFormControl.value;
        this.exerciseService.updateExercise(this.exercise.id, this.exercise).subscribe(updatedExercise => {
          this.exercise = updatedExercise;
        });
      }
    }
  }


  /**
   * Updates the exercise status
   */
  saveExerciseStatus(): void {
    console.log(this.exercise.status);
    this.exerciseService.updateExercise(this.exercise.id, this.exercise).subscribe(updatedExercise => {
      this.exercise = updatedExercise;
    });
  }


  /**
   * Delete a team after confirmation
   * @param tm The team to delete
   */
  deleteTeam(tm: Team): void {
    this.dialogService.confirm('Delete Exercise', 'Are you sure that you want to delete team ' + tm.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.teamService.deleteTeam(tm.id)
            .subscribe(deleted => {
              console.log('successfully deleted team');
              this.updateExerciseTeams();
            });
        }
      });
  }


  /**
   * Saves the team name
   * @param name New name of the team
   * @param id team Guid
   */
  saveTeamName(name: string, id: string): void {
    this.teamService.getTeam(id).subscribe(tm => {
      tm.name = name;
      this.teamService.updateTeam(id, tm).subscribe(updatedTeam => {
        this.teams.find(t => t.team.id === id).team = updatedTeam;
        console.log('Team updated:  ' + updatedTeam.name);
      });
    });
  }


  /**
   * Opens the Add/Remove Users dialog for a specific team
   * @param team The team to add/remove users to
   */
  openUsersDialog(team: Team): void {
    if (team !== undefined) {
      this.dialogService.addRemoveUsersToTeam('Add or Remove Users for team ' + team.name, team)
        .subscribe(result => {
          this.teams.find(t => t.team.id === team.id).users = result['teamUsers'];
        });
    }
  }

  /**
   * Called when the mat-step index has changed to signal an update to the exercise
   * @param event SelectionChange event
   */
  onExerciseStepChange(event: any) {
    // index 2 is the Teams step.  Refresh when selected to ensure latest information updated
    if (event.selectedIndex === 2) {
      this.currentTeam = undefined;
      this.updateExerciseTeams();
    } else if (event.selectedIndex === 1) {
      // Clicked away from teams
      this.updateApplicationTemplates();
      this.exerciseApplicationsSelectComponent.updateApplications();
    }
  }

  /**
   * Adds a new Team to the exercise
   */
  addNewTeam() {
    this.teamService.createTeam(this.exercise.id, <TeamForm>({ name: 'New Team' })).subscribe(newTeam => {
      const team = new TeamUserApp('New Team', newTeam, new Array<User>());
      this.teams.unshift(team);
      this.currentTeam = team;
      // This uses the rxjs take and ngZone to determine when the html is rendered
      this.zone.onMicrotaskEmpty.asObservable().pipe(take(1)).subscribe(() => {
        const nameElement = <HTMLInputElement>document.getElementById('teamName' + newTeam.id);
        if (nameElement) {
          nameElement.focus();
          nameElement.select();
        }
      });
    });
  }

} // End Class


/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

