/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { ApplicationData } from '../../../models/application-data';
import { FocusedAppService } from '../../../services/focused-app/focused-app.service';
import { ApplicationsService } from '../../../services/applications/applications.service';
import { ExercisesService } from '../../../services/exercises/exercises.service';
import { TeamsService } from '../../../services/teams/teams.service';
import { AuthService } from '../../../services/auth/auth.service';
import { DomSanitizer, SafeResourceUrl, SafeUrl, Title } from '@angular/platform-browser';
import { SettingsService } from '../../../services/settings/settings.service';
import { LoggedInUserService } from '../../../services/logged-in-user/logged-in-user.service';


@Component({
  selector: 'app-application-list',
  templateUrl: './application-list.component.html',
  styleUrls: ['./application-list.component.css']
})
export class ApplicationListComponent implements OnInit {
  @Output() toggleSideNavEvent = new EventEmitter<String>();


  public applicationList: Array<ApplicationData> = new Array<ApplicationData>();
  public exerciseGUID: string;
  public titleText: string;

  constructor(
    private applicationsService: ApplicationsService,
    private exercisesService: ExercisesService,
    private focusedAppService: FocusedAppService,
    private loggedInUserService: LoggedInUserService,
    private teamsService: TeamsService,
    private authService: AuthService,
    private sanitizer: DomSanitizer,
    private titleService: Title,
    private settingsService: SettingsService
  ) { }



  ngOnInit() {

    // Set the page title from configuration file
    this.titleText = this.settingsService.AppTopBarText;
    this.titleService.setTitle(this.titleText);

    // The current applications list
    this.applicationList = [];

    // Call to update the applications list anytime the Current Exercise GUID is changed
    this.exercisesService.currentExerciseGuid.subscribe(currentExerciseGUID => {
      if (currentExerciseGUID !== '') {
        // Tell the service to update once a user is officially logged in
        this.loggedInUserService.loggedInUser.subscribe(loggedInUser => {
          if (loggedInUser == null) {
            return;
          }

          this.teamsService.getUserTeamsByExercise(loggedInUser.id, currentExerciseGUID).subscribe(team => {
            this.applicationsService.getApplicationsByTeam(team.filter(t => t.isPrimary)[0].id, currentExerciseGUID).subscribe(apps => {
              this.applicationList = apps;

              this.applicationList.forEach(app => {
                  app.safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(app.url);
              });

              // Service received new application list
              if (this.applicationList.length > 0) {
                const focusedApp: ApplicationData = this.applicationList[0];

                if (focusedApp != null) {
                  this.openInFocusedApp(focusedApp.name, focusedApp.url);
                }
              }
            });
          });
        });
      }
    });

  }

  sideNavToggle() {
    this.toggleSideNavEvent.emit('Toggle Side Nav');
  }

  // Local Component functions
  openInTab(url: string) {
    window.open(url, '_blank');
  }

  openInFocusedApp(name: string, url: string) {

    this.authService.isAuthenticated().then(isAuthenticated => {
      if (!isAuthenticated) {
        console.log('User is not authenticated and must not have been redirected to login.');
        window.location.reload();
      } else {
        this.focusedAppService.focusedAppUrl.next(url);
      }
    });
  }

}

