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
import { MatDialog } from '@angular/material/dialog';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { ComnAuthService, ComnSettingsService } from '@crucible/common';
import { ViewService } from '../../generated/s3.player.api/api/view.service';
import { View } from '../../generated/s3.player.api/model/models';
import { TeamData } from '../../models/team-data';
import { LoggedInUserService } from '../../services/logged-in-user/logged-in-user.service';
import { SystemMessageService } from '../../services/system-message/system-message.service';
import { TeamsService } from '../../services/teams/teams.service';
import { ViewsService } from '../../services/views/views.service';
import { AdminViewEditComponent } from '../admin-app/admin-view-search/admin-view-edit/admin-view-edit.component';

@Component({
  selector: 'app-player',
  templateUrl: './player.component.html',
  styleUrls: ['./player.component.scss'],
})
export class PlayerComponent implements OnInit {
  public viewGuid: string;
  public teamGuid: string;
  public userGuid: string;
  public userToken: string;

  public opened: boolean;
  public username: string;
  public view: View;
  public team: string;
  public teams: Array<TeamData>;

  public canEdit = false;
  public viewName = '';
  public topBarColor = '#b00';

  constructor(
    private route: ActivatedRoute,
    private viewsService: ViewsService,
    private viewService: ViewService,
    private authService: ComnAuthService,
    private loggedInUserService: LoggedInUserService,
    private teamsService: TeamsService,
    private systemMessageService: SystemMessageService,
    private settingsService: ComnSettingsService,
    private titleService: Title,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.opened = true;
    this.teams = new Array<TeamData>();

    // Set the topbar color from config file
    this.topBarColor = this.settingsService.settings.AppTopBarHexColor;

    // Set the page title from configuration file
    this.titleService.setTitle(this.settingsService.settings.AppTitle);

    this.loggedInUserService.loggedInUser.subscribe((loggedInUser) => {
      if (loggedInUser == null) {
        return;
      }

      // Get login information
      this.username = loggedInUser.name;
      this.userGuid = loggedInUser.id;
      this.userToken = this.authService.getAuthorizationToken();

      // Get the view GUID from the URL that the user is entering the web page on
      this.route.params.subscribe((params) => {
        this.viewGuid = params['id'];

        // Tell the rest of the subscribed components to update their view guid
        this.viewsService.currentViewGuid.next(this.viewGuid);

        // Get the view object from the view GUID
        this.viewService.getView(this.viewGuid).subscribe((view) => {
          this.view = view;
          this.viewName = view.name;
          this.canEdit = view.canManage;

          // Get the teams for the view and filter the members.
          this.teamsService
            .getUserTeamsByView(this.userGuid, view.id)
            .subscribe((teams) => {
              this.teams = teams.filter((t) => t.isMember);
              // There should only be 1 primary member, set that value for the current login
              const myTeam = teams.filter((t) => t.isPrimary)[0];
              if (myTeam !== undefined) {
                this.team = myTeam.name;
                this.teamGuid = myTeam.id;
                console.log('Primary Team id:  ' + myTeam.id);
              } else {
                this.systemMessageService.displayMessage(
                  'Error',
                  'Primary team membership was not found.  Please contact administrator.'
                );
                console.log('Team membership was not found!!!');
              }
            });
        });
      });
    });

    this.team = '';
  }

  /**
   * Logout of the Identity server
   */
  logout(): void {
    this.authService.logout();
  }

  /**
   * Set the primary team instance by the team Guid.  This is only valid when a user belongs to multiple
   * teams.  If a new primary team is set int he database, the page must be reloaded
   * @param newTeamGuid
   */
  setPrimaryTeam(newTeamGuid) {
    if (newTeamGuid !== this.teamGuid) {
      this.viewsService
        .setPrimaryTeamId(this.userGuid, newTeamGuid)
        .subscribe((team) => {
          window.location.reload();
        });
    }
  }

  /**
   * Called to open the edit view dialog window
   */
  editView() {
    const dialogRef = this.dialog.open(AdminViewEditComponent);
    dialogRef.afterOpened().subscribe((r) => {
      dialogRef.componentInstance.resetStepper();
      dialogRef.componentInstance.updateApplicationTemplates();
      dialogRef.componentInstance.updateView();
      dialogRef.componentInstance.view = this.view;
    });

    dialogRef.componentInstance.editComplete.subscribe(() => {
      dialogRef.close();
    });
  }
}
