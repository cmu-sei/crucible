/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { ExerciseStore } from './exercise.store';
import { Injectable } from '@angular/core';
import { ExerciseUI, Breadcrumb, ExerciseObjectType, Tab } from './exercise.model';
import { DirectoryQuery } from 'src/app/directories';
import { ExerciseQuery } from './exercise.query';
import { Workspace, Directory, Exercise, ExercisesService, ModelFile } from 'src/app/generated/caster-api';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { WorkspaceQuery, WorkspaceService } from 'src/app/workspace/state';

@Injectable({
  providedIn: 'root'
})
export class ExerciseService {
  constructor(
    private exerciseStore: ExerciseStore,
    private directoryQuery: DirectoryQuery,
    private workspaceQuery: WorkspaceQuery,
    private exerciseQuery: ExerciseQuery,
    private exercisesService: ExercisesService
    ) {

  }

  loadExercises(): Observable<Exercise[]> {
    return this.exercisesService.getAllExercises().pipe(
      tap(exercises => {
        const exerciseUIs = this.exerciseQuery.ui.getAll();
        this.exerciseStore.set(exercises);
        exercises.forEach(exercise => {
          const exerciseUI = exerciseUIs.find(xUI => (xUI.id === exercise.id));
          if (exerciseUI) {
            this.exerciseStore.ui.upsert(exercise.id, exerciseUI);
          }
        });
      }),
    );
  }

  loadExercise(exerciseId: string): Observable<Exercise> {
    return this.exercisesService.getExercise(exerciseId).pipe(
      tap(exercise => {
        this.exerciseStore.upsert(exercise.id, exercise);
        this.exerciseStore.ui.upsert(exercise.id, this.exerciseQuery.ui.getEntity(exercise.id));
      }),
    );
  }

  createExercise(exercise: Exercise): Observable<Exercise> {
    return this.exercisesService.createExercise(exercise).pipe(
      tap(ex => {
        this.exerciseStore.add(ex);
        this.exerciseStore.ui.upsert(ex.id, this.exerciseQuery.ui.getEntity(ex.id));
      }),
    );
  }

  deleteExercise(exerciseId: string): Observable<any> {
    return this.exercisesService.deleteExercise(exerciseId).pipe(
      tap(() => {
        this.exerciseStore.remove(exerciseId);
        this.exerciseStore.ui.remove(exerciseId);
      }),
    );
  }

  updateExercise(exercise: Exercise): Observable<Exercise> {
    return this.exercisesService.editExercise(exercise.id, exercise).pipe(
      tap(ex => {
        this.exerciseStore.upsert(ex.id, ex);
        this.exerciseStore.ui.upsert(ex.id, this.exerciseQuery.ui.getEntity(ex.id));
      }),
    );
  }

  setSelectedTab(index: number) {
    const exercise = this.exerciseQuery.getActive();
    if (exercise) {
      let exUi = this.exerciseQuery.ui.getEntity(exercise.id);
      exUi = { ...exUi, selectedTab: index } as ExerciseUI;
      this.exerciseStore.ui.upsert(exUi.id, exUi);
    }
  }

  closeTab(id: string) {
    const exercise = this.exerciseQuery.getActive();
    if (exercise) {
      const exerciseUI = this.exerciseQuery.ui.getEntity(exercise.id);
      const tabs = new Array<Tab>().concat(exerciseUI.openTabs);
      const index = exerciseUI.openTabs.findIndex(e => e.id === id);
      tabs.splice(index, 1);
      const selected = (exerciseUI.selectedTab >= tabs.length) ? tabs.length - 1 : exerciseUI.selectedTab;
      const exUi = { ...exerciseUI, openTabs: tabs, selectedTab:  selected};
      this.exerciseStore.ui.upsert(exUi.id, exUi);
    }
  }


