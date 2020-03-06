/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, ViewChild } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material';
import { ScenarioListComponent } from '../scenarios/scenario-list/scenario-list.component';
import { SessionListComponent } from '../sessions/session-list/session-list.component';
import {Subject} from 'rxjs/Subject';
import { map } from 'rxjs/operators';
import { AdminUsersService } from '../admin/admin-users.service';

enum Section {
  taskBuilder = 'Task Builder',
  taskHistory = 'Task History',
  scenarios = 'Scenarios',
  sessions = 'Sessions'
}

@Component({
  selector: 'app-home-app',
  templateUrl: './home-app.component.html',
  styleUrls: ['./home-app.component.css']
})
export class HomeAppComponent {
  @ViewChild(ScenarioListComponent) scenarioListComponent: ScenarioListComponent;
  @ViewChild(SessionListComponent) sessionListComponent: SessionListComponent;

  shouldUpdateScenarios: Subject<boolean> = new Subject();
  shouldUpdateSessions: Subject<boolean> = new Subject();
  titleText = 'Steamfitter';
  section = Section;
  selectedSection = Section.taskBuilder;
  loggedInUser = this.adminUsersService.loggedInUser;
  isSuperUser = this.adminUsersService.isSuperUser();
  isSidebarOpen = true;

  constructor(
    private adminUsersService: AdminUsersService
  ) {
  }

  tabChanged = (tabChangeEvent: MatTabChangeEvent): void => {
    switch (tabChangeEvent.index) {
      case 1:
        this.shouldUpdateScenarios.next(false);
        this.shouldUpdateSessions.next(true);
        break;
      case 2:
        this.shouldUpdateScenarios.next(true);
        this.shouldUpdateSessions.next(false);
        break;
      default:
        break;
    }
  }

  logout() {
    this.adminUsersService.logout();
  }

}

