/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { MatTableDataSource, MatPaginator, PageEvent, MatSort, Sort, MatDialog, MatDialogConfig } from '@angular/material';
import { HttpEventType } from '@angular/common/http';
import { Scenario, ScenarioService, Session } from 'src/app/swagger-codegen/dispatcher.api';
import { ScenarioEditComponent } from 'src/app/components/scenarios/scenario-edit/scenario-edit.component';
import { ScenarioEditDialogComponent } from 'src/app/components/scenarios/scenario-edit-dialog/scenario-edit-dialog.component';
import { SessionEditDialogComponent } from 'src/app/components/sessions/session-edit-dialog/session-edit-dialog.component';
import { DialogService } from 'src/app/services/dialog/dialog.service';
import {Subject} from 'rxjs/Subject';
import { Observable  } from 'rxjs/Observable';
import { of  } from 'rxjs/observable/of';
import { map } from 'rxjs/operators';
import { fromMatSort, sortRows, fromMatPaginator, paginateRows } from 'src/app/datasource-utils';

export interface Action {
  Value: string;
  Text: string;
}

@Component({
  selector: 'app-scenario-list',
  templateUrl: './scenario-list.component.html',
  styleUrls: ['./scenario-list.component.css']
})
export class ScenarioListComponent implements OnInit {

  public displayedColumns: string[] = ['name', 'description', 'durationHours'];
  public filterString: string;

  public editScenarioText = 'Edit Scenario';
  public scenarioToEditId = '';
  public scenarioDataSource = new MatTableDataSource<Scenario>(new Array<Scenario>());


  // MatPaginator Output
  public defaultPageSize = 10;
  public pageEvent: PageEvent;
  public isLoading: Boolean;
  displayedRows$: Observable<Scenario[]>;
  totalRows$: Observable<number>;
  sortEvents$: Observable<Sort>;
  pageEvents$: Observable<PageEvent>;

  @Input() refresh: Subject<boolean>;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild(ScenarioEditComponent) scenarioEditComponent: ScenarioEditComponent;

  constructor(
    private scenarioService: ScenarioService,
    public dialogService: DialogService,
    private dialog: MatDialog
  ) { }

  /**
   * Initialization
   */
  ngOnInit() {
    this.sortEvents$ = fromMatSort(this.sort);
    this.pageEvents$ = fromMatPaginator(this.paginator);
    this.refresh.subscribe(shouldRefresh => {
      if (shouldRefresh) {
        this.refreshScenarios();
      }
    });
    this.refreshScenarios();
  }

  /**
     * Called by UI to add a filter to the exerciseDataSource
     * @param filterValue
     */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.scenarioDataSource.filter = filterValue;
    this.filterAndSort();
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

  /**
   * Refreshes the scenarios list and updates the mat table control
   */
  public refreshScenarios() {
    this.isLoading = true;
    this.scenarioService.getScenarios().subscribe(scenarios => {
      this.scenarioDataSource.data = scenarios;
      this.filterAndSort();
      this.isLoading = false;
    });
  }

  /**
   * filters and sorts the displayed rows
   */
  filterAndSort() {
    const rows$ = of(this.scenarioDataSource.filteredData);
    this.totalRows$ = rows$.pipe(map(rows => rows.length));
    this.displayedRows$ = rows$.pipe(sortRows(this.sortEvents$), paginateRows(this.pageEvents$));
  }

  /**
   * Adds a new scenario
   */
  addNewScenario() {
    const scenario = <Scenario>{name: 'New Scenario', description: 'Add description'};
    this.scenarioService.createScenario(scenario).subscribe(newScenario => {
      this.refreshScenarios();
      const dialogRef = this.dialog.open(ScenarioEditDialogComponent, {
        width: '800px',
        data: { scenario: newScenario }
      });
      dialogRef.componentInstance.editComplete.subscribe(result => {
        this.refreshScenarios();
        dialogRef.close();
      });
    });
  }

  /**
   * Edit the Session created from a Scenario
   */
  editNewSession(session: Session) {
    const dialogRef = this.dialog.open(SessionEditDialogComponent, {
      width: '800px',
      data: { scenario: session }
    });
    dialogRef.componentInstance.editComplete.subscribe((newSession) => {
      dialogRef.close();
    });
  }

}


