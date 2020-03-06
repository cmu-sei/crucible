/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {Exercise, Workspace, Directory, ModelFile} from '../../generated/caster-api';
import {createWorkspace} from "../../workspace/state";
export type ExerciseEntityType = Exercise | Directory | ModelFile | Workspace;
export enum ExerciseObjectType {
  EXERCISE = 'EXERCISE',
  DIRECTORY = 'DIRECTORY',
  FILE = 'FILE',
  WORKSPACE = 'WORKSPACE'
}

export interface Breadcrumb {
  id: string;
  name: string;
  type: ExerciseObjectType;
}

export interface Tab {
  id: string;
  type: ExerciseObjectType;
  name: string;
  directoryId: string;
  breadcrumb: Array<Breadcrumb>;
}

export interface ExerciseUI {
  id?: string;
  openTabs: Array<Tab>;
  selectedTab: number;
  rightSidebarOpen: boolean;
  rightSidebarView: string;
  rightSidebarWidth: number;
  leftSidebarOpen: boolean;
  leftSidebarWidth: number;
}

