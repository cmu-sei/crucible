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
import * as signalR from '@microsoft/signalr';
import { ScenarioTemplate, Scenario, Task, Result, BASE_PATH } from 'src/app/swagger-codegen/dispatcher.api';
import { AuthService } from 'src/app/services/auth/auth.service';
import { ScenarioTemplateDataService } from 'src/app/data/scenario-template/scenario-template-data.service';
import { ScenarioDataService } from 'src/app/data/scenario/scenario-data.service';
import { TaskDataService } from 'src/app/data/task/task-data.service';
import { ResultDataService } from 'src/app/data/result/result-data.service';
import { SettingsService } from 'src/app/services/settings/settings.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  private hubConnection: signalR.HubConnection;
  private connectionPromise: Promise<void>;

  constructor(
    private authService: AuthService,
    private scenarioTemplateDataService: ScenarioTemplateDataService,
    private scenarioDataService: ScenarioDataService,
    private taskDataService: TaskDataService,
    private resultDataService: ResultDataService,
    private settingsService: SettingsService
  ) { }

  public startConnection(): Promise<void> {
    if (this.connectionPromise) {
      return this.connectionPromise;
    }

    const token = this.authService.getAuthorizationToken();
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.settingsService.ApiUrl}/hubs/engine?bearer=${token}`)
      .withAutomaticReconnect(new RetryPolicy(60, 0, 5))
      .build();

      this.addHandlers();
      this.connectionPromise = this.hubConnection.start();

    return this.connectionPromise;
  }

  private addHandlers() {
    this.addScenarioTemplateHandlers();
    this.addScenarioHandlers();
    this.addTaskHandlers();
    this.addResultHandlers();
  }

  private addScenarioTemplateHandlers() {
    this.hubConnection.on('ScenarioTemplateCreated', (scenarioTemplate: ScenarioTemplate) => {
      this.scenarioTemplateDataService.updateStore(scenarioTemplate);
    });

    this.hubConnection.on('ScenarioTemplateUpdated', (scenarioTemplate: ScenarioTemplate) => {
      this.scenarioTemplateDataService.updateStore(scenarioTemplate);
    });

    this.hubConnection.on('ScenarioTemplateDeleted', (id: string) => {
      this.scenarioTemplateDataService.deleteFromStore(id);
    });
  }

  private addScenarioHandlers() {
    this.hubConnection.on('ScenarioCreated', (scenario: Scenario) => {
      this.scenarioDataService.updateStore(scenario);
    });

    this.hubConnection.on('ScenarioUpdated', (scenario: Scenario) => {
      this.scenarioDataService.updateStore(scenario);
    });

    this.hubConnection.on('ScenarioDeleted', (id: string) => {
      this.scenarioDataService.deleteFromStore(id);
    });
  }

  private addTaskHandlers() {
    this.hubConnection.on('TaskCreated', (task: Task) => {
      this.taskDataService.updateStore(task);
    });

    this.hubConnection.on('TaskUpdated', (task: Task) => {
      this.taskDataService.updateStore(task);
    });

    this.hubConnection.on('TaskDeleted', (id: string) => {
      this.taskDataService.deleteFromStore(id);
    });
  }

  private addResultHandlers() {
    this.hubConnection.on('ResultCreated', (result: Result) => {
      this.resultDataService.updateStore(result);
    });

    this.hubConnection.on('ResultUpdated', (result: Result) => {
      this.resultDataService.updateStore(result);
    });

    this.hubConnection.on('ResultsUpdated', (results: Result[]) => {
      this.resultDataService.updateStoreMany(results);
    });

    this.hubConnection.on('ResultDeleted', (id: string) => {
      this.resultDataService.deleteFromStore(id);
    });
  }

}

class RetryPolicy {

  constructor(
    private maxSeconds: number,
    private minJitterSeconds: number,
    private maxJitterSeconds: number
  ) { }

  nextRetryDelayInMilliseconds(retryContext: signalR.RetryContext): number | null {
    let nextRetrySeconds = Math.pow(2, retryContext.previousRetryCount + 1);

    if (nextRetrySeconds > this.maxSeconds) {
      nextRetrySeconds = this.maxSeconds;
    }

    nextRetrySeconds += Math.floor(
      Math.random() * (this.maxJitterSeconds - this.minJitterSeconds + 1)) + this.minJitterSeconds; // Add Jitter

    return nextRetrySeconds * 1000;
  }
}
