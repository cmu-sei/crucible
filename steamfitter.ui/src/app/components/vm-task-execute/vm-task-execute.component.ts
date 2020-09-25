/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {
  Component,
  EventEmitter,
  NgZone,
  OnDestroy,
  Output,
  ViewChild,
} from '@angular/core';
import { FormControl, FormGroupDirective, NgForm } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatStepper } from '@angular/material/stepper';
import { ComnAuthService } from '@crucible/common';
import { BehaviorSubject, Subject } from 'rxjs';
import { take, takeUntil } from 'rxjs/operators';
import { PlayerDataService } from 'src/app/data/player/player-data-service';
import { ResultDataService } from 'src/app/data/result/result-data.service';
import { ResultQuery } from 'src/app/data/result/result.query';
import { ScenarioDataService } from 'src/app/data/scenario/scenario-data.service';
import { TaskDataService } from 'src/app/data/task/task-data.service';
import { TaskQuery } from 'src/app/data/task/task.query';
import {
  Scenario,
  Task,
  TaskService,
  Vm,
} from 'src/app/swagger-codegen/dispatcher.api';
import { ComnSettingsService } from '@crucible/common';

/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(
    control: FormControl | null,
    form: FormGroupDirective | NgForm | null
  ): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

@Component({
  selector: 'app-vm-task-execute',
  templateUrl: './vm-task-execute.component.html',
  styleUrls: ['./vm-task-execute.component.scss'],
})
export class VmTaskExecuteComponent implements OnDestroy {
  @Output() editComplete = new EventEmitter<boolean>();
  @ViewChild(VmTaskExecuteComponent) child;
  @ViewChild('stepper') stepper: MatStepper;

  matcher = new UserErrorStateMatcher();
  isLinear = false;
  viewList = this.playerDataService.viewList;
  selectedView = this.playerDataService.selectedView;
  results = this.resultQuery.selectAll();
  taskList = this.taskQuery.selectAll();
  selectedVms: Array<Vm>;
  task: Task;
  isExecuting: boolean;
  lastExecutionTime = new BehaviorSubject<Date>(new Date());
  loggedInUser = this.authService.user$;
  userScenario: Scenario;
  private unsubscribe$ = new Subject();
  topbarColor = '#ef3a47';

  constructor(
    public zone: NgZone,
    private playerDataService: PlayerDataService,
    private taskService: TaskService,
    private resultQuery: ResultQuery,
    private taskDataService: TaskDataService,
    private resultDataService: ResultDataService,
    private taskQuery: TaskQuery,
    private authService: ComnAuthService,
    private scenarioDataService: ScenarioDataService,
    private settingsService: ComnSettingsService
  ) {
    this.scenarioDataService.selected
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((scenario) => {
        this.userScenario = scenario;
      });
    this.scenarioDataService.loadTaskBuilderScenario();
    this.isExecuting = false;
    this.topbarColor = this.settingsService.settings.AppTopBarHexColor ? this.settingsService.settings.AppTopBarHexColor : this.topbarColor;
  }

  setTaskVms() {
    if (this.task && this.selectedVms) {
      this.task.vmList.length = 0;
      this.selectedVms.forEach((vm) => {
        this.task.vmList.push(vm.id);
      });
    }
  }

  executeTask() {
    this.lastExecutionTime.next(new Date());
    this.isExecuting = true;
    this.taskService
      .createAndExecuteTask(this.task)
      .pipe(take(1))
      .subscribe(
        (results) => {
          this.resultDataService.updateStoreMany(results);
          this.isExecuting = false;
        },
        (error) => {
          this.isExecuting = false;
          console.log(
            'The Steamfitter API generated an error.  ' + error.message
          );
        }
      );
  }

  refreshTaskList() {
    if (this && this.task) {
      this.taskDataService.loadById(this.task.id);
    }
  }

  deleteTask(id: string) {
    this.taskDataService.delete(id);
  }

  onViewChange(event: any) {
    if (event && event.value && event.value.id) {
      const scenario = { ...this.userScenario };
      scenario.viewId = event.value.id;
      this.scenarioDataService.updateScenario(scenario);
      this.playerDataService.selectView(scenario.viewId);
    }
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
} // End Class
