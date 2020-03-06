/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { FlatTreeControl } from '@angular/cdk/tree';
import { MatTreeFlatDataSource, MatTreeFlattener, MatDialog, MatTreeNode } from '@angular/material';
import { TaskEditComponent } from 'src/app/components/tasks/task-edit/task-edit.component';
import { DispatchTask, DispatchTaskService, DispatchTaskResult, DispatchTaskResultService } from 'src/app/swagger-codegen/dispatcher.api';
import { DialogService } from 'src/app/services/dialog/dialog.service';

export enum DispatchTaskSourceType {
  None,
  DispatchTask,
  Exercise,
  Scenario,
  Session,
  SessionActive,
  SessionEnded,
  User,
  VM
}

export interface DispatchTaskSource {
  type: DispatchTaskSourceType;
  id: string;
}

interface  DispatchTaskNode {
  dispatchTask: DispatchTask;
  results?: DispatchTaskResult[];
  children?: DispatchTaskNode[];
}

interface  FlatDispatchTaskNode {
  expandable: boolean;
  level: number;
  dispatchTask: DispatchTask;
  results?: DispatchTaskResult[];
}

@Component({
  selector: 'app-task-tree',
  templateUrl: './task-tree.component.html',
  styleUrls: ['./task-tree.component.css']
})
export class TaskTreeComponent implements OnInit {

  @Input() dispatchTaskSource: DispatchTaskSource;
  @Output() dispatchTaskSelected = new EventEmitter<string>();
  @ViewChild(FlatTreeControl) flatTreeControl;
  public loading = false;
  public dispatchTasks = new Array<DispatchTask>();
  public dispatchTaskResults = new Array<DispatchTaskResult>();
  public selectedTaskId = '';
  public manualTrigger = DispatchTask.TriggerConditionEnum.Manual;
  private expandedNodeSet = new Set<string>();

  // tree controls
  private transformer = (node: DispatchTaskNode, level: number) => {
    return {
      expandable: !!node.children && node.children.length > 0,
      level: level,
      dispatchTask: node.dispatchTask,
      results: node.results
    };
  }

  treeControl = new FlatTreeControl<FlatDispatchTaskNode>(node => node.level, node => node.expandable);
  treeFlattener = new MatTreeFlattener(this.transformer, node => node.level, node => node.expandable, node => node.children);
  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

  constructor(
    public dispatchTaskService: DispatchTaskService,
    public dispatchTaskResultService: DispatchTaskResultService,
    public dialogService: DialogService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.setDispatchTasks();
  }

  /**
   * sets which dispatch tasks and results to get from the api
   */
  setDispatchTasks() {
    switch (this.dispatchTaskSource.type) {
      // case DispatchTaskSource.DispatchTask:
      //   this.getDispatchTaskChildren();
      //   break;
      case DispatchTaskSourceType.Scenario:
        this.getScenarioDispatchTasks();
        break;
      case DispatchTaskSourceType.Session:
      case DispatchTaskSourceType.SessionActive:
      case DispatchTaskSourceType.SessionEnded:
        this.getSessionDispatchTasks();
        break;
      // case DispatchTaskSource.Exercise:
      //   this.getExerciseDispatchTasks();
      //   break;
      // case DispatchTaskSource.User:
      //   this.getUserDispatchTasks();
      //   break;
      // case DispatchTaskSource.VM:
      //   this.getVmDispatchTasks();
      //   break;
      default:
        this.dataSource.data.length = 0;
        this.dispatchTasks.length = 0;
        this.dispatchTaskResults.length = 0;
        break;
    }
  }

  hasChild = (_: number, node: FlatDispatchTaskNode) => node.expandable;

  /**
   * gets a SCENARIO's DispatchTasks and DispatchTaskResults
   */
  private getScenarioDispatchTasks() {
    this.loading = true;
    this.dispatchTaskResults.length = 0;
    this.dispatchTaskService.getScenarioDispatchTasks(this.dispatchTaskSource.id).subscribe(dispatchTasks => {
      this.dispatchTasks = dispatchTasks;
      const newDispatchTaskNodes = new Array<DispatchTaskNode>();
      dispatchTasks.forEach(dispatchTask => {
        if (!dispatchTask.triggerTaskId) {
          const newNode = this.createDispatchTaskNode(dispatchTask);
          newDispatchTaskNodes.push(newNode);
        }
      });
      this.rebuildTreeForData(newDispatchTaskNodes);
      this.loading = false;
    });
  }

  /**
   * gets a session's DispatchTasks and DispatchTaskResults
   */
  private getSessionDispatchTasks() {
    this.loading = true;
    this.dispatchTaskResultService.getSessionDispatchTaskResults(this.dispatchTaskSource.id).subscribe(dispatchTaskResults => {
      this.dispatchTaskResults = dispatchTaskResults;
      this.dispatchTaskService.getSessionDispatchTasks(this.dispatchTaskSource.id).subscribe(dispatchTasks => {
        this.dispatchTasks = dispatchTasks;
        const newDispatchTaskNodes = new Array<DispatchTaskNode>();
        dispatchTasks.forEach(dispatchTask => {
          if (!dispatchTask.triggerTaskId) {
            const newNode = this.createDispatchTaskNode(dispatchTask);
            newDispatchTaskNodes.push(newNode);
          }
        });
        this.rebuildTreeForData(newDispatchTaskNodes);
        this.loading = false;
      });
    });
  }

