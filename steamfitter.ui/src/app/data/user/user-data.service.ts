/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Injectable, OnDestroy } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ComnAuthQuery, ComnAuthService } from '@crucible/common';
import { User as AuthUser } from 'oidc-client';
import { BehaviorSubject, combineLatest, Observable, Subject } from 'rxjs';
import { filter, map, take, takeUntil } from 'rxjs/operators';
import {
  PermissionService,
  UserPermissionService,
  UserService,
} from 'src/app/swagger-codegen/dispatcher.api/api/api';
import {
  Permission,
  User,
  UserPermission,
} from 'src/app/swagger-codegen/dispatcher.api/model/models';

@Injectable({
  providedIn: 'root',
})
export class UserDataService implements OnDestroy {
  private _permissions: Permission[] = [];
  private _users: User[] = [];
  readonly users = new BehaviorSubject<User[]>(this._users);
  readonly filterControl = new FormControl();
  readonly userList: Observable<User[]>;
  readonly userListTotalLength: Observable<number>;
  readonly selectedUser: Observable<User>;
  readonly loggedInUser: BehaviorSubject<AuthUser> = new BehaviorSubject<
    AuthUser
  >(null);
  readonly isAuthorizedUser = new BehaviorSubject<boolean>(false);
  readonly isSuperUser = new BehaviorSubject<boolean>(false);
  private loggedInUserPermissions: Permission[] = [];
  private filterTerm: Observable<string>;
  private sortColumn: Observable<string>;
  private sortIsAscending: Observable<boolean>;
  private pageSize: Observable<number>;
  private pageIndex: Observable<number>;
  requestedUserId: Observable<string> | undefined;
  unsubscribe$: Subject<null> = new Subject<null>();

  constructor(
    private userService: UserService,
    private userPermissionService: UserPermissionService,
    private permissionService: PermissionService,
    private authQuery: ComnAuthQuery,
    private authService: ComnAuthService,
    private router: Router,
    activatedRoute: ActivatedRoute
  ) {
    this.authQuery.user$
      .pipe(
        filter((user: AuthUser) => user != null),
        takeUntil(this.unsubscribe$)
      )
      .subscribe((authUser) => {
        // For some reason I can't get this to work without this helper function.
        // Modled after player - logged-in-user-service
        this.setLoggedInUser(authUser);
      });

    this.filterTerm = activatedRoute.queryParamMap.pipe(
      map((params) => params.get('filter') || '')
    );
    this.sortColumn = activatedRoute.queryParamMap.pipe(
      map((params) => params.get('sorton') || 'name')
    );
    this.sortIsAscending = activatedRoute.queryParamMap.pipe(
      map((params) => (params.get('sortdir') || 'asc') === 'asc')
    );
    this.pageSize = activatedRoute.queryParamMap.pipe(
      map((params) => parseInt(params.get('pagesize') || '20', 10))
    );
    this.pageIndex = activatedRoute.queryParamMap.pipe(
      map((params) => parseInt(params.get('pageindex') || '0', 10))
    );
    this.filterControl.valueChanges.subscribe((term) => {
      router.navigate([], {
        queryParams: { filter: term },
        queryParamsHandling: 'merge',
      });
    });
    this.userList = combineLatest([
      this.users,
      this.filterTerm,
      this.sortColumn,
      this.sortIsAscending,
      this.pageSize,
      this.pageIndex,
    ]).pipe(
      map(
        ([
          users,
          filterTerm,
          sortColumn,
          sortIsAscending,
          pageSize,
          pageIndex,
        ]) =>
          users
            ? (users as User[])
                .sort((a: User, b: User) =>
                  this.sortUsers(a, b, sortColumn, sortIsAscending)
                )
                .filter(
                  (user) =>
                    user.name
                      .toLowerCase()
                      .includes(filterTerm.toLowerCase()) ||
                    user.id.toLowerCase().includes(filterTerm.toLowerCase())
                )
            : []
      )
    );
    this.requestedUserId = activatedRoute.queryParamMap.pipe(
      map((params) => params.get('userId') || '')
    );
    this.selectedUser = combineLatest([
      this.userList,
      this.requestedUserId,
    ]).pipe(
      map(([userList, requestedUserId]) => {
        const selectedUser = userList.find(
          (user) => user.id === requestedUserId
        );
        return selectedUser ? selectedUser : userList[0];
      })
    );
  }

