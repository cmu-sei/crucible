/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/
import { Component, OnDestroy, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PlayerDataService } from 'src/app/data/player/player-data-service';
import { ComnSettingsService, Theme, ComnAuthQuery } from '@crucible/common';
import { SignalRService } from 'src/app/services/signalr/signalr.service';
import { UserDataService } from '../../data/user/user-data.service';
import { TopbarView } from './../shared/top-bar/topbar.models';

enum Section {
  taskBuilder = 'Tasks',
  history = 'History',
  scenarioTemplates = 'Scenario Templates',
  scenarios = 'Scenarios',
}

@Component({
  selector: 'app-home-app',
  templateUrl: './home-app.component.html',
  styleUrls: ['./home-app.component.scss'],
})
export class HomeAppComponent implements OnDestroy {
  @ViewChild('sidenav') sidenav: MatSidenav;
  titleText = 'Steamfitter';
  section = Section;
  selectedSection: Section;
  loggedInUser = this.userDataService.loggedInUser;
  isSuperUser = false;
  isAuthorizedUser = false;
  isSidebarOpen = true;
  viewList = this.playerDataService.viewList;
  private unsubscribe$ = new Subject();
  topbarColor = '#ef3a47';
  topbarTextColor = '#FFFFFF';
  TopbarView = TopbarView;
  theme$: Observable<Theme>;

  constructor(
    private userDataService: UserDataService,
    private router: Router,
    activatedRoute: ActivatedRoute,
    private playerDataService: PlayerDataService,
    private settingsService: ComnSettingsService,
    private signalRService: SignalRService,
    private authQuery: ComnAuthQuery
  ) {

    this.theme$ = this.authQuery.userTheme$;

    this.playerDataService.getViewsFromApi();
    activatedRoute.queryParamMap
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((params) => {
        this.selectedSection = (params.get('tab') ||
          Section.taskBuilder) as Section;
      });
    this.userDataService.isSuperUser
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((isSuper) => {
        this.isSuperUser = isSuper;
      });
    this.userDataService.isAuthorizedUser
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((isAuthorized) => {
        this.isAuthorizedUser = isAuthorized;
      });
    this.signalRService.startConnection();

    // Set the display settings from config file
    this.topbarColor = this.settingsService.settings.AppTopBarHexColor ? this.settingsService.settings.AppTopBarHexColor : this.topbarColor;
    this.topbarTextColor = this.settingsService.settings.AppTopBarHexTextColor ? this.settingsService.settings.AppTopBarHexTextColor : this.topbarTextColor;
    this.titleText = this.settingsService.settings.AppTitle ? this.settingsService.settings.AppTitle : this.titleText;
  }

  selectTab(section: Section) {
    this.router.navigate([], {
      queryParams: { tab: section },
      queryParamsHandling: 'merge',
    });
  }

  sidenavToggleFn() {
    this.sidenav.toggle();
  }

  logout() {
    this.userDataService.logout();
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}
