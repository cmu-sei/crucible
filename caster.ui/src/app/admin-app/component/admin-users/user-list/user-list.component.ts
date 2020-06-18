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
  Component,
  EventEmitter,
  OnInit,
  ViewChild,
  Input,
  Output,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { MatDialogRef, MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { ConfirmDialogComponent } from 'src/app/sei-cwd-common/confirm-dialog/components/confirm-dialog.component';
import {
  User,
  Permission,
  PermissionsService,
  UserPermission,
  UserPermissionsService,
} from '../../../../generated/caster-api';
import { Subject, Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import {
  fromMatSort,
  sortRows,
  fromMatPaginator,
  paginateRows,
} from 'src/app/datasource-utils';

const WAS_CANCELLED = 'wasCancelled';

export interface Action {
  Value: string;
  Text: string;
}

@Component({
  selector: 'cas-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css'],
})
export class UserListComponent implements OnInit, OnChanges {
  public displayedColumns: string[] = ['id', 'name', 'permissions'];
  public filterString = '';
  public savedFilterString = '';
  public userDataSource = new MatTableDataSource<User>(new Array<User>());
  public newUser: User = { permissions: [] };

  // MatPaginator Output
  public defaultPageSize = 10;
  public pageEvent: PageEvent;
  public addingNewUser: boolean;
  displayedRows$: Observable<User[]>;
  totalRows$: Observable<number>;
  sortEvents$: Observable<Sort>;
  pageEvents$: Observable<PageEvent>;

  @Input() users: User[];
  @Input() isLoading: boolean;
  @Input() permissions: Permission[] = [];
  @Output() create: EventEmitter<User> = new EventEmitter<User>();
  @Output() delete: EventEmitter<string> = new EventEmitter<string>();
  @Output() addUserPermission: EventEmitter<UserPermission> = new EventEmitter<
    UserPermission
  >();
  @Output() removeUserPermission: EventEmitter<
    UserPermission
  > = new EventEmitter<UserPermission>();
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor(
    // public dialogService: DialogService,
    private dialog: MatDialog
  ) {}

  /**
   * Initialization
   */
  ngOnInit() {
    this.sortEvents$ = fromMatSort(this.sort);
    this.pageEvents$ = fromMatPaginator(this.paginator);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!!changes.users && !!changes.users.currentValue) {
      this.userDataSource.data = changes.users.currentValue;
      this.filterAndSort(this.filterString);
    }
  }

  /**
   * Called by UI to add a filter to the userDataSource
   * @param filterValue
   */
  applyFilter(filterValue: string) {
    this.filterString = filterValue.toLowerCase();
    this.filterAndSort(this.filterString);
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

  /**
   * filters and sorts the displayed rows
   */
  filterAndSort(filterValue: string) {
    this.userDataSource.filter = filterValue;
    const rows$ = of(this.userDataSource.filteredData);
    this.totalRows$ = rows$.pipe(map((rows) => rows.length));
    if (!!this.sortEvents$ && !!this.pageEvents$) {
      this.displayedRows$ = rows$.pipe(
        sortRows(this.sortEvents$),
        paginateRows(this.pageEvents$)
      );
    }
  }

  /**
   * Adds a new user
   */
  addNewUser(addUser: boolean) {
    if (addUser) {
      const user = {
        id: this.newUser.id,
        name: this.newUser.name,
        permissions: [],
      };
      this.savedFilterString = this.filterString;
      this.create.emit(user);
    } else {
      this.newUser = { permissions: [] };
    }
  }

  deleteUser(user: User) {
    this.confirmDialog('Delete ' + user.name + '?', user.id, {
      buttonTrueText: 'Delete',
    }).subscribe((result) => {
      if (!result[WAS_CANCELLED]) {
        this.delete.emit(user.id);
      }
    });
  }

  toggleUserPermission(user: User, permissionId: string) {
    const userPermission: UserPermission = {
      userId: user.id,
      permissionId: permissionId,
    };
    if (this.hasPermission(permissionId, user)) {
      this.removeUserPermission.emit(userPermission);
    } else {
      this.addUserPermission.emit(userPermission);
    }
  }
  hasPermission(permissionId: string, user: User) {
    return (
      !!user.permissions &&
      user.permissions.some((p) => {
        return p.id === permissionId;
      })
    );
  }

  confirmDialog(
    title: string,
    message: string,
    data?: any
  ): Observable<boolean> {
    let dialogRef: MatDialogRef<ConfirmDialogComponent>;
    dialogRef = this.dialog.open(ConfirmDialogComponent, { data: data || {} });
    dialogRef.componentInstance.title = title;
    dialogRef.componentInstance.message = message;

    return dialogRef.afterClosed();
  }
}
