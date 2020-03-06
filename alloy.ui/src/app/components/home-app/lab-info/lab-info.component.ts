/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Input, } from '@angular/core';
import { MatTableDataSource } from '@angular/material';
import { SettingsService } from '../../../services/settings/settings.service';
import { DefinitionService } from '../../../swagger-codegen/alloy.api/api/definition.service';
import { Definition } from '../../../swagger-codegen/alloy.api/model/definition';
import { ImplementationService } from '../../../swagger-codegen/alloy.api/api/implementation.service';
import { Implementation } from '../../../swagger-codegen/alloy.api/model/implementation';
import { timer } from 'rxjs';


@Component({
  selector: 'app-lab-info',
  templateUrl: './lab-info.component.html',
  styleUrls: ['./lab-info.component.css']
})
export class LabInfoComponent implements OnInit {

  @Input() definitionId: string;

  readonly ONE_HOUR = (1000 * 3600);

  public impsDataSource: MatTableDataSource<Implementation>;
  public displayedColumns: string[] = ['status', 'username', 'launchDate', 'endDate'];

  public definition: Definition;
  public implementations: Array<Implementation>;
  public currentImplementation: Implementation;
  public isLoading: boolean;
  public labStatus: string;
  public pollingIntervalMS: number;
  public remainingTime: string;
  public timeRunningLow: boolean;

  constructor(
    private settingsService: SettingsService,
    private definitionService: DefinitionService,
    private implementationService: ImplementationService
    ) {
      this.impsDataSource = new MatTableDataSource<Implementation>(new Array<Implementation>());
     }

  ngOnInit() {
    this.isLoading = true;
    this.remainingTime = '';
    this.timeRunningLow = false;
    this.pollingIntervalMS = parseInt(this.settingsService.PollingIntervalMS, 10);
    this.labStatus = '';
    this.definitionService.getDefinition(this.definitionId).subscribe(def => {
      this.definition = def;
      this.startTimer();
      this.isLoading = false;
    });
  }

  startTimer() {
    timer(0, this.pollingIntervalMS).subscribe(() => this.updateImplementations());
  }


  updateImplementations() {
    this.implementationService.getMyDefinitionImplementations(this.definition.id).subscribe(imps => {
      this.implementations = imps;
      this.labStatus = this.determineLabStatus(imps);
      this.impsDataSource.data = imps;
      this.isLoading = false;
      console.log('tick');
    });
  }

  determineLabStatus(imps: Array<Implementation>): string {
    // There are 3 states that a lab can be in
    // LabReadyToLaunch
    // LabLaunchInProgress
    // LabActive
    let status = '';
    if (imps.length === 0) {
      // No implementations found
      status = 'LabReadyToLaunch';
      this.currentImplementation = undefined;
      this.remainingTime = '';
    } else {
      const actives = imps.find(s => s.status === Implementation.StatusEnum.Active);
      if (actives !== undefined) {
        // Active Lab exit now
        status = 'LabActive';
        this.currentImplementation = actives;
        this.remainingTime = this.calculateRemainingTime(this.currentImplementation.expirationDate);
      } else {
        // No active Labs, now check if anything is in progress
        const inProgress = imps.find(s =>
          s.status === Implementation.StatusEnum.Creating ||
          s.status === Implementation.StatusEnum.Planning ||
          s.status === Implementation.StatusEnum.Applying ||
          s.status === Implementation.StatusEnum.Ending);
          if (inProgress !== undefined) {
            status = 'LabLaunchInProgress';
            this.currentImplementation = inProgress;
            this.remainingTime = '';
          } else {
            // At this point, the lab is not active and not in progress
            // therefore is must be ready to be launched
            // TODO:  Handle Paused
            status = 'LabReadyToLaunch';
            this.currentImplementation = undefined;
            this.remainingTime = '';
            if (this.isIframe()) {
              // At this point the app is shown within Player therefore the parent must moved to Alloy lab page.
              window.top.location.href = window.location.href;
            }
          }
      }
    }
    return status;
  }

  launchImplementation() {
    console.log('Launching implementation for ' + this.definition.name);
    this.labStatus = 'LabLaunchInProgress';
    this.implementationService.createImplementationFromDefinition(this.definition.id).subscribe(imp => {
      this.updateImplementations();
    });
  }

  rejoinImplementation() {
    console.log('Opening ' + this.currentImplementation.name + ' inside Player!!!');
    window.location.href = this.settingsService.PlayerUIAddress + '/exercise-player/' + this.currentImplementation.exerciseId;
  }

  endImplementation() {
    console.log('Ending ' + this.currentImplementation.name);
    this.labStatus = 'LabLaunchInProgress';
    this.implementationService.endImplementation(this.currentImplementation.id).subscribe(imp => {
      if (this.isIframe()) {
        // At this point the app is shown within Player therefore the parent must moved to Alloy lab page.
        window.top.location.href = window.location.href;
      } else {
        this.updateImplementations();
      }
    });
  }

  isIframe(): boolean {
    if ( window.location !== window.parent.location ) {
      // The page is in an iframe
      return true;
    } else {
      // The page is not in an iframe
      return false;
    }
  }

  calculateRemainingTime(expirationDate: Date): string {
    let timeLeft = '';

    if (expirationDate !== undefined) {
      const now = new Date();
      // Note:  A C# date time is different and when parsing againt the timezone must be added.
      const exp = new Date(Date.parse(expirationDate.toLocaleString()).valueOf() - (now.getTimezoneOffset() * 60 * 1000));
      let diffInMs: number = exp.valueOf() - Date.parse(now.toISOString()).valueOf();
      if (diffInMs < 0) {
        diffInMs = 0; // Force to zero.  Do not display a negative time.
      }
      const modHrs = (diffInMs / this.ONE_HOUR) % 1;
      const diffInHrs: number = diffInMs / (1000 * 3600) - modHrs;
      const modMins = Math.floor(modHrs * 60);
      timeLeft = 'Time Remaining:  ' + diffInHrs.toString() + ' hrs ' + modMins + ' mins';

      this.timeRunningLow = (diffInMs < this.ONE_HOUR);
    }
    return timeLeft;
  }

}

