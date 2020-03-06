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
import { MatTableDataSource, MatPaginator, PageEvent, MatSort, Sort, MatDialog } from '@angular/material';
import { HttpEventType } from '@angular/common/http';
import { Implementation, ImplementationService } from 'src/app/swagger-codegen/alloy.api';
import { ImplementationEditComponent } from '../implementation-edit/implementation-edit.component';
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
  selector: 'app-implementation-list',
  templateUrl: './implementation-list.component.html',
  styleUrls: ['./implementation-list.component.css']
})
export class ImplementationListComponent implements OnInit {

  public displayedColumns: string[] = ['name', 'username', 'status', 'statusDate', 'launchDate', 'expirationDate'];
  public filterString: string;

  public editImplementationText = 'Edit Implementation';
  public implementationToEdit: Implementation;
  public implementationDataSource = new MatTableDataSource<Implementation>(new Array<Implementation>());
  public activeImplementations = new Array<Implementation>();
  public failedImplementations = new Array<Implementation>();
  public endedImplementations = new Array<Implementation>();
  public showActive = true;
  public showFailed = false;
  public showEnded = false;


  // MatPaginator Output
  public defaultPageSize = 10;
  public pageEvent: PageEvent;
  public isLoading: Boolean;
  displayedRows$: Observable<Implementation[]>;
  totalRows$: Observable<number>;
  sortEvents$: Observable<Sort>;
  pageEvents$: Observable<PageEvent>;

  @Input() refresh: Subject<boolean>;
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;
  @ViewChild(ImplementationEditComponent, { static: true }) implementationEditComponent: ImplementationEditComponent;

  constructor(
    private implementationService: ImplementationService,
    public dialogService: DialogService,
    private dialog: MatDialog
  ) { }

  /**
   * Initialization
   */
  ngOnInit() {
    this.sortEvents$ = fromMatSort(this.sort);
    this.pageEvents$ = fromMatPaginator(this.paginator);
    // this.implementationDataSource.filterPredicate = (data: Implementation, filterString: string) => {
    //   return this.customImplementationFilter(data, filterString);
    // };
    this.refresh.subscribe(shouldRefresh => {
      if (shouldRefresh) {
        this.refreshImplementations();
      }
    });
    this.refreshImplementations();
  }

  /**
   * Defines the custom filterPredicate to filter the implementations
   */
  // customImplementationFilter(data: Implementation, filterString: string) {
  //   return data.status.toLowerCase().includes(filterString.toLowerCase());
  // }

  /**
     * Called by UI to add a filter to the exerciseDataSource
     * @param filterValue
     */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.implementationDataSource.filter = filterValue;
    this.filterAndSort();
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

  /**
   * Refreshes the implementations list and updates the mat table control
   */
  public refreshImplementations() {
    this.isLoading = true;
    this.implementationToEdit = undefined;
    this.implementationService.getImplementations().subscribe(implementations => {
      this.activeImplementations.length = 0;
      this.endedImplementations.length = 0;
      this.failedImplementations.length = 0;
      implementations.forEach(implementation => {
        implementation.launchDate = !implementation.launchDate ? null : new Date(implementation.launchDate + 'Z');
        implementation.endDate = !implementation.endDate ? null : new Date(implementation.endDate + 'Z');
        implementation.expirationDate = !implementation.expirationDate ? null : new Date(implementation.expirationDate + 'Z');
        implementation.statusDate = !implementation.statusDate ? null : new Date(implementation.statusDate + 'Z');
        switch (implementation.status) {
          case ('Failed'): {
            this.failedImplementations.push(implementation);
            break;
          }
          case ('Ended'):
          case ('Expired'): {
            this.endedImplementations.push(implementation);
            break;
          }
          default: {
            this.activeImplementations.push(implementation);
            break;
          }
        }
      });
      this.filterAndSort();
      this.isLoading = false;
    });
  }

  /**
   * filters and sorts the displayed rows
   */
  filterAndSort() {
    this.implementationDataSource.data = this.selectImplementations();
    const rows$ = of(this.implementationDataSource.filteredData);
    this.totalRows$ = rows$.pipe(map(rows => rows.length));
    this.displayedRows$ = rows$.pipe(sortRows(this.sortEvents$), paginateRows(this.pageEvents$));
  }

  /**
   * filters the implementations by status (active, ended, failed)
   */
  selectImplementations() {
    let selectedImplementations = new Array<Implementation>();
    if (this.showActive) {
      selectedImplementations = selectedImplementations.concat(this.activeImplementations);
    }
    if (this.showEnded) {
      selectedImplementations = selectedImplementations.concat(this.endedImplementations);
    }
    if (this.showFailed) {
      selectedImplementations = selectedImplementations.concat(this.failedImplementations);
    }
    return selectedImplementations;
  }

  /**
   * Executes an action menu item
   * @param action: action string to case from
   * @param implementationGuid: The guid for exercise
   */
  executeImplementationAction(action: string, implementationGuid: string) {
    switch (action) {
      case ('edit'): {
        // Edit exercise
        this.implementationService.getImplementation(implementationGuid)
          .subscribe(implementation => {
            const dialogRef = this.dialog.open(ImplementationEditComponent);
            dialogRef.afterOpened().subscribe(r => {
              implementation.launchDate = new Date(implementation.launchDate + 'Z');
              implementation.endDate = new Date(implementation.endDate + 'Z');
              dialogRef.componentInstance.implementation = implementation;
            });

            dialogRef.componentInstance.editComplete.subscribe((newImplementation) => {
              dialogRef.close();
              this.refreshImplementations();
              if (!!newImplementation) {
                this.executeImplementationAction('edit', newImplementation.id);
              }
            });
        });
        break;
      }
      default: {
        alert('Unknown Action');
        break;
      }
    }
  }


  /**
   * Adds a new implementation
   */
  addNewImplementation() {
    const startDate = new Date();
    startDate.setDate(startDate.getDate() + 1);
    startDate.setHours(8, 0, 0, 0);
    const endDate = new Date(startDate);
    endDate.setMonth(startDate.getMonth() + 1);
    const implementation = {name: 'New Implementation', description: 'Add description', status: 'ready', startDate: startDate, endDate: endDate};
    this.implementationService.createImplementation(<Implementation>implementation).subscribe(ex => {
      this.refreshImplementations();
      this.executeImplementationAction('edit', ex.id);
    });
  }

}


