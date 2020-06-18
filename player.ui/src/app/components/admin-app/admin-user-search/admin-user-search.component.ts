/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { MatTableDataSource, PageEvent, MatPaginator, MatSort, MatSortable } from '@angular/material';
import { SUPER_USER } from '../../../services/logged-in-user/logged-in-user.service';
import { User, UserService, RoleService, Role } from '../../../generated/s3.player.api';

export interface Action {
  Value: string;
  Text: string;
}

@Component({
  selector: 'app-admin-user-search',
  templateUrl: './admin-user-search.component.html',
  styleUrls: ['./admin-user-search.component.css']
})
export class AdminUserSearchComponent implements OnInit, AfterViewInit {

  public displayedColumns: string[] = ['name', 'roleName'];
  public isSuperUser: boolean;
  public filterString: string;
  public superUserRole: Role;

  public editUserText = 'Edit User';
  public userToEdit: User;
  public userDataSource = new MatTableDataSource<User>(new Array<User>());


  // MatPaginator Output
  public defaultPageSize = 10;
  public pageEvent: PageEvent;
  public uploading = false;
  public uploadProgress = 0;
  public isLoading: Boolean;

  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor(
    private userService: UserService,
    private roleService: RoleService
  ) { }

  /**
   * Initialization
   */
  ngOnInit() {

    this.sort.sort(<MatSortable>({id: 'name', start: 'asc'}));
    this.userDataSource.sort = this.sort;

    this.pageEvent = new PageEvent();
    this.pageEvent.pageIndex = 0;
    this.pageEvent.pageSize = this.defaultPageSize;
    this.isLoading = false;

    // Initial datasource
    this.isSuperUser = false;
    this.filterString = '';

    // Get the superUser Role for the action later
    this.roleService.getRoles()
      .subscribe(roles => {
        this.superUserRole = roles.find(r => r.name === SUPER_USER);
      });

    this.refreshUsers();
  }

  /**
   * Called after the components initialized
   */
  ngAfterViewInit() {
    this.userDataSource.paginator = this.paginator;
  }


  /**
   * permission list for display
   */
  permissionsString(permissions) {
    let val = permissions.map(p => p.key).join(', ');
    if (val.length > 50) {
      val = val.substring(0, 50) + ' ...';
    }
    return val;
  }


  /**
     * Called by UI to add a filter to the viewDataSource
     * @param filterValue
     */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    this.pageEvent.pageIndex = 0;
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.userDataSource.filter = filterValue;
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

  /**
   * Refreshes the users list and updates the mat table control
   */
  refreshUsers() {
    this.userToEdit = undefined;
    this.isLoading = true;
    this.userService.getUsers().subscribe(users => {
      this.userDataSource.data = users;
      this.isLoading = false;
    });
  }




}
