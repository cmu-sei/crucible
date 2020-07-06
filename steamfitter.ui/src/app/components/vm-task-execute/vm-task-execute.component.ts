/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, EventEmitter, Output, NgZone, ViewChild } from '@angular/core';
import { ErrorStateMatcher, MatStepper } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm } from '@angular/forms';
import { PlayerDataService } from 'src/app/services/data/player-data-service';
import { TaskDataService } from 'src/app/data/task/task-data.service';
import { Task, Vm, Result, TaskService } from 'src/app/swagger-codegen/dispatcher.api';
import { NewTaskService } from '../../services/new-task/new-task.service';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { ResultQuery } from 'src/app/data/result/result.query';

/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

@Component({
  selector: 'app-vm-task-execute',
  templateUrl: './vm-task-execute.component.html',
  styleUrls: ['./vm-task-execute.component.css']
})
export class VmTaskExecuteComponent {

  @Output() editComplete = new EventEmitter<boolean>();
  @ViewChild(VmTaskExecuteComponent) child;
  @ViewChild('stepper') stepper: MatStepper;

  matcher = new UserErrorStateMatcher();
  isLinear = false;
  viewList = this.playerDataService.viewList;
  selectedView = this.playerDataService.selectedView;
  vmList = this.playerDataService.vmList;
  pageEvent = this.playerDataService.vmPageEvent;
  results = this.resultQuery.selectAll();
  selectedVms: Array<Vm>;
  task: Task;
  isExecuting: boolean;
  lastExecutionTime = new BehaviorSubject<Date>(new Date);

  constructor(
    public zone: NgZone,
    private playerDataService: PlayerDataService,
    private newTaskService: NewTaskService,
    private taskService: TaskService,
    private resultQuery: ResultQuery,
    private taskDataService: TaskDataService
  ) {
    this.playerDataService.getViewsFromApi();
    this.isExecuting = false;

    this.newTaskService.task.subscribe(task => {
      this.task = task;
      this.setTaskVms();
    });

    this.newTaskService.vmList.subscribe(vms => {
      this.selectedVms = vms;
      this.setTaskVms();
    });

    taskDataService.resetResultStore();
  }

  setTaskVms() {
    if (this.task && this.selectedVms) {
      this.task.vmList.length = 0;
      this.selectedVms.forEach(vm => {
        this.task.vmList.push(vm.id);
      });
    }
  }

  executeTask() {
    this.lastExecutionTime.next(new Date());
    this.isExecuting = true;
    this.taskService.createAndExecuteTask(this.task).pipe(take(1)).subscribe(results => {
      this.taskDataService.updateResultStoreMany(results);
      this.isExecuting = false;
    },
    error => {
      this.isExecuting = false;
      console.log('The Steamfitter API generated an error.  ' + error.message);
    });
  }

  openVmConsole(id: string) {
    const url = this.selectedVms.find(v => v.id === id).url;
    window.open(url, '_blank');
  }

  /**
   * Returns the stepper to zero index
   */
  resetStepper() {
    if (this.stepper) {
      console.log('here  ' + this.stepper);
      this.stepper.selectedIndex = 0;
    }
  }

  /**
   * Closes the edit screen
   */
  returnToMain(): void {
    // this.currentTeam = undefined;
    // this.editComplete.emit(true);
  }

  /**
   * Called when the mat-step index has changed to signal an update to the task
   * @param event SelectionChange event
   */
  onTaskStepChange(event: any) {
    // index 2 is the Teams step.  Refresh when selected to ensure latest information updated
    if (event.selectedIndex === 2) {
      // this.updateView();
    } else {
      // Clicked away from teams
      // this.currentTeam = undefined;
      // this.updateApplicationTemplates();
    }
  }

} // End Class
