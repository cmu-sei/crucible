/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, EventEmitter, Output, NgZone, ViewChild, Input } from '@angular/core';
import { ScenarioTemplate } from 'src/app/swagger-codegen/dispatcher.api';
import { ScenarioTemplateQuery } from 'src/app/data/scenario-template/scenario-template.query';
import { TasksComponent } from '../../tasks/tasks.component';
import { TaskDataService } from 'src/app/data/task/task-data.service';
import { TaskQuery } from 'src/app/data/task/task.query';

@Component({
  selector: 'app-scenario-template-edit',
  templateUrl: './scenario-template-edit.component.html',
  styleUrls: ['./scenario-template-edit.component.css']
})

export class ScenarioTemplateEditComponent {

  @Input() scenarioTemplate: ScenarioTemplate;
  @Output() editComplete = new EventEmitter<boolean>();
  @Output() editScenarioTemplate = new EventEmitter<ScenarioTemplate>();
  @ViewChild(ScenarioTemplateEditComponent) child;
  @ViewChild(TasksComponent) tasks: TasksComponent;

  taskList = this.taskQuery.selectAll();
  isLoading = this.scenarioTemplateQuery.selectLoading();

  constructor(
    private scenarioTemplateQuery: ScenarioTemplateQuery,
    private taskDataService: TaskDataService,
    private taskQuery: TaskQuery,
    public zone: NgZone
  ) {
  }

  refreshTaskList() {
    if (this && this.scenarioTemplate) {
      this.taskDataService.loadByScenarioTemplate(this.scenarioTemplate.id);
    }
  }

  deleteTask(id: string) {
    this.taskDataService.delete(id);
  }

  returnToScenarioTemplateList() {
    this.editComplete.emit(true);
  }

} // End Class