  /**
   * Creates a DispatchTaskNode to display in the tree
   */
  createDispatchTaskNode(parentTask: DispatchTask): DispatchTaskNode {
    const results = this.dispatchTaskResults.filter(result => {
      return result.dispatchTaskId === parentTask.id;
    });
    const newNode: DispatchTaskNode = {
      dispatchTask: parentTask,
      results: results,
      children: new Array<DispatchTaskNode>()
    };
    newNode.dispatchTask = parentTask;
    this.dispatchTasks.forEach(childTask => {
      if (childTask.triggerTaskId === parentTask.id) {
        const newChildNode = this.createDispatchTaskNode(childTask);
        if (!newNode.children) {
          newNode.children = new Array<DispatchTaskNode>();
        }
        newNode.children.push(newChildNode);
      }
    });
    return newNode;
  }

  /**
   * Selects a DispatchTask to edit/create
   */
  selectDispatchTask(dispatchTaskId: string) {
    this.selectedTaskId = dispatchTaskId;
    this.dispatchTaskSelected.emit(dispatchTaskId);
  }

  /**
   * Determines if a DispatchTask has already been executed
   */
  isExecuted(dispatchTaskId: string) {
    const results = this.dispatchTaskResults.filter(r => r.dispatchTaskId === dispatchTaskId);
    return results.length > 0 || this.dispatchTaskSource.type === DispatchTaskSourceType.SessionEnded;
  }

    /**
   * Determines if a DispatchTask can be executed
   */
  isExecutableTask(dispatchTask: DispatchTask) {
    // must be a manual session task
    let isExecutable = this.dispatchTaskSource.type === DispatchTaskSourceType.SessionActive &&
                       dispatchTask.triggerCondition === this.manualTrigger;
    // must not be executed already
    isExecutable = isExecutable ? !this.isExecuted(dispatchTask.id) : isExecutable;
    // if this task has a parent, the parent task must be complete
    if (isExecutable) {
      const triggerTaskId = dispatchTask.triggerTaskId;
      if (triggerTaskId) {
        const parent = this.dispatchTasks.find(dt => dt.id === triggerTaskId);
        const results = this.dispatchTaskResults.filter(r => r.dispatchTaskId === triggerTaskId);
        if (results.length === 0) {
          isExecutable = false;
        } else if (results.some(r => r.status === DispatchTaskResult.StatusEnum.Pending ||
                                     r.status === DispatchTaskResult.StatusEnum.Queued ||
                                     r.status === DispatchTaskResult.StatusEnum.Sent)) {
          isExecutable = false;
        }
      }
    }
    return isExecutable;
  }

  /**
   * Executes an action menu item
   * @param action: action string to case from
   * @param dispatchTaskGuid: The guid for exercise
   */
  executeDispatchTaskAction(action: string, dispatchTaskId: string) {
    switch (action) {
      case ('addTop'): {
        // add a top level DispatchTask
        const dialogRef = this.dialog.open(TaskEditComponent);
        dialogRef.afterOpened().subscribe(r => {
          dialogRef.componentInstance.dispatchTask = dialogRef.componentInstance.blankTask;
          switch (this.dispatchTaskSource.type) {
            case DispatchTaskSourceType.Scenario:
              dialogRef.componentInstance.dispatchTask.scenarioId = this.dispatchTaskSource.id;
              break;
            case DispatchTaskSourceType.Session:
            case DispatchTaskSourceType.SessionActive:
              dialogRef.componentInstance.dispatchTask.sessionId = this.dispatchTaskSource.id;
              break;
            default:
              break;
          }
        });
        dialogRef.afterClosed().subscribe(r => {
          if (dialogRef.componentInstance.changesWereMade) {
            this.setDispatchTasks();
          }
          dialogRef.close();
        });
        break;
      }
      case ('addDependent'): {
        // add a dependent DispatchTask
        const dialogRef = this.dialog.open(TaskEditComponent);
        dialogRef.afterOpened().subscribe(r => {
          dialogRef.componentInstance.dispatchTask = dialogRef.componentInstance.blankTask;
          dialogRef.componentInstance.dispatchTask.triggerTaskId = dispatchTaskId;
          switch (this.dispatchTaskSource.type) {
            case DispatchTaskSourceType.Scenario:
              dialogRef.componentInstance.dispatchTask.scenarioId = this.dispatchTaskSource.id;
              break;
            case DispatchTaskSourceType.Session:
            case DispatchTaskSourceType.SessionActive:
              dialogRef.componentInstance.dispatchTask.sessionId = this.dispatchTaskSource.id;
              break;
            default:
              break;
          }
        });
        dialogRef.afterClosed().subscribe(r => {
          if (dialogRef.componentInstance.changesWereMade) {
            // if a DispatchTask was added, then show it
            if (dialogRef.componentInstance.newDispatchTaskId) {
              this.expandedNodeSet.add(dialogRef.componentInstance.newDispatchTaskId);
            }
            this.setDispatchTasks();
          }
          dialogRef.close();
        });
        break;
      }
      case ('edit'): {
        // Edit exercise
        this.dispatchTaskService.getDispatchTask(dispatchTaskId)
          .subscribe(editDispatchTask => {
            const dialogRef = this.dialog.open(TaskEditComponent);
            dialogRef.afterOpened().subscribe(r => {
              dialogRef.componentInstance.dispatchTask = editDispatchTask;
            });
            dialogRef.afterClosed().subscribe(r => {
              if (dialogRef.componentInstance.changesWereMade) {
                this.setDispatchTasks();
              }
              dialogRef.close();
            });
        });
        break;
      }
      case ('execute'): {
        this.dialogService.confirm('Execute Task', 'Are you sure that you want to execute this task?').subscribe(result => {
          if (result['confirm']) {
            this.dispatchTaskService.executeDispatchTask(dispatchTaskId).subscribe(() => {
              this.setDispatchTasks();
            });
          }
        });
        break;
      }
      default: {
        alert('Unknown Action');
        break;
      }
    }
  }

