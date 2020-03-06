/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {QueryEntity, EntityUIQuery} from '@datorama/akita';
import {ExerciseStore, ExerciseState, ExerciseUIState} from './exercise.store';
import {Breadcrumb, Tab} from './exercise.model';
import {Observable} from 'rxjs';
import {Injectable} from '@angular/core';
import { Exercise } from 'src/app/generated/caster-api';



@Injectable({
  providedIn: 'root'
})
export class ExerciseQuery extends QueryEntity<ExerciseState, Exercise> {
  ui: EntityUIQuery<ExerciseUIState>;

  constructor(protected store: ExerciseStore) {
    super(store);
    this.createUIQuery();
  }

  getRightSidebarOpen$(exerciseId): Observable<boolean> {
    return this.ui.selectEntity(exerciseId, entity => entity.rightSidebarOpen);
  }

  getRightSidebarView$(exerciseId): Observable<string> {
    return this.ui.selectEntity(exerciseId, entity => entity.rightSidebarView);
  }

  getRightSidebarWidth(exerciseId): Observable<number> {
    return this.ui.selectEntity(exerciseId, entity => entity.rightSidebarWidth);
  }

  getLeftSidebarOpen(exerciseId): Observable<boolean> {
    return this.ui.selectEntity(exerciseId, entity => entity.leftSidebarOpen);
  }

  getLeftSidebarWidth(exerciseId): Observable<number> {
    return this.ui.selectEntity(exerciseId, entity => entity.leftSidebarWidth);
  }

  selectTabBreadcrumb(exerciseId: string, tabId: string): Observable<Breadcrumb[]> {
    return this.ui.selectEntity(exerciseId, entity => {
      const tab = entity.openTabs.find(t => t.id === tabId);
      return tab ? tab.breadcrumb : [];
    });
  }

  selectOpenTabs(exerciseId: string): Observable<Tab[]> {
    return this.ui.selectEntity(exerciseId, entity => entity.openTabs);
  }

  selectSelectedTab(exerciseId: string): Observable<number> {
    return this.ui.selectEntity(exerciseId, entity => entity.selectedTab);
  }

  selectBreadcrumb(exerciseId: string, entityId: string): Observable<Breadcrumb[]> {
    return this.ui.selectEntity(exerciseId, entity => entity.openTabs.find(t => t.id === entityId).breadcrumb);
  }
}

