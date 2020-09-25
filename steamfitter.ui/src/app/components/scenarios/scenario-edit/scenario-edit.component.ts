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
  Input,
  Output,
  ViewChild,
} from '@angular/core';
import { TaskTreeComponent } from 'src/app/components/tasks/task-tree/task-tree.component';
import { ResultQuery } from 'src/app/data/result/result.query';
import { ScenarioQuery } from 'src/app/data/scenario/scenario.query';
import { TaskDataService } from 'src/app/data/task/task-data.service';
import { TaskQuery } from 'src/app/data/task/task.query';
import { DialogService } from 'src/app/services/dialog/dialog.service';
import { Scenario } from 'src/app/swagger-codegen/dispatcher.api/model/models';

@Component({
  selector: 'app-scenario-edit',
  templateUrl: './scenario-edit.component.html',
  styleUrls: ['./scenario-edit.component.scss'],
})
export class ScenarioEditComponent {
  @Input() scenario: Scenario;
  @Output() editComplete = new EventEmitter<boolean>();
  @ViewChild(ScenarioEditComponent) child: ScenarioEditComponent;
  @ViewChild(TaskTreeComponent) taskTree: TaskTreeComponent;

  public changesWereMade = false;
  public scenarioStates = Object.values(Scenario.StatusEnum);
  taskList = this.taskQuery.selectAll();
  resultList = this.resultQuery.selectAll();
  isLoading = this.scenarioQuery.selectLoading();

  constructor(
    private scenarioQuery: ScenarioQuery,
    private taskDataService: TaskDataService,
    private taskQuery: TaskQuery,
    private resultQuery: ResultQuery,
    public dialogService: DialogService
  ) {}

  refreshTaskList() {
    if (this && this.scenario) {
      this.taskDataService.loadByScenario(this.scenario.id);
    }
  }

  deleteTask(id: string) {
    this.taskDataService.delete(id);
  }

  returnToScenarioList() {
    this.editComplete.emit(true);
  }
} // End Class
