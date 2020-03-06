/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit } from '@angular/core';
import { NewDispatchTaskService } from '../../services/new-dispatch-task/new-dispatch-task.service';
import { DispatchTaskService } from '../../swagger-codegen/dispatcher.api/api/dispatchTask.service';
import { DispatchTask } from '../../swagger-codegen/dispatcher.api/model/dispatchTask';
import { DispatchTaskResult } from 'src/app/swagger-codegen/dispatcher.api';
import { Vm } from '../../swagger-codegen/dispatcher.api/model/vm';

@Component({
  selector: 'app-execute-results',
  templateUrl: './execute-results.component.html',
  styleUrls: ['./execute-results.component.css']
})
export class ExecuteResultsComponent implements OnInit {


  public selectedVms: Array<Vm>;
  public dispatchTask: DispatchTask;
  public taskResults: DispatchTaskResult[];
  public isExecuting: boolean;

  constructor(
    private newDispatchTaskService: NewDispatchTaskService,
    private dispatchTaskService: DispatchTaskService
  ) {

    this.isExecuting = false;

    this.newDispatchTaskService.dispatchTask.subscribe(task => {
      this.taskResults = undefined;
      this.dispatchTask = task;
      this.setTaskVms();
    });

    this.newDispatchTaskService.vmList.subscribe(vms => {
      this.taskResults = undefined;
      this.selectedVms = vms;
      this.setTaskVms();
    });
  }

  ngOnInit() {
  }

  setTaskVms() {
    if (this.dispatchTask && this.selectedVms) {
      this.dispatchTask.vmList.length = 0;
      this.selectedVms.forEach(vm => {
        this.dispatchTask.vmList.push(vm.id);
      });
    }
  }

  executeTask() {
    this.isExecuting = true;
    this.taskResults = undefined;
    this.dispatchTaskService.createAndExecuteDispatchTask(this.dispatchTask).subscribe(results => {
      this.taskResults = results;
      this.isExecuting = false;
    },
    error => {
      this.isExecuting = false;
      console.log('The Dispatcher API generated an error.  ' + error.message);
    });
  }
}

