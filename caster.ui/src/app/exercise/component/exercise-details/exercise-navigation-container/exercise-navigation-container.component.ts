/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Observable, merge, BehaviorSubject, Subject } from 'rxjs';
import { ExerciseService, ExerciseQuery, ExerciseUI, ExerciseStore } from '../../../state';
import { DirectoryQuery, DirectoryService } from '../../../../directories/state';
import { Directory, Exercise } from '../../../../generated/caster-api';
import { take, tap, map, startWith, takeUntil } from 'rxjs/operators';
import { WorkspaceQuery, WorkspaceService } from '../../../../workspace/state';
import { RouterQuery } from '@datorama/akita-ng-router-store';
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material';
import { ConfirmDialogComponent } from 'src/app/sei-cwd-common/confirm-dialog/components/confirm-dialog.component';
import { NameDialogComponent } from 'src/app/sei-cwd-common/name-dialog/name-dialog.component';


const WAS_CANCELLED = 'wasCancelled';
const NAME_VALUE = 'nameValue';

@Component({
  selector: 'cas-exercise-navigation',
  templateUrl: './exercise-navigation-container.component.html',
  styleUrls: ['./exercise-navigation-container.component.scss']
})

export class ExerciseNavigationContainerComponent implements OnInit, OnDestroy {

  public exercise$: Observable<Exercise>;
  public exerciseDirectories$: Observable<Directory[]>;
  public exerciseId: string;

  private unsubscribe$ = new Subject<void>();

  constructor(
    private routerQuery: RouterQuery,
    private exerciseQuery: ExerciseQuery,
    private exerciseService: ExerciseService,
    private exerciseStore: ExerciseStore,
    private directoryQuery: DirectoryQuery,
    private directoryService: DirectoryService,
    private dialog: MatDialog
  ) { }

  ngOnInit() {

    this.routerQuery.select('state').pipe(
      map(state => state.root.params),
      takeUntil(this.unsubscribe$),
    ).subscribe(params => {

      this.exerciseId = params.id;

      if (this.exerciseId) {
        this.exercise$ = this.exerciseQuery.selectActive();
        this.exerciseDirectories$ = this.directoryQuery.selectAll(
          { filterBy: dir => dir.exerciseId === this.exerciseId && dir.parentId === null });

        // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
        this.exerciseService.loadExercise(this.exerciseId).pipe(take(1)).subscribe();
        // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
        this.directoryService.loadDirectories(this.exerciseId).pipe(take(1)).subscribe();

        this.exerciseStore.setActive(this.exerciseId);
        this.exerciseStore.ui.setActive(this.exerciseId);
      }
    });
  }

  nameDialog(title: string, message: string, data?: any): Observable<boolean> {
    let dialogRef: MatDialogRef<NameDialogComponent>;
    dialogRef = this.dialog.open(NameDialogComponent, { data: data || {} });
    dialogRef.componentInstance.title = title;
    dialogRef.componentInstance.message = message;

    return dialogRef.afterClosed();
  }

  createNewDirectory(dirId?: string) {
    // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
    this.nameDialog('Create New Directory?', '', { nameValue: '' }).pipe(take(1)).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        this.directoryService.add({ name: result[NAME_VALUE], exerciseId: this.exerciseId, parentId: null } as Directory);
      }
    });
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}

