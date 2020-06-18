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
import { CommonModule } from '@angular/common';
import { CwdTableComponent } from './components/cwd-table/cwd-table.component';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSortModule } from '@angular/material/sort';
import { FlexModule, ExtendedModule } from '@angular/flex-layout';
import { FormsModule } from '@angular/forms';
import { TableItemContentDirective } from './directives/table-item-content.directive';
import { TableActionDirective } from './directives/table-action.directive';
import { TableItemActionDirective } from './directives/table-item-action.directive';

@NgModule({
  declarations: [
    CwdTableComponent,
    TableItemContentDirective,
    TableActionDirective,
    TableItemActionDirective,
  ],
  imports: [
    CommonModule,
    MatIconModule,
    MatFormFieldModule,
    MatPaginatorModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatInputModule,
    MatSortModule,
    FlexModule,
    ExtendedModule,
    FormsModule,
  ],
  exports: [
    CwdTableComponent,
    TableItemContentDirective,
    TableActionDirective,
    TableItemActionDirective,
  ],
})
export class CwdTableModule {}
