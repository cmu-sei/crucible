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
import { ComnAuthQuery } from '@crucible/common';
import { User as AuthUser } from 'oidc-client';
import { BehaviorSubject, Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import {
  RoleService,
  User as PlayerUser,
  UserService,
} from '../../generated/s3.player.api';
// Used to display Super User text
export const SUPER_USER = 'Super User';

@Injectable({ providedIn: 'root' })
export class LoggedInUserService implements OnDestroy {
  public loggedInUser$: BehaviorSubject<AuthUser> = new BehaviorSubject(null);
  public isSuperUser$: BehaviorSubject<Boolean> = new BehaviorSubject(false);
  unsubscribe$: Subject<null> = new Subject<null>();

  constructor(
    private userService: UserService,
    private authQuery: ComnAuthQuery,
    private roleService: RoleService
  ) {
    this.authQuery.user$
      .pipe(
        filter((user: AuthUser) => user != null),
        takeUntil(this.unsubscribe$)
      )
      .subscribe((user) => {
        this.setLoggedInUser(user);
      });
  }

  /**
   * Once a user is logged in, this obtains Player Api specific User data.
   * @param guid
   */
  public setLoggedInUser(authUser: AuthUser) {
    this.userService
      .getUser(authUser.profile.sub)
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((playerUser: PlayerUser) => {
        // combine player properties into the AuthUser profile.
        authUser.profile = { ...authUser.profile, ...playerUser };
        this.isSuperUser$.next(authUser.profile.isSystemAdmin);
        this.loggedInUser$.next(authUser);
      });
  }

  ngOnDestroy() {
    this.unsubscribe$.next(null);
    this.unsubscribe$.complete();
  }
}
