/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { FlatTreeControl } from '@angular/cdk/tree';
import { MatTreeFlatDataSource, MatTreeFlattener, MatDialog, MatMenuTrigger } from '@angular/material';
import { TaskEditComponent } from 'src/app/components/tasks/task-edit/task-edit.component';
import { Task, Result } from 'src/app/swagger-codegen/dispatcher.api';
import { DialogService } from 'src/app/services/dialog/dialog.service';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

interface  TaskNode {
  task: Task;
  results?: Result[];
  children?: TaskNode[];
}

interface  FlatTaskNode {
  expandable: boolean;
  level: number;
  task: Task;
  results?: Result[];
}

const BLANK_TASK: Task = {
  name: '',
  description: '',
  action: Task.ActionEnum.GuestProcessRun,
  vmMask: '',
  vmList: [],
  apiUrl: '',
  inputString: '',
  expectedOutput: '',
  delaySeconds: 0,
  expirationSeconds: 0,
  intervalSeconds: 0,
  iterations: 0,
  triggerCondition: Task.TriggerConditionEnum.Manual,
};

@Component({
  selector: 'app-task-tree',
  templateUrl: './task-tree.component.html',
  styleUrls: ['./task-tree.component.css']
})
export class TaskTreeComponent implements OnInit, OnDestroy {

  @Input() taskList: Observable<Task[]>;
  @Input() resultList: Observable<Result[]>;
  @Input() isLoading: boolean;
  @Input() scenarioTemplateId: string;
  @Input() scenarioId: string;
  @Input() userId: string;
  @Input() isEditableState: boolean;
  @Input() isExecutableState: boolean;
  @Input() clipboard: any;
  @Output() taskSelected = new EventEmitter<string>();
  @Output() saveTask = new EventEmitter<Task>();
  @Output() deleteTaskRequested = new EventEmitter<string>();
  @Output() executeRequested = new EventEmitter<string>();
  @Output() sendToClipboard = new EventEmitter<any>();
  @Output() pasteClipboard = new EventEmitter<string>();
  @ViewChild(FlatTreeControl) flatTreeControl;
  selectedTaskId = '';
  manualTrigger = Task.TriggerConditionEnum.Manual;
  private tasks = new Array<Task>();
  private results = new Array<Result>();
  private expandedNodeSet = new Set<string>();
  public expandedDetails = new Set<string>();
  private unsubscribe = new Subject();

  // tree controls
  private transformer = (node: TaskNode, level: number) => {
    return {
      expandable: !!node.children && node.children.length > 0,
      level: level,
      task: node.task,
      results: node.results
    };
  }
  treeControl = new FlatTreeControl<FlatTaskNode>(node => node.level, node => node.expandable);
  treeFlattener = new MatTreeFlattener(this.transformer, node => node.level, node => node.expandable, node => node.children);
  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

  // context menu
  @ViewChild(MatMenuTrigger, null) contextMenu: MatMenuTrigger;
  contextMenuPosition = { x: '0px', y: '0px' };



  constructor(
    public dialogService: DialogService,
    private dialog: MatDialog
  ) {
  }

  ngOnInit() {
    this.taskList.pipe(takeUntil(this.unsubscribe)).subscribe(tasks => {
      this.tasks = !tasks ? [] : this.filterTasks(tasks);
      this.createTaskNodes();
    });
    if (!!this.resultList) {
      this.resultList.pipe(takeUntil(this.unsubscribe)).subscribe(results => {
        this.results = !results ? [] : results;
        this.createTaskNodes();
      });
    }
  }

  filterTasks(tasks: Task[]): Task[] {
    if (this.scenarioTemplateId) {
      tasks = tasks.filter(item => item.scenarioTemplateId && item.scenarioTemplateId === this.scenarioTemplateId);
    } else if (this.scenarioId) {
      tasks = tasks.filter(item => item.scenarioId && item.scenarioId === this.scenarioId);
    } else if (this.userId) {
      tasks = tasks.filter(item => item.userId && item.userId === this.userId);
    }

    return tasks;
  }

  hasChild = (_: number, node: FlatTaskNode) => node.expandable;

  /**
   * Creates TaskNodes from the tasks
   */
  private createTaskNodes() {
    const newTaskNodes = new Array<TaskNode>();
    this.tasks.forEach(task => {
      if (!task.triggerTaskId) {
        const newNode = this.createTaskNode(task);
        newTaskNodes.push(newNode);
      }
    });
    this.rebuildTreeForData(newTaskNodes);
  }

  /**
   * Creates a TaskNode to display in the tree
   */
  private createTaskNode(parentTask: Task): TaskNode {
    const results = this.results.filter(result => {
      return result.taskId === parentTask.id;
    });
    const newNode: TaskNode = {
      task: parentTask,
      results: results,
      children: new Array<TaskNode>()
    };
    newNode.task = parentTask;
    this.tasks.forEach(childTask => {
      if (childTask.triggerTaskId === parentTask.id) {
        const newChildNode = this.createTaskNode(childTask);
        if (!newNode.children) {
          newNode.children = new Array<TaskNode>();
        }
        newNode.children.push(newChildNode);
      }
    });
    return newNode;
  }

  /**
   * Selects a Task to edit/create
   */
  selectTask(taskId: string) {
    this.selectedTaskId = taskId;
    this.taskSelected.emit(taskId);
  }

  /**
   * Determines if a Task can be edited
   */
  isEditableTask(taskId: string) {
    if (!this.isEditableState) { return false; }
    const results = this.results.filter(r => r.taskId === taskId);
    return (results.length === 0);
  }

