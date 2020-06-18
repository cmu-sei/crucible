/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AdminContainerComponent } from './component/admin-container/admin-container.component';
import { UsersComponent } from './component/admin-users/users.component';
import { UserListComponent } from './component/admin-users/user-list/user-list.component';
import { FlexLayoutModule } from '@angular/flex-layout';
import { ProjectModule } from '../project/project.module';
import { RouterModule } from '@angular/router';
import { ClipboardModule } from 'ngx-clipboard';
import { AdminModuleListComponent } from './component/admin-modules/modules-list/module-list.component';
import { AdminModulesComponent } from './component/admin-modules/modules.component';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSortModule } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTreeModule } from '@angular/material/tree';
import { AdminWorkspacesComponent } from './component/admin-workspaces/admin-workspaces.component';
import { LockingStatusComponent } from './component/admin-workspaces/locking-status/locking-status.component';
import { ActiveRunsComponent } from './component/admin-workspaces/active-runs/active-runs.component';
import { CwdTableModule } from '../sei-cwd-common/cwd-table/cwd-table.module';
import { WorkspaceModule } from '../workspace/workspace.module';

@NgModule({
  declarations: [
    AdminContainerComponent,
    AdminModuleListComponent,
    AdminModulesComponent,
    UsersComponent,
    UserListComponent,
    AdminWorkspacesComponent,
    LockingStatusComponent,
    ActiveRunsComponent,
  ],
  imports: [
    ClipboardModule,
    CommonModule,
    ProjectModule,
    FlexLayoutModule,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSidenavModule,
    MatSortModule,
    MatTooltipModule,
    MatTreeModule,
    RouterModule,
    MatSlideToggleModule,
    CwdTableModule,
    WorkspaceModule,
  ],
  exports: [
    AdminContainerComponent,
    MatPaginatorModule,
    UsersComponent,
    UserListComponent,
  ],
})
export class AdminAppModule {}
