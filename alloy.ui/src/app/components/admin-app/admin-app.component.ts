/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/
import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { ComnAuthService, ComnSettingsService, Theme, ComnAuthQuery } from '@crucible/common';
import { Subject } from 'rxjs/Subject';
import { LoggedInUserService } from '../../services/logged-in-user/logged-in-user.service';
import { TopbarView } from './../shared/top-bar/topbar.models';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-admin-app',
  templateUrl: './admin-app.component.html',
  styleUrls: ['./admin-app.component.scss'],
})
export class AdminAppComponent implements OnInit {
  username: string;
  titleText: string;
  isSuperUser: Boolean;
  topBarColor = '#719F94';
  topBarTextColor = '#FFFFFF';
  eventTemplateId = '';
  isSidebarOpen = true;
  eventTemplatesText = 'Event Templates';
  eventsText = 'Events';
  showStatus = 'Event Templates';
  shouldUpdateEventTemplates: Subject<boolean> = new Subject();
  shouldUpdateEvents: Subject<boolean> = new Subject();
  TopbarView = TopbarView;
  theme$: Observable<Theme>;


  constructor(
    private router: Router,
    private authService: ComnAuthService,
    private settingsService: ComnSettingsService,
    private titleService: Title,
    private usersService: LoggedInUserService,
    private authQuery: ComnAuthQuery
  ) {
    this.theme$ = this.authQuery.userTheme$;
  }

  ngOnInit() {
    // Set the topbar color from config file
    this.topBarColor = this.settingsService.settings.AppTopBarHexColor;
    this.topBarTextColor = this.settingsService.settings.AppTopBarHexTextColor;
    // Set the page title from configuration file
    this.titleText = this.settingsService.settings.AppTopBarText;
    this.titleService.setTitle(this.settingsService.settings.AppTitle);
    this.username = '';
    this.isSuperUser = false;
    this.usersService.isSuperUser.subscribe((isSuperUser) => {
      this.isSuperUser = isSuperUser;
    });
  }

  logout(): void {
    this.authService.logout();
  }

  isIframe(): boolean {
    if (window.location !== window.parent.location) {
      // The page is in an iframe
      return true;
    } else {
      // The page is not in an iframe
      return false;
    }
  }

  /**
   * Set the display to View
   */
  adminGotoEventTemplates(): void {
    this.showStatus = this.eventTemplatesText;
  }

  /**
   * Sets the display to Users
   */
  adminGotoEvents(): void {
    this.showStatus = this.eventsText;
  }
}