  openTab(obj: ModelFile | Workspace, objType: ExerciseObjectType) {
    const exercise = this.exerciseQuery.getActive();
    if (exercise) {
      const exerciseUI = this.exerciseQuery.ui.getEntity(exercise.id);
      const tabIndex = exerciseUI.openTabs.findIndex(t => t.id === obj.id);
      if (tabIndex > -1) {
        // Tab already open so simply select it
        if (exerciseUI.selectedTab !== tabIndex) {
          const exUi = { ...exerciseUI, selectedTab: tabIndex } as ExerciseUI;
          this.exerciseStore.ui.upsert(exUi.id, exUi);
        }
      } else {
        // Tab is not open, add it
        let tabs = new Array<Tab>();
        tabs = tabs.concat(exerciseUI.openTabs);
        tabs.push(
          {
            id: obj.id,
            name: obj.name,
            type: objType,
            directoryId: obj.directoryId
          } as Tab);
        const exUi = { ...exerciseUI, openTabs: tabs, selectedTab: tabs.length - 1 } as ExerciseUI;
        this.exerciseStore.ui.upsert(exUi.id, exUi);
      }
    }
  }


  updateTabBreadcrumb(tabId: string, newBreadcrumb: Breadcrumb[]) {
    const exercise = this.exerciseQuery.getActive();
    let isChanged = false;
    if (exercise) {
      const exerciseUI = this.exerciseQuery.ui.getEntity(exercise.id);
      let updatedTabs = new Array<Tab>();
      exerciseUI.openTabs.forEach(tab => {
        if (tab.id === tabId) {
          if (!tab.breadcrumb) {
            isChanged = true;
          } else if (newBreadcrumb.length === tab.breadcrumb.length) {
            for (let i = 0; i < newBreadcrumb.length; i++) {
              if (newBreadcrumb[i].id !== tab.breadcrumb[i].id ||
                  newBreadcrumb[i].name !== tab.breadcrumb[i].name ||
                  newBreadcrumb[i].type !== tab.breadcrumb[i].type) {
                isChanged = true;
              }
            }
          }
          if (isChanged) {
            const newTab = {
              id: tab.id,
              name: tab.name,
              type: tab.type,
              directoryId: tab.directoryId,
              breadcrumb: newBreadcrumb
            };
            updatedTabs.push(newTab);
          }
        } else {
          updatedTabs.push(tab);
        }
      });
      if (isChanged) {
        const exUi = { ...exerciseUI, openTabs: updatedTabs } as ExerciseUI;
        this.exerciseStore.ui.upsert(exUi.id, exUi);
      }
    }
  }


  createBreadcrumb(exercise: Exercise, obj: ModelFile | Workspace | Tab , objType: ExerciseObjectType): Array<Breadcrumb> {
    const breadcrumb: Breadcrumb[] = [];
    breadcrumb.push({ name: obj.name, id: obj.id, type: objType } as Breadcrumb);
    if (objType === ExerciseObjectType.FILE && (obj as ModelFile).workspaceId !== null) {
      // When the file belongs to a workspace, add the workspace to the breadcrumb
      const workspace = this.workspaceQuery.getEntity((obj as ModelFile).workspaceId);
      if (workspace) {
        breadcrumb.unshift({ name: workspace.name, id: workspace.id, type: ExerciseObjectType.WORKSPACE } as Breadcrumb);
      }
    }

    let currentDir: Directory = this.directoryQuery.getEntity(obj.directoryId);
    while (currentDir) {
      breadcrumb.unshift({ name: currentDir.name, id: currentDir.id, type: ExerciseObjectType.DIRECTORY } as Breadcrumb);
      currentDir = this.directoryQuery.getEntity(currentDir.parentId);
    }
    breadcrumb.unshift({ name: exercise.name, id: exercise.id, type: ExerciseObjectType.EXERCISE } as Breadcrumb);

    return breadcrumb;
  }

  setRightSidebarOpen(exerciseId: string, open: boolean) {
    this.exerciseStore.ui.upsert(exerciseId, {rightSidebarOpen: open});
  }

  setRightSidebarView(exerciseId: string, view: string) {
    this.exerciseStore.ui.upsert(exerciseId, {rightSidebarView: view});
  }

  setRightSidebarWidth(exerciseId: string, width: number) {
    this.exerciseStore.ui.upsert(exerciseId, {rightSidebarWidth: width});
  }

  setLeftSidebarOpen(exerciseId: string, open: boolean) {
    this.exerciseStore.ui.upsert(exerciseId, {leftSidebarOpen: open});
  }

  setLeftSidebarWidth(exerciseId: string, width: number) {
    this.exerciseStore.ui.upsert(exerciseId, {leftSidebarWidth: width});
  }
}