    /**
   * Determines if a Task can be executed
   */
  isExecutableTask(task: Task) {
    // must be a manual scenario task
    let isExecutable = this.isExecutableState;
    // if this task has a parent, the parent task must be complete
    if (isExecutable) {
      const triggerTaskId = task.triggerTaskId;
      if (triggerTaskId) {
        const parent = this.tasks.find(dt => dt.id === triggerTaskId);
        const results = this.results.filter(r => r.taskId === triggerTaskId);
        if (results.length === 0) {
          isExecutable = false;
        } else if (results.some(r => r.status === Result.StatusEnum.Pending ||
                                     r.status === Result.StatusEnum.Queued ||
                                     r.status === Result.StatusEnum.Sent)) {
          isExecutable = false;
        }
      }
    }
    return isExecutable;
  }

  onContextMenu(event: MouseEvent, task: Task) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + 'px';
    this.contextMenuPosition.y = event.clientY + 'px';
    this.contextMenu.menuData = { item: task };
    this.contextMenu.menu.focusFirstItem('mouse');
    this.contextMenu.openMenu();
  }

  onContextEdit(task: Task) {
    const dialogRef = this.dialog.open(TaskEditComponent, {
      data: { task: {...task}}
    });
    dialogRef.componentInstance.editComplete.subscribe(result => {
      if (result.saveChanges && result.task) {
        this.saveTask.emit(result.task);
      }
      dialogRef.close();
    });
  }

  onContextCopy(id: string) {
    this.sendToClipboard.emit({ id: id, isCut: false });
  }

  onContextCut(id: string) {
    this.sendToClipboard.emit({ id: id, isCut: true });
  }

  onContextPaste(id: string) {
    this.pasteClipboard.emit(id);
  }

  onContextNew(task: Task) {
    // add a dependent Task
    const newTask = {...BLANK_TASK};
    if (!!this.scenarioTemplateId) {
      newTask.scenarioTemplateId = this.scenarioTemplateId;
    }
    if (!!this.scenarioId) {
      newTask.scenarioId = this.scenarioId;
    }
    newTask.triggerTaskId = task ? task.id : null;
    const dialogRef = this.dialog.open(TaskEditComponent, {
      data: { task: newTask }
    });
    dialogRef.componentInstance.editComplete.subscribe(result => {
      if (result.saveChanges && result.task) {
        this.saveTask.emit(result.task);
      }
      dialogRef.close();
    });
  }

  onContextDelete(task: Task) {
    this.dialogService.confirm('Delete Task', 'Are you sure that you want to delete ' + task.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.deleteTaskRequested.emit(task.id);
        }
      });
  }

  onContextExecute(task: Task) {
    this.dialogService.confirm('Execute Task', 'Are you sure that you want to execute ' + task.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.executeRequested.emit(task.id);
        }
      });
  }

  /**
   * This constructs an array of nodes that matches the DOM,
   * and calls rememberExpandedTreeNodes to persist expand state
   */
  visibleNodes(): TaskNode[] {
    this.rememberExpandedTreeNodes(this.treeControl, this.expandedNodeSet);
    const result = [];

    function addExpandedChildren(node: TaskNode, expanded: Set<string>) {
      result.push(node);
      if (expanded.has(node.task.id)) {
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
    treeControl: FlatTreeControl<FlatTaskNode>,
    expandedNodeSet: Set<string>
  ) {
    if (treeControl.dataNodes) {
      treeControl.dataNodes.forEach((node) => {
        if (treeControl.isExpandable(node) && treeControl.isExpanded(node)) {
          // capture latest expanded state
          expandedNodeSet.add(node.task.id);
        }
      });
    }
  }

  private forgetMissingExpandedNodes(
    treeControl: FlatTreeControl<FlatTaskNode>,
    expandedNodeSet: Set<string>
  ) {
    if (treeControl.dataNodes) {
      expandedNodeSet.forEach((nodeId) => {
        // maintain expanded node state
        if (!treeControl.dataNodes.find((n) => n.task.id === nodeId)) {
          // if the tree doesn't have the previous node, remove it from the expanded list
          expandedNodeSet.delete(nodeId);
        }
      });
    }
  }

  private expandNodesById(flatNodes: FlatTaskNode[], ids: string[]) {
    if (!flatNodes || flatNodes.length === 0) { return; }
    const idSet = new Set(ids);
    return flatNodes.forEach((node) => {
      if (idSet.has(node.task.id)) {
        this.treeControl.expand(node);
        let parent = this.getParentNode(node);
        while (parent) {
          this.treeControl.expand(parent);
          parent = this.getParentNode(parent);
        }
      }
    });
  }

  private getParentNode(node: FlatTaskNode): FlatTaskNode | null {
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

  sortedResults(results: Result[], sortBy: string, sortDescending: boolean) {
    const sortValue = sortDescending ? 1 : -1;
    const sortedResults = results.sort((a, b) => a[sortBy] < b[sortBy] ? sortValue : -1 * sortValue);
    return sortedResults;
  }

  toggleNodeDetails(node: FlatTaskNode) {
    const id = !!node.task ? node.task.id : null;
    if (!!id) {
      if (this.expandedDetails.has(id)) {
        this.expandedDetails.delete(id);
      } else {
        this.expandedDetails.add(id);
      }
    }
  }

  areDetailsExpanded(node: FlatTaskNode) {
    const id = !!node.task ? node.task.id : null;
    const isExpanded = !!id && this.expandedDetails.has(id);
    return isExpanded;
  }

  ngOnDestroy() {
    this.unsubscribe.next();
    this.unsubscribe.complete();
  }

}

