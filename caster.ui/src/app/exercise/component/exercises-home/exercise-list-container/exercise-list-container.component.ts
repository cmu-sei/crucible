/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { ExerciseQuery, ExerciseService } from '../../../state';
import { CwdAuthService } from '../../../../sei-cwd-common/cwd-auth/services';
import { CwdSettingsService } from '../../../../sei-cwd-common/cwd-settings/services';
import { Exercise } from '../../../../generated/caster-api';
import { take } from 'rxjs/operators';
import { MatDialogRef, MatDialog } from '@angular/material';
import { ConfirmDialogComponent } from 'src/app/sei-cwd-common/confirm-dialog/components/confirm-dialog.component';
import { NameDialogComponent } from 'src/app/sei-cwd-common/name-dialog/name-dialog.component';
import {UserService, CurrentUserQuery} from '../../../../users/state';
const WAS_CANCELLED = 'wasCancelled';
const NAME_VALUE = 'nameValue';
@Component({
  selector: 'cas-exercise-container',
  templateUrl: './exercise-list-container.component.html',
  styleUrls: ['./exercise-list-container.component.scss'],
})
export class ExerciseListContainerComponent implements OnInit {
  public username: string;
  public titleText: string;
  public topBarColor = '#0FABEA';
  public isSuperUser = false;
  public exercises$: Observable<Exercise[]>;
  public isLoading$: Observable<boolean>;

  constructor(
    private exerciseService: ExerciseService,
    private exerciseQuery: ExerciseQuery,
    private authService: CwdAuthService,
    private settingsService: CwdSettingsService,
    private dialog: MatDialog,
    private userService: UserService,
    private currentUserQuery: CurrentUserQuery) {
  }

  ngOnInit() {

    this.exercises$ = this.exerciseQuery.selectAll();

    this.exerciseService.loadExercises().pipe(
      take(1)
    ).subscribe();

    this.isLoading$ = this.exerciseQuery.selectLoading();

    // Set the topbar color from config file
    this.topBarColor = this.settingsService.settings.AppTopBarHexColor;


    // Set the page title from configuration file
    this.titleText = this.settingsService.settings.AppTopBarText;

    this.currentUserQuery.select().subscribe(cu => {
      this.isSuperUser = cu.isSuperUser;
      this.username = cu.name;
    });
    this.userService.setCurrentUser();
  }

  logout(): void {
    this.authService.logout();
  }

  create() {
    this.nameDialog('Create New Exercise?', '', { nameValue: '' }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        const newExercise = {
          name: result[NAME_VALUE]
        } as Exercise;
        this.exerciseService.createExercise(newExercise).pipe(take(1)).subscribe();
      }
    });
  }

  update(exercise: Exercise) {
    this.nameDialog('Rename ' + exercise.name, '', { nameValue: exercise.name }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        const updatedExercise = { ...exercise, name: result[NAME_VALUE] } as Exercise;
        this.exerciseService.updateExercise(updatedExercise).pipe(take(1)).subscribe();
      }
    });
  }

  delete(exercise: Exercise) {
    this.confirmDialog('Delete Exercise?', 'Delete Exercise ' + exercise.name + '?', { buttonTrueText: 'Delete' }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        this.exerciseService.deleteExercise(exercise.id).pipe(take(1)).subscribe();
      }
    });
  }

  confirmDialog(title: string, message: string, data?: any): Observable<boolean> {
    let dialogRef: MatDialogRef<ConfirmDialogComponent>;
    dialogRef = this.dialog.open(ConfirmDialogComponent, { data: data || {} });
    dialogRef.componentInstance.title = title;
    dialogRef.componentInstance.message = message;

    return dialogRef.afterClosed();
  }

  nameDialog(title: string, message: string, data?: any): Observable<boolean> {
    let dialogRef: MatDialogRef<NameDialogComponent>;
    dialogRef = this.dialog.open(NameDialogComponent, { data: data || {} });
    dialogRef.componentInstance.title = title;
    dialogRef.componentInstance.message = message;

    return dialogRef.afterClosed();
  }

}

