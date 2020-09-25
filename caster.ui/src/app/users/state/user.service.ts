/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Injectable } from '@angular/core';
import { ComnAuthService, Theme } from '@crucible/common';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  PermissionsService,
  User,
  UserPermissionsService,
  UsersService,
} from '../../generated/caster-api';
import { UserQuery } from './user.query';
import { CurrentUserStore, UserStore } from './user.store';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  constructor(
    private userStore: UserStore,
    private currentUserStore: CurrentUserStore,
    private userQuery: UserQuery,
    private usersService: UsersService,
    private userPermissionService: UserPermissionsService,
    private permissionService: PermissionsService,
    private authService: ComnAuthService
  ) {}

  load(): Observable<User[]> {
    this.userStore.setLoading(true);
    return this.usersService.getAllUsers().pipe(
      tap((users: User[]) => {
        this.userStore.set(users);
      }),
      tap(() => {
        this.userStore.setLoading(false);
      })
    );
  }

  loadById(id: string): Observable<User> {
    this.userStore.setLoading(true);
    return this.usersService.getUser(id).pipe(
      tap((_user: User) => {
        this.userStore.upsert(_user.id, { ..._user });
      }),
      tap(() => {
        this.userStore.setLoading(false);
      })
    );
  }

  create(user: User): Observable<User> {
    return this.usersService.createUser(user).pipe(
      tap((u) => {
        this.userStore.add(u);
        this.userStore.ui.upsert(u.id, this.userQuery.ui.getEntity(u.id));
      })
    );
  }

  delete(userId: string): Observable<any> {
    return this.usersService.deleteUser(userId).pipe(
      tap(() => {
        this.userStore.remove(userId);
        this.userStore.ui.remove(userId);
      })
    );
  }

  addUserPermission(userId: string, permissionId: string) {
    this.userPermissionService
      .createUserPermission({ userId, permissionId })
      .subscribe((up) => {
        this.loadById(userId).subscribe();
      });
  }

  removeUserPermission(userId: string, permissionId: string) {
    this.userPermissionService
      .deleteUserPermissionByIds(userId, permissionId)
      .subscribe((up) => {
        this.loadById(userId).subscribe();
      });
  }

  setCurrentUser() {
    const currentUser = {
      name: '',
      isSuperUser: false,
      id: '',
    };
    this.currentUserStore.update(currentUser);
    this.authService.user$.subscribe((user) => {
      if (!!user) {
        currentUser.name = user.profile.name;
        currentUser.id = user.profile.sub;
        this.currentUserStore.update(currentUser);
        this.permissionService.getMyPermissions().subscribe((permissions) => {
          currentUser.isSuperUser = permissions.some((permission) => {
            return permission.key === 'SystemAdmin';
          });
          this.currentUserStore.update(currentUser);
        });
      }
    });
  }

  setUserTheme(theme: Theme) {
    this.currentUserStore.update({ theme });
  }

  toggleSelected(id: string) {
    this.userStore.ui.upsert(id, (entity) => ({
      isSelected: !entity.isSelected,
    }));
  }

  setActive(id) {
    this.userStore.setActive(id);
    this.userStore.ui.setActive(id);
  }
}
