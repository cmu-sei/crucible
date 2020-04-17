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
import { MatTabChangeEvent } from '@angular/material';
import { ScenarioTemplateListComponent } from '../scenario-templates/scenario-template-list/scenario-template-list.component';
import { ScenarioListComponent } from '../scenarios/scenario-list/scenario-list.component';
import { AdminUsersService } from '../admin/admin-users.service';
import { PlayerDataService } from 'src/app/services/data/player-data-service';
import { ScenarioTemplateDataService } from 'src/app/data/scenario-template/scenario-template-data.service';
import { ScenarioDataService } from 'src/app/data/scenario/scenario-data.service';
import { TaskDataService } from 'src/app/data/task/task-data.service';
import {Router, ActivatedRoute} from '@angular/router';
import {Subject} from 'rxjs';
import {map, takeUntil} from 'rxjs/operators';

enum Section {
  taskBuilder = 'Task Builder',
  taskHistory = 'Task History',
  scenarioTemplates = 'Scenario Templates',
  scenarios = 'Scenarios'
}

@Component({
  selector: 'app-home-app',
  templateUrl: './home-app.component.html',
  styleUrls: ['./home-app.component.css']
})
export class HomeAppComponent implements OnDestroy {

  titleText = 'Steamfitter';
  section = Section;
  selectedSection: Section;
  loggedInUser = this.adminUsersService.loggedInUser;
  isSuperUser = false;
  isAuthorizedUser = false;
  isSidebarOpen = true;
  viewList = this.playerDataService.viewList;
  private unsubscribe$ = new Subject();

  constructor(
    private adminUsersService: AdminUsersService,
    private router: Router,
    activatedRoute: ActivatedRoute,
    private playerDataService: PlayerDataService,
    private scenarioTemplateDataService: ScenarioTemplateDataService,
    private scenarioDataService: ScenarioDataService,
    private taskDataService: TaskDataService
  ) {
    this.playerDataService.getViewsFromApi();
    activatedRoute.queryParamMap.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
       this.selectedSection = (params.get('tab') || Section.taskBuilder) as Section;
    });
    this.adminUsersService.isSuperUser.pipe(takeUntil(this.unsubscribe$)).subscribe(isSuper => {
      this.isSuperUser = isSuper;
    });
    this.adminUsersService.isAuthorizedUser.pipe(takeUntil(this.unsubscribe$)).subscribe(isAuthorized => {
      this.isAuthorizedUser = isAuthorized;
    });
  }

  selectTab(section: Section) {
    this.router.navigate([], { queryParams: { tab: section }, queryParamsHandling: 'merge'});
  }

  logout() {
    this.adminUsersService.logout();
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}

