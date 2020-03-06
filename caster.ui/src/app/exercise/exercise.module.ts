/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {RouterModule, Routes} from '@angular/router';
import {FlexLayoutModule} from '@angular/flex-layout';
import {
  MatButtonModule,
  MatCardModule,
  MatFormFieldModule,
  MatIconModule,
  MatInputModule,
  MatProgressSpinnerModule,
  MatSortModule,
  MatTableModule,
  MatTabsModule,
  MatTooltipModule,
  MatButtonToggleModule,
  MatExpansionModule,
  MatListModule,
  MatMenuModule,
  MatSidenavModule,
  MatBadgeModule,
  MatDialogModule,
} from '@angular/material';
import {ResizableModule} from 'angular-resizable-element';
import {ExerciseQuery, ExerciseService, ExerciseStore} from './state';
import {DirectoriesModule} from '../directories';
import {ExerciseComponent} from './component/exercise-details/exercise/exercise.component';
import {ExerciseListComponent} from './component/exercises-home/exercise-list/exercise-list.component';
import {ExerciseListContainerComponent} from './component/exercises-home/exercise-list-container/exercise-list-container.component';
import {CwdAuthGuardService} from '../sei-cwd-common/cwd-auth/services';
import {CwdToolbarModule} from '../sei-cwd-common/cwd-toolbar';
import {EditorModule} from '../editor/editor.module';
import {WorkspaceModule} from '../workspace/workspace.module';
import {ExerciseTabComponent} from './component/exercise-details/exercise-tab/exercise-tab.component';
import {ExerciseNavigationContainerComponent} from './component/exercise-details/exercise-navigation-container/exercise-navigation-container.component';
import {ExerciseCollapseContainerComponent} from './component/exercise-details/exercise-collapse-container/exercise-collapse-container.component';
import {ConfirmDialogComponent} from '../sei-cwd-common/confirm-dialog/components/confirm-dialog.component';
import {NameDialogComponent} from '../sei-cwd-common/name-dialog/name-dialog.component';
import {DirectoryPanelComponent} from './component/exercise-details/exercise-navigation-container/directory-panel/directory-panel.component';
import { ExerciseBreadcrumbComponent } from './component/exercise-details/exercise-breadcrumb/exercise-breadcrumb.component';
import { CanDeactivateGuard  } from 'src/app/sei-cwd-common/cwd-route-guards/can-deactivate.guard';
import { FilesFilterPipe } from './pipes/files-filter-pipe';

const exerciseRoutes: Routes = [
  {path: 'exercises', component: ExerciseListContainerComponent, canActivate: [CwdAuthGuardService]},
  {path: 'exercises/:id', component: ExerciseCollapseContainerComponent, canActivate: [CwdAuthGuardService], canDeactivate: [CanDeactivateGuard]}
];


@NgModule({
  declarations: [
    ExerciseComponent,
    ExerciseListComponent,
    ExerciseListContainerComponent,
    ExerciseNavigationContainerComponent,
    ExerciseCollapseContainerComponent,
    ConfirmDialogComponent,
    NameDialogComponent,
    DirectoryPanelComponent,
    ExerciseBreadcrumbComponent,
    ExerciseTabComponent,
    FilesFilterPipe
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(exerciseRoutes),
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    FlexLayoutModule,
    MatButtonModule,
    MatSortModule,
    MatTooltipModule,
    MatTabsModule,
    RouterModule,
    ResizableModule,
    CwdToolbarModule,
    DirectoriesModule,
    WorkspaceModule,
    MatButtonToggleModule,
    MatExpansionModule,
    MatListModule,
    MatMenuModule,
    MatSidenavModule,
    MatBadgeModule,
    MatDialogModule,
    EditorModule
  ],
  exports: [
    ExerciseComponent,
    ExerciseListContainerComponent,
    ExerciseNavigationContainerComponent,
    ExerciseBreadcrumbComponent,
    ExerciseTabComponent
  ],
  entryComponents: [
    ConfirmDialogComponent,
    NameDialogComponent
  ]
})
export class ExerciseModule { }