  setLoggedInUser(authUser: AuthUser) {
    this.userService
      .getUser(authUser.profile.sub)
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((user) => {
        const permissions = user.permissions;
        // merge steamfitter user into authUser for the topbar component.
        authUser.profile = { ...authUser.profile, ...user };
        this.isAuthorizedUser.next(
          permissions.some(
            (p) => p.key === 'ContentDeveloper' || p.key === 'SystemAdmin'
          )
        );
        // Steamfitter user model doesn't have a property 'isSystemAdmin' but it's required for the topbar
        // Set it by checking permissions.
        permissions.some((p) => {
          if (p.key === 'SystemAdmin') {
            this.isSuperUser.next(true);
            authUser.profile.isSystemAdmin = true;
          }
        });
        // Emit the modified user.
        this.loggedInUser.next(authUser);
      });
  }

  private sortUsers(a: User, b: User, column: string, isAsc: boolean) {
    switch (column) {
      case 'name':
        return (
          (a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1) *
          (isAsc ? 1 : -1)
        );
      case 'id':
        return (
          (a.id.toLowerCase() < b.id.toLowerCase() ? -1 : 1) * (isAsc ? 1 : -1)
        );
      default:
        return 0;
    }
  }

  setActive(id: string) {
    this.router.navigate([], {
      queryParams: { userId: id },
      queryParamsHandling: 'merge',
    });
  }

  private updateUsers(users: User[]) {
    this._users = Object.assign([], users);
    this.users.next(this._users);
  }

  getUsersFromApi() {
    return this.userService
      .getUsers()
      .pipe(take(1))
      .subscribe(
        (users) => {
          this.updateUsers(users);
        },
        (error) => {
          this.updateUsers([]);
        }
      );
  }

  getPermissionsFromApi() {
    return this.permissionService.getPermissions().pipe(
      map(
        (permissions) => {
          this._permissions = permissions;
        },
        (error) => {
          this._permissions = [];
        }
      )
    );
  }

  addUser(user: User) {
    this.userService.createUser(user).subscribe(
      (u) => {
        this._users.unshift(u);
        this.updateUsers(this._users);
      },
      (error) => {
        this.updateUsers(this._users);
      }
    );
  }

  deleteUser(user: User) {
    this.userService.deleteUser(user.id).subscribe(
      (response) => {
        this._users = this._users.filter((u) => u.id !== user.id);
        this.updateUsers(this._users);
      },
      (error) => {
        this.updateUsers(this._users);
      }
    );
  }

  addUserPermission(userPermission: UserPermission) {
    this.userPermissionService
      .createUserPermission(userPermission)
      .subscribe((response) => {
        const user = this._users.find((u) => u.id === userPermission.userId);
        const permission = this._permissions.find(
          (p) => p.id === userPermission.permissionId
        );
        user.permissions.push({ ...permission } as Permission);
        this.updateUsers(this._users);
      });
  }

  deleteUserPermission(userPermission: UserPermission) {
    this.userPermissionService
      .deleteUserPermissionByIds(
        userPermission.userId,
        userPermission.permissionId
      )
      .subscribe((up) => {
        const user = this._users.find((u) => u.id === userPermission.userId);
        const permissionIndex = user.permissions.findIndex(
          (p) => p.id === userPermission.permissionId
        );
        user.permissions.splice(permissionIndex, 1);
        this.updateUsers(this._users);
      });
  }

  logout() {
    this.authService.logout();
  }

  ngOnDestroy() {
    this.unsubscribe$.next(null);
    this.unsubscribe$.complete();
  }
}
