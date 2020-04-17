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
import { DispatchTaskService } from '../../swagger-codegen/dispatcher.api';
import { Vm } from '../../swagger-codegen/dispatcher.api/model/vm';
import { NewTaskService } from 'src/app/services/new-task/new-task.service';
import { Command } from '../../models/command';


@Component({
  selector: 'app-commands',
  templateUrl: './commands.component.html',
  styleUrls: ['./commands.component.css']
})
export class CommandsComponent implements OnInit {

  constructor(
    private taskService: DispatchTaskService,
    private newTaskService: NewTaskService
  ) { }

  public availableCommands: Command[];
  public selectedCommand: Command;
  public selectedVms: Array<Vm>;

  ngOnInit() {
    this.taskService.getAvailableCommands().subscribe(cmdsJson => {
      if (cmdsJson != null) {
        // console.log(cmdsJson.toString());
        const cmds: AvailableCommands = JSON.parse(cmdsJson.toString());
        cmds.availableCommands.forEach(cmd => {
          cmd.parameters.forEach(p => { p.value = ''; });
        });
        this.availableCommands = cmds.availableCommands;
      }
    },
    error => {
      console.log('The Dispatcher API is not responding.  ' + error.message);
    });

    this.newTaskService.vmList.subscribe(vmList => {
      this.selectedVms = vmList;
    });
  }


  onCommandChange() {
    this.newTaskService.UpdateCommand(this.selectedCommand);
  }

}


interface AvailableCommands {
  availableCommands: Command[];
}

