/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { QueryEntity, EntityUIQuery } from '@datorama/akita';
import {
  ProjectStore,
  ProjectState,
  ProjectUIState,
} from './project-store.service';
import { Breadcrumb, Tab } from './project.model';
import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { Project } from 'src/app/generated/caster-api';

@Injectable({
  providedIn: 'root',
})
export class ProjectQuery extends QueryEntity<ProjectState, Project> {
  ui: EntityUIQuery<ProjectUIState>;

  constructor(protected store: ProjectStore) {
    super(store);
    this.createUIQuery();
  }

  getRightSidebarOpen$(projectId): Observable<boolean> {
    return this.ui.selectEntity(projectId, (entity) => entity.rightSidebarOpen);
  }

  getRightSidebarView$(projectId): Observable<string> {
    return this.ui.selectEntity(projectId, (entity) => entity.rightSidebarView);
  }

  getRightSidebarWidth(projectId): Observable<number> {
    return this.ui.selectEntity(
      projectId,
      (entity) => entity.rightSidebarWidth
    );
  }

  getLeftSidebarOpen(projectId): Observable<boolean> {
    return this.ui.selectEntity(projectId, (entity) => entity.leftSidebarOpen);
  }

  getLeftSidebarWidth(projectId): Observable<number> {
    return this.ui.selectEntity(projectId, (entity) => entity.leftSidebarWidth);
  }

  selectTabBreadcrumb(
    projectId: string,
    tabId: string
  ): Observable<Breadcrumb[]> {
    return this.ui.selectEntity(projectId, (entity) => {
      const tab = entity.openTabs.find((t) => t.id === tabId);
      return tab ? tab.breadcrumb : [];
    });
  }

  selectOpenTabs(projectId: string): Observable<Tab[]> {
    return this.ui.selectEntity(projectId, (entity) => entity.openTabs);
  }

  selectSelectedTab(projectId: string): Observable<number> {
    return this.ui.selectEntity(projectId, (entity) => entity.selectedTab);
  }

  selectBreadcrumb(
    projectId: string,
    entityId: string
  ): Observable<Breadcrumb[]> {
    return this.ui.selectEntity(
      projectId,
      (entity) => entity.openTabs.find((t) => t.id === entityId).breadcrumb
    );
  }
}
