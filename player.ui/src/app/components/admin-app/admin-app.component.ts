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
import { SettingsService } from '../../services/settings/settings.service';
import { LoggedInUserService } from '../../services/logged-in-user/logged-in-user.service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-admin-app',
  templateUrl: './admin-app.component.html',
  styleUrls: ['./admin-app.component.css']
})
export class AdminAppComponent implements OnInit {

  public viewsText = 'Views';
  public usersText = 'Users';
  public appTemplatesText = 'Application Templates';
  public rolesPermissionsText = 'Roles / Permissions';
  public topBarColor = '#b00';

  public username: string;

  public opened: boolean;
  public showStatus: string;

  constructor(
    private settingsService: SettingsService,
    private titleService: Title,
    private loggedInUserService: LoggedInUserService,
    private router: Router,
    private authService: AuthService
  ) { }

  /**
   * Initialization
   */
  ngOnInit() {
    this.opened = true;
    if (this.showStatus === undefined) {
      this.adminGotoViews();
    }

    // Set the topbar color from config file
    this.topBarColor = this.settingsService.AppTopBarHexColor;


    // Set the page title from configuration file
    this.titleService.setTitle(this.settingsService.AppTitle);
    this.username = '';

    this.loggedInUserService.loggedInUser.subscribe(loggedInUser => {
      if (loggedInUser == null) {
        return;
      }
      // Get username information
      this.username = loggedInUser.name;

      this.loggedInUserService.isSuperUser.subscribe(isSuperUser => {
        if (!isSuperUser) {
          this.router.navigate(['']);
          return;
        }
      });
    });

  }

  /**
   * Set the display to View
   */
  adminGotoViews(): void {
    this.showStatus = this.viewsText;
  }

  /**
   * Sets the display to Users
   */
  adminGotoUsers(): void {
    this.showStatus = this.usersText;
  }

  /**
   * Sets the display to App Templates
   */
  adminGotoAppTemplates(): void {
    this.showStatus = this.appTemplatesText;
  }

  /**
   * Sets the display to roles/permissions
   */
  adminGotoRolesPermissions(): void {
    this.showStatus = this.rolesPermissionsText;
  }

  /**
   * Calls the identity logout method
   */
  logout(): void {
    this.authService.logout();
  }

  /**
   * Change the screen to the main home page
   */
  GotoMainPage(): void {
    this.router.navigate(['']);
  }

}
