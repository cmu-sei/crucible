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
import { BehaviorSubject } from 'rxjs';
import { Command } from '../../models/command';
import { Task, Vm } from 'src/app/swagger-codegen/dispatcher.api/model/models';

const BLANK_TASK = {
  name: '',
  description: '',
  scenarioTemplateId: '',
  scenarioId: '',
  action: Task.ActionEnum.GuestProcessRun,
  vmMask: '',
  vmList: [],
  apiUrl: '',
  actionParameters: {},
  expectedOutput: '',
  expirationSeconds: 0,
  intervalSeconds: 0,
  iterations: 1,
  iterationTermination: Task.IterationTerminationEnum.IterationCount,
  triggerTaskId: '',
  triggerCondition: Task.TriggerConditionEnum.Manual,
};

@Injectable({
  providedIn: 'root'
})
export class NewTaskService {

  public vmList = new BehaviorSubject<Array<Vm>>(new Array<Vm>());
  public command = new BehaviorSubject<Command>(undefined);
  public task = new BehaviorSubject<Task>(undefined);
  private _task = {... BLANK_TASK};

  constructor() { }

  reset() {
    this.vmList.next(new Array<Vm>());
    this.command.next(undefined);
    this.task.next(undefined);
    this._task = {... BLANK_TASK};
  }

  addVm(vm: Vm) {
    const newList: Array<Vm> = this.vmList.value;
    newList.push(vm);
    this.vmList.next(newList);
    this.buildRawCommand();
  }

  removeVm(vm: Vm) {
    const newList: Array<Vm> = this.vmList.value.filter(virtM => virtM.id !== vm.id);
    this.vmList.next(newList);
    this.buildRawCommand();
  }

  updateCommand(cmd: Command) {
    this.command.next(cmd);
    this.buildRawCommand();
  }

  private buildRawCommand() {
    if (this.command.value !== undefined) {
      // Get first vm moid, NOT a list!
      const moid = this.vmList.value.length > 0 ? this.vmList.value[0].id : '';
      this._task.actionParameters = this.command.value.parameters;
      this._task.apiUrl = this.command.value.api;
      this._task.vmList = [moid];
      this._task.action =
        Task.ActionEnum[Object.keys(Task.ActionEnum).find(key =>
        Task.ActionEnum[key] ===  this.command.value.action)];
      // let evryone know the new Task
      this.task.next(JSON.parse(JSON.stringify(this._task)));
    }
  }


  stringToEnum<ET, T>(enumObj: ET, str: keyof ET): T {
    return enumObj[<string>str];
  }
}


