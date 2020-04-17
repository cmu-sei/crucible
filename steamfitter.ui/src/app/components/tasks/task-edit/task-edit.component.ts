/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Output, Inject, EventEmitter } from '@angular/core';
// TODO: resolve imports when APIs change nouns
import { DispatchTask, DispatchTaskService } from 'src/app/swagger-codegen/dispatcher.api';
import { Command } from 'src/app/models/command';
import { MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-task-edit',
  templateUrl: './task-edit.component.html',
  styleUrls: ['./task-edit.component.css']
})
export class TaskEditComponent implements OnInit {

  @Output() editComplete = new EventEmitter<any>();
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

  constructor(
    public taskService: DispatchTaskService,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  ngOnInit() {
    this.taskService.getAvailableCommands().subscribe(cmdsJson => {
      if (cmdsJson != null) {
        const cmds: AvailableCommands = JSON.parse(cmdsJson.toString());
        this.availableCommands = cmds.availableCommands;
        this.selectTheTaskCommand();
      }
    },
    error => {
      console.log('The Dispatcher API is not responding.  ' + error.message);
    });
  }

  onCommandChange() {
    // Build dispatcher command
    let dispatcherCmd = `{ "Moid": "${ '{moid}' }"`;
    this.selectedCommand.parameters.filter(obj => obj.key !== 'Moid').forEach(param => {
      dispatcherCmd += `, "${ param.key }": "${ param.value.replace(/\\/g, '\\\\').replace(/"/g, '\\\"')}"`;
    });
    dispatcherCmd += '}';
    this.data.task.inputString = dispatcherCmd;
    this.data.task.apiUrl = this.selectedCommand.api;
    this.data.task.action = this.selectedCommand.action;
  }

  private selectTheTaskCommand() {
    if (!!this.availableCommands && this.availableCommands.length > 0) {
      this.availableCommands.forEach(cmd => {
        if (cmd.api === this.data.task.apiUrl && cmd.action === this.data.task.action) {
          const selectedParameters = JSON.parse(this.data.task.inputString);
          cmd.parameters.forEach(p => {
            p.value = selectedParameters[p.key];
          });
          this.selectedCommand = cmd;
        } else {
          cmd.parameters.forEach(p => { p.value = ''; });
        }
      });
    }
  }

  errorFree() {
    const isOkay = this.data
      && this.data.task
      && this.data.task.name
      && this.data.task.name.length > 0;
    return isOkay;
  }

  handleEditComplete(saveChanges: boolean) {
    this.editComplete.emit({ saveChanges: saveChanges, task: this.data.task });
  }

}

interface AvailableCommands {
  availableCommands: Command[];
}


