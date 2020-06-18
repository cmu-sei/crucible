/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Task, Result } from 'src/app/swagger-codegen/dispatcher.api/model/models';
import { TaskDataService } from 'src/app/data/task/task-data.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-tasks',
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.css']
})
export class TasksComponent {

  @Input() taskList: Observable<Task[]>;
  @Input() resultList: Observable<Result[]>;
  @Input() isLoading: Observable<boolean>;
  @Input() scenarioTemplateId: string;
  @Input() scenarioId: string;
  @Input() isEditable: boolean;
  @Input() isExecutable: boolean;
  clipboard = this.taskDataService.clipboard;

  constructor(
    private taskDataService: TaskDataService
  ) { }

  deleteTask(id: string) {
    this.taskDataService.delete(id);
  }

  executeTask(id: string) {
    this.taskDataService.execute(id);
  }

  saveTaskHandler(task: Task) {
    if (!task.id) {
      this.taskDataService.add(task);
    } else {
      this.taskDataService.updateTask(task);
    }
  }

  sendToClipboard(data: any) {
    this.taskDataService.setClipboard(data);
  }

  pasteClipboard(taskId: string) {
    if (taskId) {
      this.taskDataService.pasteClipboard( {id: taskId, locationType: 'task'} );
    } else if (this.scenarioTemplateId) {
      this.taskDataService.pasteClipboard( {id: this.scenarioTemplateId, locationType: 'scenarioTemplate'} );
    } else if (this.scenarioId) {
      this.taskDataService.pasteClipboard( {id: this.scenarioId, locationType: 'scenario'} );
    } else {
      this.taskDataService.pasteClipboard(null);
    }
  }

}

