/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Input } from '@angular/core';
import { Team, ApplicationService, ApplicationInstance, Application, ApplicationInstanceForm, Exercise, ApplicationTemplate } from '../../../swagger-codegen/s3.player.api';
import { DialogService } from '../../../services/dialog/dialog.service';

export enum ObjectType { Unknown, Team, Exercise }

@Component({
  selector: 'app-team-applications-select',
  templateUrl: './team-applications-select.component.html',
  styleUrls: ['./team-applications-select.component.css']
})
export class TeamApplicationsSelectComponent implements OnInit {

  @Input() team: Team;
  @Input() exercise: Exercise;

  public exerciseApplications: Array<Application>;
  public applications: Array<ApplicationInstance>;
  public applicationTemplates = new Array<ApplicationTemplate>();
  public objTypes = ObjectType;

  public subjectType = ObjectType.Unknown;
  public subject: any;
  public currentApp: Application;

  constructor(
    public applicationService: ApplicationService,
    public dialogService: DialogService
  ) { }

  /**
   * Initialization
   */
  ngOnInit() {
    if (!this.team) {
      // a team must be provided or will not be functional
      console.log('The applications select component requires a team, therefore will be non-functional.');
      return;
    } else {
      this.subjectType = ObjectType.Team;
      this.subject = this.team;
      this.refreshTeamApplications();
    }
  }

  /**
   * Refreshes the Exercise Apps list
   */
  refreshExerciseAppsAvailable(): void {
    this.applicationService.getExerciseApplications(this.exercise.id).subscribe(apps => {
      this.exerciseApplications = new Array<Application>();
      apps.forEach(app => {
        if (this.applications.findIndex(a => app.id === a.applicationId) === -1) {
          this.exerciseApplications.push(app);
        }
      });
    });

    this.applicationService.getApplicationTemplates().subscribe(appTmps => {
      this.applicationTemplates = appTmps;
    });
  }


  /**
   * Refreshes the team apps
   */
  refreshTeamApplications(): void {
    this.applicationService.getTeamApplicationInstances(this.team.id).subscribe(appInsts => {
      this.applications = appInsts;
      this.refreshExerciseAppsAvailable();
    });
  }


  /**
   * Adds an application to the team
   * @param app The app to add
   */
  addExerciseAppToTeam(app: Application): void {
    const appInstance = <ApplicationInstanceForm>({ teamId: this.team.id, applicationId: app.id, displayOrder: this.applications.length });
    this.applicationService.createApplicationInstance(this.team.id, appInstance).subscribe(rslt => {
      this.refreshTeamApplications();
    });
  }


  /**
   * Swaps the display orders of two teams in the application
   * @param app1
   * @param app2
   */
  swapDisplayOrders(app1: ApplicationInstance, app2: ApplicationInstance): void {
    const a1 = <ApplicationInstanceForm>({ id: app1.id, teamId: this.team.id, applicationId: app1.applicationId, displayOrder: app2.displayOrder });
    const a2 = <ApplicationInstanceForm>({ id: app2.id, teamId: this.team.id, applicationId: app2.applicationId, displayOrder: app1.displayOrder });

    this.applicationService.updateApplicationInstance(app1.id, a1).subscribe(result1 => {
      this.applicationService.updateApplicationInstance(app2.id, a2).subscribe(result2 => {
        this.refreshTeamApplications();
      });
    });
  }


  /**
   * Removes an application from a team
   * @param app App to remove
   */
  removeApplicationInstanceFromTeam(app: ApplicationInstance): void {
    this.dialogService.confirm('Remove Application from Team', 'Are you sure that you want to remove application ' + app.name + ' from team ' + this.team.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.applicationService.deleteApplicationInstance(app.id).subscribe(rslt => {
            // Verify display orders and fix if necessary
            let index = 0;
            const apps = this.applications.filter(a => a.id !== app.id);
            apps.forEach(a => {
              if (a.displayOrder !== index) {
                const appOrdered = <ApplicationInstanceForm>({ id: a.id, teamId: this.team.id, applicationId: a.applicationId, displayOrder: index });
                this.applicationService.updateApplicationInstance(appOrdered.id, appOrdered).subscribe(() => {
                  a.displayOrder = index; // Update here rather than calling again.
                });
              }
              index++;
            });

            this.refreshTeamApplications();
          });
        }
      });
  }

  getAppName(app: Application) {
    if (app.name != null) {
      return app.name;
    } else if (app.applicationTemplateId != null) {
      const template = this.applicationTemplates.find(x => x.id === app.applicationTemplateId);

      if (template != null) {
        return template.name;
      } else {
        return null;
      }
    } else {
      return null;
    }
  }
}
