/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {CommonModule} from '@angular/common';
import {AdminContainerComponent} from './component/admin-container/admin-container.component';
import {UsersComponent} from './component/admin-users/users.component';
import {UserListComponent} from './component/admin-users/user-list/user-list.component';
import {FlexLayoutModule} from '@angular/flex-layout';
import {ProjectModule} from '../project/project.module';
import {RouterModule} from '@angular/router';
import { ClipboardModule } from 'ngx-clipboard';
import { AdminModuleListComponent } from './component/admin-modules/modules-list/module-list.component';
import { AdminModulesComponent } from './component/admin-modules/modules.component';
import {
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
} from '@angular/material';

@NgModule({
  declarations: [
    AdminContainerComponent,
    AdminModuleListComponent,
    AdminModulesComponent,
    UsersComponent,
    UserListComponent
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
    RouterModule
  ],
  exports: [
    AdminContainerComponent,
    MatPaginatorModule,
    UsersComponent,
    UserListComponent
  ]
})
export class AdminAppModule { }