  /**
   * Delete a dispatch task after confirmation
   */
  deleteDispatchTask(node: FlatDispatchTaskNode): void {
    this.dialogService.confirm('Delete Dispatch Task', 'Are you sure that you want to delete ' + node.dispatchTask.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.dispatchTaskService.deleteDispatchTask(node.dispatchTask.id)
            .subscribe(deleted => {
              console.log('successfully deleted dispatch task');
              this.setDispatchTasks();
            });
        }
      });
  }

  /**
   * This constructs an array of nodes that matches the DOM,
   * and calls rememberExpandedTreeNodes to persist expand state
   */
  visibleNodes(): DispatchTaskNode[] {
    this.rememberExpandedTreeNodes(this.treeControl, this.expandedNodeSet);
    const result = [];

    function addExpandedChildren(node: DispatchTaskNode, expanded: Set<string>) {
      result.push(node);
      if (expanded.has(node.dispatchTask.id)) {
        node.children.map(child => addExpandedChildren(child, expanded));
      }
    }
    this.dataSource.data.forEach(node => {
      addExpandedChildren(node, this.expandedNodeSet);
    });
    return result;
  }

/**
   * The following methods are for persisting the tree expand state
   * after being rebuilt
   */

  rebuildTreeForData(data: any) {
    this.rememberExpandedTreeNodes(this.treeControl, this.expandedNodeSet);
    this.dataSource.data = data;
    this.forgetMissingExpandedNodes(this.treeControl, this.expandedNodeSet);
    this.expandNodesById(this.treeControl.dataNodes, Array.from(this.expandedNodeSet));
  }

  private rememberExpandedTreeNodes(
    treeControl: FlatTreeControl<FlatDispatchTaskNode>,
    expandedNodeSet: Set<string>
  ) {
    if (treeControl.dataNodes) {
      treeControl.dataNodes.forEach((node) => {
        if (treeControl.isExpandable(node) && treeControl.isExpanded(node)) {
          // capture latest expanded state
          expandedNodeSet.add(node.dispatchTask.id);
        }
      });
    }
  }

  private forgetMissingExpandedNodes(
    treeControl: FlatTreeControl<FlatDispatchTaskNode>,
    expandedNodeSet: Set<string>
  ) {
    if (treeControl.dataNodes) {
      expandedNodeSet.forEach((nodeId) => {
        // maintain expanded node state
        if (!treeControl.dataNodes.find((n) => n.dispatchTask.id === nodeId)) {
          // if the tree doesn't have the previous node, remove it from the expanded list
          expandedNodeSet.delete(nodeId);
        }
      });
    }
  }

  private expandNodesById(flatNodes: FlatDispatchTaskNode[], ids: string[]) {
    if (!flatNodes || flatNodes.length === 0) return;
    const idSet = new Set(ids);
    return flatNodes.forEach((node) => {
      if (idSet.has(node.dispatchTask.id)) {
        this.treeControl.expand(node);
        let parent = this.getParentNode(node);
        while (parent) {
          this.treeControl.expand(parent);
          parent = this.getParentNode(parent);
        }
      }
    });
  }

  private getParentNode(node: FlatDispatchTaskNode): FlatDispatchTaskNode | null {
    const currentLevel = node.level;
    if (currentLevel < 1) {
      return null;
    }
    const startIndex = this.treeControl.dataNodes.indexOf(node) - 1;
    for (let i = startIndex; i >= 0; i--) {
      const currentNode = this.treeControl.dataNodes[i];
      if (currentNode.level < currentLevel) {
        return currentNode;
      }
    }
    return null;
  }

  sortedResults(results: DispatchTaskResult[], sortBy: string, sortDescending: boolean) {
    const sortValue = sortDescending ? 1 : -1;
    const sortedResults = results.sort((a, b) => a[sortBy] < b[sortBy] ? sortValue : -1 * sortValue);
    return sortedResults;
  }

}

