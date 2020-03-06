/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnDestroy } from '@angular/core';
import { FormControl } from '@angular/forms';
import {Sort} from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import { Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import { User, Permission, UserPermission } from 'src/app/swagger-codegen/dispatcher.api/model/models';
import { PermissionService, UserPermissionService } from 'src/app/swagger-codegen/dispatcher.api/api/api';
import {Router, ActivatedRoute} from '@angular/router';
import { AdminUsersService } from '../admin-users.service';

@Component({
  selector: 'app-admin-container',
  templateUrl: './admin-container.component.html',
  styleUrls: ['./admin-container.component.css']
})
export class AdminContainerComponent implements OnDestroy {
  loggedInUser = this.adminUsersService.loggedInUser;
  usersText = 'Users';
  showSection: Observable<string>;
  isSidebarOpen = true;
  isSuperUser = this.adminUsersService.isSuperUser();
  userList: Observable<User[]>;
  filterControl: FormControl = this.adminUsersService.filterControl;
  permissionList: Observable<Permission[]>;
  pageSize: Observable<number>;
  pageIndex: Observable<number>;
  private unsubscribe$ = new Subject();

  constructor(
    private router: Router,
    private adminUsersService: AdminUsersService,
    activatedRoute: ActivatedRoute,
    private permissionService: PermissionService,
    private userPermissionService: UserPermissionService
  ) {
    this.userList = this.adminUsersService.userList;
    this.permissionList = this.permissionService.getPermissions();
    this.pageSize = activatedRoute.queryParamMap.pipe(
      map(params => (parseInt(params.get('pagesize') || '20', 10)))
    );
    this.pageIndex = activatedRoute.queryParamMap.pipe(
      map(params => (parseInt(params.get('pageindex') || '0', 10)))
    );
    this.showSection = activatedRoute.queryParamMap.pipe(
      map(params => params.get('section') || this.usersText)
    );
    this.adminUsersService.getUsersFromApi().pipe(takeUntil(this.unsubscribe$)).subscribe();
    this.adminUsersService.getPermissionsFromApi().pipe(takeUntil(this.unsubscribe$)).subscribe();
    this.gotoUserSection();
  }

  gotoUserSection() {
    this.router.navigate([], { queryParams: { section: this.usersText }, queryParamsHandling: 'merge'});
  }

  logout() {
    this.adminUsersService.logout();
  }

  selectUser(userId: string) {
    this.router.navigate([], { queryParams: { selectedUser: userId }, queryParamsHandling: 'merge'});
  }

  addUserHandler(user: User) {
    this.adminUsersService.addUser(user);
  }

  deleteUserHandler(user: User) {
    this.adminUsersService.deleteUser(user);
  }

  addUserPermissionHandler(userPermission: UserPermission) {
    this.adminUsersService.addUserPermission(userPermission);
  }

  removeUserPermissionHandler(userPermission: UserPermission) {
    this.adminUsersService.deleteUserPermission(userPermission);
  }

  sortChangeHandler(sort: Sort) {
    this.router.navigate([], { queryParams: { sorton: sort.active, sortdir: sort.direction }, queryParamsHandling: 'merge'});
  }

  pageChangeHandler(page: PageEvent) {
    this.router.navigate([], { queryParams: { pageindex: page.pageIndex, pagesize: page.pageSize }, queryParamsHandling: 'merge'});
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

}

