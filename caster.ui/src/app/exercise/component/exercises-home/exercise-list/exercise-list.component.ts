/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
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
  ViewChild
} from '@angular/core';
import {Exercise} from '../../../../generated/caster-api';
import {MatSort, MatSortable, MatTableDataSource} from '@angular/material';

@Component({
  selector: 'cas-exercise-list',
  templateUrl: './exercise-list.component.html',
  styleUrls: ['./exercise-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExerciseListComponent implements OnInit, OnChanges {
  @Input() exercises: Exercise[];
  @Input() isLoading: boolean;
  
  @Output() create: EventEmitter<string> = new EventEmitter<string>();
  @Output() update: EventEmitter<Exercise> = new EventEmitter<Exercise>();
  @Output() delete: EventEmitter<Exercise> = new EventEmitter<Exercise>();
  
  @ViewChild('createInput', {static: true}) createInput: HTMLInputElement;
  @ViewChild(MatSort, {static: true}) sort: MatSort;
  
  filterString = '';
  displayedColumns: string[] = ['name'];
  dataSource: MatTableDataSource<Exercise> = new MatTableDataSource();
  
  constructor() {
  }
 
  
  ngOnInit() {
    if (this.exercises) {
      this.dataSource = new MatTableDataSource(this.exercises);
      if (this.sort) {
      this.sort.disableClear = true;
      this.sort.sort(({id: 'name', start: 'asc'}) as MatSortable);
      this.dataSource.sort = this.sort;
      }
    }
  }
  
  ngOnChanges(changes: SimpleChanges): void {
    if (changes.exercises) {
      this.dataSource.data = changes.exercises.currentValue;
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

  deleteRequest(exercise: Exercise) {
    this.delete.emit(exercise);
  }
  
  updateRequest(exercise: Exercise) {
    this.update.emit(exercise);
  }

  createRequest() {
    this.create.emit();
  }

}

