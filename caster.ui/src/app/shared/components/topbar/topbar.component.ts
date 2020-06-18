/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import {
  CurrentUserQuery,
  CurrentUserState,
  UserService,
} from '../../../users/state';
import { Subject } from 'rxjs';
import { CwdAuthService } from '../../../sei-cwd-common/cwd-auth/services';
import { Theme } from '../../models/theme-enum';

@Component({
  selector: 'cas-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
})
export class TopbarComponent implements OnInit, OnDestroy {
  @Input() title: string;
  @Input() sidenav;
  currentUser$ = this.currentUserQuery.select();
  unsubscribe$: Subject<null> = new Subject<null>();
  constructor(
    private currentUserQuery: CurrentUserQuery,
    private userService: UserService,
    private authService: CwdAuthService
  ) {}

  ngOnInit() {}

  themeFn(event) {
    const theme = event.checked ? Theme.DARK : Theme.LIGHT;
    this.userService.setUserTheme(theme);
  }

  logout(): void {
    this.authService.logout();
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next(null);
    this.unsubscribe$.complete();
  }
}
