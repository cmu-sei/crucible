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
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  ViewEncapsulation,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { Module, CreateSnippetCommand } from '../../../generated/caster-api';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { ModuleVariablesComponent } from '../module-variables/module-variables.component';

@Component({
  selector: 'cas-module-list',
  templateUrl: './module-list.component.html',
  styleUrls: ['./module-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class ModuleListComponent implements OnChanges, OnInit {
  @Input() modules: Module[];
  @Input() selectedModule: Module;
  @Input() isEditing: boolean;

  @Output() insertModule: EventEmitter<CreateSnippetCommand> = new EventEmitter<
    CreateSnippetCommand
  >();
  @Output() getModule: EventEmitter<{ id: string }> = new EventEmitter();
  code: string;
  dataSource = new MatTableDataSource();
  filterString = '';
  isDialogOpen = false;
  variablesDialogRef: MatDialogRef<ModuleVariablesComponent>;

  @ViewChild('variablesDialog') variablesTemplate: TemplateRef<
    ModuleVariablesComponent
  >;

  constructor(public dialog: MatDialog) {}

  ngOnInit(): void {
    this.dataSource.data = this.modules;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.modules) {
      this.dataSource.data = changes.modules.currentValue;
    }

    if (
      !this.isDialogOpen &&
      changes.selectedModule &&
      !!this.selectedModule &&
      !!this.selectedModule.versions &&
      this.selectedModule.versions.length > 0
    ) {
      this.isDialogOpen = true;
      this.variablesDialogRef = this.dialog.open(this.variablesTemplate, {
        disableClose: true,
      });
    }
  }

  variablesSelected(command: CreateSnippetCommand) {
    if (this.variablesDialogRef) {
      this.variablesDialogRef.close();
      this.isDialogOpen = false;
    }

    if (command) {
      this.insertModule.emit(command);
    }
  }

  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.dataSource.filter = filterValue;
  }

  clearFilter() {
    this.applyFilter('');
  }

  selectModuleFn(module) {
    this.getModule.emit({ id: module.id });
  }
}
