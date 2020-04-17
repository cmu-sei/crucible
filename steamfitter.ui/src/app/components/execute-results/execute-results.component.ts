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
import { NewTaskService } from '../../services/new-task/new-task.service';
// TODO: resolve imports when API changes nouns
import { Task } from 'src/app/data/task/task.store';
import { TaskResult } from 'src/app/data/task-result/task-result.store';
import { DispatchTaskService } from '../../swagger-codegen/dispatcher.api';
import { Vm } from '../../swagger-codegen/dispatcher.api/model/vm';

@Component({
  selector: 'app-execute-results',
  templateUrl: './execute-results.component.html',
  styleUrls: ['./execute-results.component.css']
})
export class ExecuteResultsComponent implements OnInit {


  public selectedVms: Array<Vm>;
  public task: Task;
  public taskResults: TaskResult[];
  public isExecuting: boolean;

  constructor(
    private newTaskService: NewTaskService,
    private taskService: DispatchTaskService
  ) {

    this.isExecuting = false;

    this.newTaskService.task.subscribe(task => {
      this.taskResults = undefined;
      this.task = task;
      this.setTaskVms();
    });

    this.newTaskService.vmList.subscribe(vms => {
      this.taskResults = undefined;
      this.selectedVms = vms;
      this.setTaskVms();
    });
  }

  ngOnInit() {
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
    this.isExecuting = true;
    this.taskResults = undefined;
    this.taskService.createAndExecuteDispatchTask(this.task).subscribe(results => {
      this.taskResults = results;
      this.isExecuting = false;
    },
    error => {
      this.isExecuting = false;
      console.log('The Dispatcher API generated an error.  ' + error.message);
    });
  }
}

