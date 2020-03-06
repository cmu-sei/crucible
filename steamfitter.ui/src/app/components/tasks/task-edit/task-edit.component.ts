/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Output } from '@angular/core';
import { DispatchTask, DispatchTaskService } from 'src/app/swagger-codegen/dispatcher.api';
import { Command } from 'src/app/models/command';
import { MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-task-edit',
  templateUrl: './task-edit.component.html',
  styleUrls: ['./task-edit.component.css']
})
export class TaskEditComponent implements OnInit {

  @Output() changesWereMade = false;
  @Output() newDispatchTaskId = '';
  public blankTask = <DispatchTask>{
    name: '',
    description: '',
    scenarioId: '',
    sessionId: '',
    action: DispatchTask.ActionEnum.GuestProcessRun,
    vmMask: '',
    vmList: [],
    apiUrl: '',
    inputString: '',
    expectedOutput: '',
    expirationSeconds: 0,
    intervalSeconds: 0,
    iterations: 0,
    triggerTaskId: '',
    triggerCondition: DispatchTask.TriggerConditionEnum.Manual,
  };
  public triggerConditions = [
    DispatchTask.TriggerConditionEnum.Time,
    DispatchTask.TriggerConditionEnum.Manual,
    DispatchTask.TriggerConditionEnum.Completion,
    DispatchTask.TriggerConditionEnum.Success,
    DispatchTask.TriggerConditionEnum.Failure,
    DispatchTask.TriggerConditionEnum.Expiration
  ];
  public availableCommands: Command[];
  public selectedCommand: Command;

  private _dispatchTask = this.blankTask;
  public get dispatchTask(): DispatchTask {
    return this._dispatchTask;
  }
  public set dispatchTask(value: DispatchTask) {
    this._dispatchTask = value;
    this.selectTheDispatchTaskCommand();
    this.newDispatchTaskId = '';
  }

  constructor(
    public dispatchTaskService: DispatchTaskService,
    public dialogRef: MatDialogRef<TaskEditComponent>
  ) {}

  ngOnInit() {
    this.dispatchTaskService.getAvailableCommands().subscribe(cmdsJson => {
      if (cmdsJson != null) {
        // console.log(cmdsJson.toString());
        const cmds: AvailableCommands = JSON.parse(cmdsJson.toString());
        this.availableCommands = cmds.availableCommands;
        this.selectTheDispatchTaskCommand();
      }
    },
    error => {
      console.log('The Dispatcher API is not responding.  ' + error.message);
    });
  }

  onDataChange() {
    if (!this.dispatchTask.id) {
      this.dispatchTaskService.createDispatchTask(this.dispatchTask).subscribe(newTask => {
        this.dispatchTask = newTask;
        this.newDispatchTaskId = newTask.id;
      });
    } else {
      this.dispatchTaskService.updateDispatchTask(this.dispatchTask.id, this.dispatchTask).subscribe(updatedTask => {
        this.dispatchTask = updatedTask;
      });
    }
    this.changesWereMade = true;
  }

  onCommandChange() {
    // Build dispatcher command
    let dispatcherCmd = `{ "Moid": "${ '{moid}' }"`;
    this.selectedCommand.parameters.filter(obj => obj.key !== 'Moid').forEach(param => {
      dispatcherCmd += `, "${ param.key }": "${ param.value.replace(/\\/g, '\\\\')}"`;
    });
    dispatcherCmd += '}';
    this.dispatchTask.inputString = dispatcherCmd;
    this.dispatchTask.apiUrl = this.selectedCommand.api;
    this.dispatchTask.action = this.selectedCommand.action;
    this.onDataChange();
  }

  private selectTheDispatchTaskCommand() {
    if (!!this.availableCommands && this.availableCommands.length > 0) {
      this.availableCommands.forEach(cmd => {
        if (cmd.api === this.dispatchTask.apiUrl && cmd.action === this.dispatchTask.action) {
          const selectedParameters = JSON.parse(this.dispatchTask.inputString);
          cmd.parameters.forEach(p => {
            p.value = selectedParameters[p.key];
          });
          this.selectedCommand = cmd;
        } else {
          cmd.parameters.forEach(p => { p.value = ''; });
        }
        console.log(this.availableCommands);
        console.log(this.selectedCommand);
      });
    }
  }

}

interface AvailableCommands {
  availableCommands: Command[];
}


