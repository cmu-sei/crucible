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
  ViewChild,
} from '@angular/core';
import { Project } from '../../../../generated/caster-api';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'cas-project-list',
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectListComponent implements OnInit, OnChanges {
  @Input() projects: Project[];
  @Input() isLoading: boolean;

  @Output() create: EventEmitter<string> = new EventEmitter<string>();
  @Output() update: EventEmitter<Project> = new EventEmitter<Project>();
  @Output() delete: EventEmitter<Project> = new EventEmitter<Project>();

  @ViewChild('createInput', { static: true }) createInput: HTMLInputElement;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  filterString = '';
  displayedColumns: string[] = ['name'];
  dataSource: MatTableDataSource<Project> = new MatTableDataSource();

  constructor() {}

  ngOnInit() {
    if (this.projects) {
      this.dataSource = new MatTableDataSource(this.projects);
      if (this.sort) {
        this.sort.disableClear = true;
        this.sort.sort({ id: 'name', start: 'asc' } as MatSortable);
        this.dataSource.sort = this.sort;
      }
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.projects) {
      this.dataSource.data = changes.projects.currentValue;
    }
    if (changes.isLoading) {
      this.isLoading = changes.isLoading.currentValue;
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

  deleteRequest(project: Project) {
    this.delete.emit(project);
  }

  updateRequest(project: Project) {
    this.update.emit(project);
  }

  createRequest() {
    this.create.emit();
  }
}
