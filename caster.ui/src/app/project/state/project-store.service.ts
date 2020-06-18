/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {
  EntityState,
  EntityStore,
  EntityUIStore,
  StoreConfig,
  ActiveState,
} from '@datorama/akita';
import { ProjectUI } from './project.model';
import { Injectable } from '@angular/core';
import { Project } from 'src/app/generated/caster-api';

export interface ProjectUIState extends EntityState<ProjectUI>, ActiveState {}
export interface ProjectState extends EntityState<Project>, ActiveState {}

export const initialProjectUIState: ProjectUI = {
  openTabs: [],
  selectedTab: null,
  rightSidebarOpen: false,
  rightSidebarView: '',
  rightSidebarWidth: 300,
  leftSidebarOpen: true,
  leftSidebarWidth: 364,
};

@Injectable({
  providedIn: 'root',
})
@StoreConfig({ name: 'projects' })
export class ProjectStore extends EntityStore<ProjectState> {
  ui: EntityUIStore<ProjectUIState>;
  constructor() {
    super();
    this.createUIStore().setInitialEntityState((entity) => ({
      ...initialProjectUIState,
    }));
  }
}
