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
import { Session, SessionService } from 'src/app/swagger-codegen/dispatcher.api';
import { SessionEditComponent } from 'src/app/components/sessions/session-edit/session-edit.component';
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
  selector: 'app-session-list',
  templateUrl: './session-list.component.html',
  styleUrls: ['./session-list.component.css']
})
export class SessionListComponent implements OnInit {

  public displayedColumns: string[] = ['name', 'exercise', 'status', 'startDate', 'endDate', 'description'];
  public filterString: string;
  showActive = true;
  showReady = true;
  showEnded = false;
  public editSessionText = 'Edit Session';
  public sessionToEdit: Session;
  public sessionDataSource = new MatTableDataSource<Session>(new Array<Session>());


  // MatPaginator Output
  public defaultPageSize = 10;
  public pageEvent: PageEvent;
  public isLoading: Boolean;
  displayedRows$: Observable<Session[]>;
  totalRows$: Observable<number>;
  sortEvents$: Observable<Sort>;
  pageEvents$: Observable<PageEvent>;

  @Input() refresh: Subject<boolean>;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild(SessionEditComponent) sessionEditComponent: SessionEditComponent;

  constructor(
    private sessionService: SessionService,
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
        this.refreshSessions();
      }
    });
    this.refreshSessions();
  }

  /**
     * Called by UI to add a filter to the exerciseDataSource
     * @param filterValue
     */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.sessionDataSource.filter = filterValue;
    this.filterAndSort();
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

  /**
   * Refreshes the sessions list and updates the mat table control
   */
  public refreshSessions() {
    this.isLoading = true;
    this.sessionToEdit = undefined;
    this.sessionService.getSessions().subscribe(sessions => {
      sessions.forEach(session => {
        session.startDate = new Date(session.startDate + 'Z');
        session.endDate = new Date(session.endDate + 'Z');
      });
      this.sessionDataSource.data = sessions;
      this.filterAndSort();
      this.isLoading = false;
    });
  }

  /**
   * filters and sorts the displayed rows
   */
  filterAndSort() {
    const rows$ = of(this.selectSessions(this.sessionDataSource.filteredData));
    this.totalRows$ = rows$.pipe(map(rows => rows.length));
    this.displayedRows$ = rows$.pipe(sortRows(this.sortEvents$), paginateRows(this.pageEvents$));
  }

  /**
   * filters the implementations by status (active, ended, failed)
   */
  selectSessions(sessions) {
    let selectedSessions = new Array<Session>();
    if (this.showActive) {
      selectedSessions = selectedSessions.concat(sessions.filter(s => s.status === 'active'));
    }
    if (this.showEnded) {
      selectedSessions = selectedSessions.concat(sessions.filter(s => s.status === 'ended'));
    }
    if (this.showReady) {
      selectedSessions = selectedSessions.concat(sessions.filter(s => s.status === 'ready'));
    }
    return selectedSessions;
  }

  /**
   * Executes an action menu item
   * @param action: action string to case from
   * @param sessionGuid: The guid for exercise
   */
  executeSessionAction(action: string, sessionGuid: string) {
    switch (action) {
      case ('edit'): {
        // Edit exercise
        this.sessionService.getSession(sessionGuid)
          .subscribe(session => {
            const dialogRef = this.dialog.open(SessionEditComponent);
            dialogRef.afterOpened().subscribe(r => {
              session.startDate = new Date(session.startDate + 'Z');
              session.endDate = new Date(session.endDate + 'Z');
              dialogRef.componentInstance.session = session;
            });

            dialogRef.componentInstance.editComplete.subscribe((newSession) => {
              dialogRef.close();
              this.refreshSessions();
              if (!!newSession) {
                this.executeSessionAction('edit', newSession.id);
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
   * Adds a new session
   */
  addNewSession() {
    const startDate = new Date();
    startDate.setDate(startDate.getDate() + 1);
    startDate.setHours(8, 0, 0, 0);
    const endDate = new Date(startDate);
    endDate.setMonth(startDate.getMonth() + 1);
    const session = {name: 'New Session', description: 'Add description', status: 'ready', startDate: startDate, endDate: endDate};
    this.sessionService.createSession(<Session>session).subscribe(newSession => {
      this.refreshSessions();
      const dialogRef = this.dialog.open(SessionEditDialogComponent, {
        width: '800px',
        data: { session: newSession }
      });
      dialogRef.componentInstance.editComplete.subscribe(result => {
        this.refreshSessions();
        dialogRef.close();
      });
    });
  }

}


