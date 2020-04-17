/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, ViewChild, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatTableDataSource, MatPaginator, PageEvent, MatSort, Sort, MatDialog } from '@angular/material';
import { HttpEventType } from '@angular/common/http';
import { Scenario } from 'src/app/data/scenario/scenario.store';
import { View } from 'src/app/services/data/player-data-service';
import { ScenarioDataService } from 'src/app/data/scenario/scenario-data.service';
import { ScenarioEditComponent } from 'src/app/components/scenarios/scenario-edit/scenario-edit.component';
import { ScenarioEditDialogComponent } from 'src/app/components/scenarios/scenario-edit-dialog/scenario-edit-dialog.component';
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

  @Input() scenarioList: Scenario[];
  @Input() selectedScenario: Scenario;
  @Input() pageSize: number;
  @Input() pageIndex: number;
  @Input() isLoading: boolean;
  @Input() filterControl: FormControl;
  @Input() filterString: string;
  @Input() views: Observable<View[]>;
  @Input() statuses: string;
  @Output() saveScenario = new EventEmitter<Scenario>();
  @Output() setActive = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<Sort>();
  @Output() pageChange = new EventEmitter<PageEvent>();
  @Output() filterStatusChange = new EventEmitter<any>();

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild(ScenarioEditComponent) scenarioEditComponent: ScenarioEditComponent;

  displayedColumns: string[] = ['name', 'view', 'status', 'startDate', 'endDate', 'description'];
  showStatus = { active: true, ready: true, ended: false };
  editScenarioText = 'Edit Scenario';
  scenarioToEdit: Scenario;
  scenarioDataSource = new MatTableDataSource<Scenario>(new Array<Scenario>());

  // MatPaginator Output
  defaultPageSize = 10;
  pageEvent: PageEvent;
  displayedRows$: Observable<Scenario[]>;
  totalRows$: Observable<number>;
  sortEvents$: Observable<Sort>;
  pageEvents$: Observable<PageEvent>;

  constructor(
    private scenarioDataService: ScenarioDataService,
    public dialogService: DialogService,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.showStatus.active = this.statuses.indexOf('active') > -1;
    this.showStatus.ended = this.statuses.indexOf('ended') > -1;
    this.showStatus.ready = this.statuses.indexOf('ready') > -1;
    this.filterControl.setValue(this.filterString);
  }

  clearFilter() {
    this.filterControl.setValue('');
  }

  /**
   * Edits or adds a scenario
   */
  editScenario(scenarioToEdit: Scenario) {
    let scenario: Scenario;
    if (!scenarioToEdit) {
      const startDate = new Date();
      startDate.setHours(startDate.getHours() + 1);
      const endDate = new Date(startDate);
      endDate.setHours(endDate.getHours() + 4);
      scenario = {
        name: '',
        description: '',
        startDate: startDate,
        endDate: endDate,
        status: 'ready'
      };
    } else {
      scenario = {...scenarioToEdit};
    }
    const dialogRef = this.dialog.open(ScenarioEditDialogComponent, {
      width: '800px',
      data: {scenario: scenario, views: this.views}
    });
    dialogRef.componentInstance.editComplete.subscribe(result => {
      if (result.saveChanges && result.scenario) {
        this.saveScenario.emit(result.scenario);
      }
      dialogRef.close();
    });
  }

  /**
   * Delete a scenario after confirmation
   */
  deleteScenario(scenario: Scenario): void {
    this.dialogService.confirm('Delete Scenario', 'Are you sure that you want to delete scenario ' + scenario.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.scenarioDataService.delete(scenario.id);
        }
      });
  }

  /**
   * Copy a scenario after confirmation
   */
  copyScenario(scenario: Scenario): void {
    this.dialogService.confirm('Copy Scenario', 'Are you sure that you want to copy scenario ' + scenario.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.scenarioDataService.copyScenario(scenario.id);
        }
      });
  }

  /**
   * Start a scenario
   */
  startScenario(scenario: Scenario): void {
    this.dialogService.confirm('Start Scenario Now', 'Are you sure that you want to start scenario ' + scenario.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.scenarioDataService.start(scenario.id);
        }
      });
  }

  /**
   * End a scenario
   */
  endScenario(scenario: Scenario): void {
    this.dialogService.confirm('End Scenario Now', 'Are you sure that you want to end scenario ' + scenario.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.scenarioDataService.end(scenario.id);
        }
      });
  }

  /**
   * filters and sorts the displayed rows
   */
  filterStatus() {
    this.filterStatusChange.emit(this.showStatus);
  }

  selectScenario(scenarioId: string) {
    if (!!this.selectedScenario && scenarioId === this.selectedScenario.id) {
      this.setActive.emit('');
    } else {
      this.setActive.emit(scenarioId);
    }
  }

  paginateScenarios(scenarios: Scenario[], pageIndex: number, pageSize: number) {
    if (!scenarios) {
      return [];
    }
    const startIndex = pageIndex * pageSize;
    const copy = scenarios.slice();
    return copy.splice(startIndex, pageSize);
  }

  paginatorEvent(page: PageEvent) {
    this.pageChange.emit(page);
  }

  sortChanged(sort: Sort) {
    this.sortChange.emit(sort);
  }

}


