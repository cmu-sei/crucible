/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource, MatSort, MatSortable } from '@angular/material';
import { Exercise, ExerciseService } from '../../../swagger-codegen/s3.player.api';
import { LoggedInUserService } from '../../../services/logged-in-user/logged-in-user.service';
import { DialogService } from '../../../services/dialog/dialog.service';
import { AdminExerciseEditComponent } from './admin-exercise-edit/admin-exercise-edit.component';

export interface Action {
  Value: string;
  Text: string;
}

@Component({
  selector: 'app-admin-exercise-search',
  templateUrl: './admin-exercise-search.component.html',
  styleUrls: ['./admin-exercise-search.component.css']
})
export class AdminExerciseSearchComponent implements OnInit {

  @ViewChild(AdminExerciseEditComponent, { static: true }) adminExerciseEditComponent: AdminExerciseEditComponent;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  public exerciseActions: Action[] = [
    { Value: 'edit', Text: 'Edit Exercise' },
    { Value: 'activate', Text: 'Activate/Deactivate Exercise' }
  ];

  public exerciseDataSource: MatTableDataSource<Exercise>;
  public displayedColumns: string[] = ['name', 'description', 'status'];
  public filterString: string;
  public showEditScreen: Boolean;
  public isLoading: Boolean;

  constructor(private exerciseService: ExerciseService,
    public loggedInUserService: LoggedInUserService,
    public dialogService: DialogService
  ) { }

  /**
   * Initialization
   */
  ngOnInit() {
    // Initial datasource
    this.exerciseDataSource = new MatTableDataSource<Exercise>(new Array<Exercise>());
    this.sort.sort(<MatSortable>({id: 'name', start: 'asc'}));
    this.exerciseDataSource.sort = this.sort;
    this.showEditScreen = false;
    this.filterString = '';

    // Initial datasource
    this.filterString = '';

    this.loggedInUserService.loggedInUser.subscribe(user => {
      this.refreshExercises();
    });
  }


  /**
   * Executes an action menu item
   * @param action: action string to case from
   * @param exerciseGuid: The guid for exercise
   */
  executeExerciseAction(action: string, exerciseGuid: string) {
    switch (action) {
      case ('edit'): {
        // Edit exercise
        this.exerciseService.getExercise(exerciseGuid)
          .subscribe(exercise => {
            this.adminExerciseEditComponent.resetStepper();
            this.adminExerciseEditComponent.updateExercise();
            this.adminExerciseEditComponent.updateApplicationTemplates();
            this.adminExerciseEditComponent.exercise = exercise;
            this.showEditScreen = true;
        });
        break;
      }
      case ('activate'): {
        // Activate or Deactivate
        this.exerciseService.getExercise(exerciseGuid)
          .subscribe(exercise => {
            let msg = '';
            let title = '';
            let activation = Exercise.StatusEnum.Inactive;
            if (exercise.status === undefined || exercise.status === Exercise.StatusEnum.Inactive) {
              msg = 'Do you wish to Activate exercise ' + exercise.name + '?';
              title = 'Activate Exercise?';
              activation = Exercise.StatusEnum.Active;
            } else {
              msg = 'Do you wish to deactivate exercise ' + exercise.name + '?';
              title = 'Deactivate Exercise?';
              activation = Exercise.StatusEnum.Inactive;
            }
            this.dialogService.confirm(title, msg)
              .subscribe(result => {
                if (result['confirm']) {
                  exercise.status = activation;
                  this.exerciseService.updateExercise(exerciseGuid, exercise)
                    .subscribe(updateexercise => {
                      console.log('successfully updated exercise ' + updateexercise.name);
                      this.refreshExercises();
                    });
                }
              });
          });
        break;
      }
      default: {
        alert('Unknown Action');
        break;
      }
    }
  }


  /**
   * Adds a new exercise
   */
  addNewExercise() {
    const exercise = <Exercise>{name: 'New Exercise', description: 'Add description', status: Exercise.StatusEnum.Active};
    this.exerciseService.createExercise(exercise).subscribe(ex => {
      this.refreshExercises();
      this.executeExerciseAction('edit', ex.id);
    });
  }

  /**
   * Called by UI to add a filter to the exerciseDataSource
   * @param filterValue
   */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.exerciseDataSource.filter = filterValue;
  }

  /**
   * Updated the datasource for the exercise search table
   */
  refreshExercises() {
    this.showEditScreen = false;
    this.isLoading = true;
    this.exerciseService.getExercises().subscribe(exercises => {
      this.exerciseDataSource.data = exercises;
      this.isLoading = false;
    });
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

}

