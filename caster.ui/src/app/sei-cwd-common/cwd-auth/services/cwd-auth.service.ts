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
import { CwdSettingsService } from '../../cwd-settings';
import {
  User,
  UserManager,
  WebStorageStateStore,
  UserManagerEvents,
} from 'oidc-client';
import { AuthStore } from '../state/auth.store';
import { AuthQuery } from '../state/auth.query';
import UserSignedOutCallback = UserManagerEvents.UserSignedOutCallback;

@Injectable({
  providedIn: 'root',
})
export class CwdAuthService {
  private userManager: UserManager;
  private user: User;
  user$ = this.authQuery.user$;

  constructor(
    private settingsService: CwdSettingsService,
    private store: AuthStore,
    private authQuery: AuthQuery
  ) {
    if (this.settingsService.settings.UseLocalAuthStorage) {
      this.settingsService.settings.OIDCSettings.userStore = new WebStorageStateStore(
        { store: window.localStorage }
      );
    }
    this.userManager = new UserManager(
      this.settingsService.settings.OIDCSettings
    );
    this.userManager.events.addUserLoaded((user) => {
      this.onTokenLoaded(user);
    });
    this.userManager.events.addUserSignedOut(() => this.logout());

    this.userManager.getUser().then((user) => {
      if (user != null && user.profile != null) {
        this.onTokenLoaded(user);
      }
    });
  }

  public isAuthenticated(): Promise<boolean> {
    return this.userManager.getUser().then((user) => {
      return user != null && !user.expired;
    });
  }

  public startAuthentication(url: string): Promise<User> {
    this.userManager.signinRedirect({ state: url });
    return this.userManager.signinRedirectCallback(url);
  }

  public completeAuthentication(url: string): Promise<User> {
    return this.userManager.signinRedirectCallback(url);
  }

  public startSilentAuthentication(): Promise<User> {
    return this.userManager.signinSilent();
  }

  public completeSilentAuthentication(): Promise<User> {
    return this.userManager.signinSilentCallback();
  }

  public getAuthorizationHeader(): string {
    return `${this.user.token_type} ${this.user.access_token}`;
  }

  public getAuthorizationToken(): string {
    return this.user.access_token;
  }

  public logout() {
    return this.userManager.signoutRedirect();
  }

  private onTokenLoaded(user) {
    this.user = user;
    this.store.update({ user });
    const userGuid = user.profile.sub;
    // this.loggedInUserService.setLoggedInUser(userGuid);
  }
}
