/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, ViewChild } from '@angular/core';
import { ExerciseData } from '../../../models/exercise-data';
import { ExercisesService } from '../../../services/exercises/exercises.service';
import { MatTableDataSource, MatSort, MatSortable } from '@angular/material';
import { LoggedInUserService } from '../../../services/logged-in-user/logged-in-user.service';


@Component({
  selector: 'app-exercise-list',
  templateUrl: './exercise-list.component.html',
  styleUrls: ['./exercise-list.component.css']
})
export class ExerciseListComponent implements OnInit {

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  public exerciseDataSource: MatTableDataSource<ExerciseData>;
  public displayedColumns: string[] = ['name', 'teamName', 'description'];

  public filterString: string;
  public isLoading: Boolean;


  constructor(
    private exercisesService: ExercisesService,
    private loggedInUserService: LoggedInUserService
  ) { }

  /**
   * Initalization
   */
  ngOnInit() {

    this.filterString = '';

    // Initial datasource
    this.exerciseDataSource = new MatTableDataSource<ExerciseData>(new Array<ExerciseData>());
    this.sort.sort(<MatSortable>({id: 'name', start: 'asc'}));
    this.exerciseDataSource.sort = this.sort;

    // Subscribe to the service
    this.isLoading = true;
    this.exercisesService.exerciseList.subscribe(exercises => {
      this.exerciseDataSource.data = exercises;
      this.isLoading = false;
    });

    // Tell the service to update once a user is officially logged in
    this.loggedInUserService.loggedInUser.subscribe(loggedInUser => {
      if (loggedInUser == null) {
        return;
      }
      this.exercisesService.getExerciseList(loggedInUser.id);
    });

  }

  /**
   * Called by UI to add a filter to the exerciseDataSource
   * @param filterValue
   */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.exerciseDataSource.filter = filterValue;
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

}






