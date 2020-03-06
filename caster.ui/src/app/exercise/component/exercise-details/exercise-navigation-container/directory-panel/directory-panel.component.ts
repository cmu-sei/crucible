/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Input, OnDestroy, ViewChild } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { DirectoryQuery, DirectoryService, DirectoryUI } from '../../../../../directories/state';
import { FileService, FileQuery } from '../../../../../files/state';
import { WorkspaceService, WorkspaceQuery, StatusFilter } from '../../../../../workspace/state';
import { MatDialogRef, MatDialog, MatMenuTrigger, getMatFormFieldPlaceholderConflictError } from '@angular/material';
import { ConfirmDialogComponent } from 'src/app/sei-cwd-common/confirm-dialog/components/confirm-dialog.component';
import { NameDialogComponent } from 'src/app/sei-cwd-common/name-dialog/name-dialog.component';
import { Workspace, Run, Directory, ModelFile } from 'src/app/generated/caster-api';
import { ExerciseService, ExerciseObjectType } from 'src/app/exercise/state';

const WAS_CANCELLED = 'wasCancelled';
const NAME_VALUE = 'nameValue';

@Component({
  selector: 'cas-directory-panel',
  templateUrl: './directory-panel.component.html',
  styleUrls: ['./directory-panel.component.scss']
})
export class DirectoryPanelComponent implements OnInit, OnDestroy {

  @Input() parentDirectory: Directory;
  public parentDirectoryUI$: Observable<DirectoryUI>;
  public panelOpenState = false;
  public directories$: Observable<Directory[]>;
  public files$: Observable<ModelFile[]>;
  public parentFiles$: Observable<ModelFile[]>;
  public workspaces$: Observable<Workspace[]>;
  public ExerciseObjectType = ExerciseObjectType; // For usage in html

  private _destroyed$ = new Subject();

  @ViewChild(MatMenuTrigger, null) contextMenu: MatMenuTrigger;

  contextMenuPosition = { x: '0px', y: '0px' };

  constructor(
    private directoryService: DirectoryService,
    private directoryQuery: DirectoryQuery,
    private fileService: FileService,
    private fileQuery: FileQuery,
    private workspaceQuery: WorkspaceQuery,
    private workspaceService: WorkspaceService,
    private exerciseService: ExerciseService,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    if (this.parentDirectory) {
      this.parentDirectoryUI$ = this.directoryQuery.ui.selectEntity(this.parentDirectory.id);

      this.directories$ = this.directoryQuery.selectAll(
        { filterBy: (d => d.parentId === this.parentDirectory.id && d.exerciseId === this.parentDirectory.exerciseId) });

      this.files$ = this.fileQuery.selectAll(
        { filterBy: f => (f.directoryId === this.parentDirectory.id) });
      this.parentFiles$ = this.fileQuery.selectAll(
        { filterBy: f => (f.directoryId === this.parentDirectory.id && f.workspaceId === null) });

      this.workspaces$ = this.workspaceQuery.selectAll(
        { filterBy: (w => w.directoryId === this.parentDirectory.id) });
    }
  }

  ngOnDestroy() {
    this._destroyed$.next();
    this._destroyed$.complete();
  }

  confirmDialog(title: string, message: string, data?: any): Observable<boolean> {
    let dialogRef: MatDialogRef<ConfirmDialogComponent>;
    dialogRef = this.dialog.open(ConfirmDialogComponent, { data: data || {} });
    dialogRef.componentInstance.title = title;
    dialogRef.componentInstance.message = message;

    return dialogRef.afterClosed();
  }

  nameDialog(title: string, message: string, data?: any): Observable<boolean> {
    let dialogRef: MatDialogRef<NameDialogComponent>;
    dialogRef = this.dialog.open(NameDialogComponent, { data: data || {} });
    dialogRef.componentInstance.title = title;
    dialogRef.componentInstance.message = message;

    return dialogRef.afterClosed();
  }

  deleteDirectory(dir: Directory) {
    this.confirmDialog('Delete Directory?', 'Delete directory ' + dir.name + '?', { buttonTrueText: 'Delete' }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        this.directoryService.delete(dir.id);
      }
    });
  }

  createNewDirectory(dirId?: string) {
    this.nameDialog('Create New Directory?', '', { nameValue: '' }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        const newDir = {
          name: result[NAME_VALUE],
          exerciseId: this.parentDirectory.exerciseId,
          parentId: dirId,
        };
        this.directoryService.add(newDir);
      }
    });
  }

  renameDirectory() {
    this.nameDialog('Rename ' + this.parentDirectory.name, '', { nameValue: this.parentDirectory.name }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        console.log(result[NAME_VALUE]);
        const updatedDirectory = { ...this.parentDirectory, name: result[NAME_VALUE] } as Directory;
        this.directoryService.update(updatedDirectory);
      }
    });
  }


  createFile(dirId: string, workspaceId?: string) {
    this.nameDialog('Create New File?', '', { nameValue: '' }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        const newFile = {
            workspaceId: (workspaceId) ? workspaceId : null,
            directoryId: dirId,
            content: '',
            name: result[NAME_VALUE]
          } as ModelFile;
        this.fileService.add(newFile);
      }
    });
  }

  renameFile(file: ModelFile) {
    this.nameDialog('Rename ' + file.name, '', { nameValue: file.name }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        this.fileService.renameFile(file.id, result[NAME_VALUE]);
      }
    });
  }

  deleteFile(file: ModelFile) {
    this.confirmDialog('Delete File?', 'Delete file ' + file.name + '?', { buttonTrueText: 'Delete' }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        this.fileService.delete(file);
      }
    });
  }

  renameWorkspace(workspace: Workspace) {
    this.nameDialog('Rename ' + workspace.name, '', { nameValue: workspace.name }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        const newWorkspace = { ...workspace, name: result[NAME_VALUE] };
        this.workspaceService.update(newWorkspace);
      }
    });
  }

  createWorkspace() {
    this.nameDialog('Create New Workspace?', '', { nameValue: '' }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        const newWorkspace =
          {
            directoryId: this.parentDirectory.id,
            name: result[NAME_VALUE],
            statusFilter: new Array<StatusFilter>(),
            runs: new Array<Run>(),
          } as Workspace;
        this.workspaceService.add(newWorkspace);
      }
    });
  }

  deleteWorkspace(workspace: Workspace) {
    this.confirmDialog('Delete workspace?', 'Delete workspace ' + workspace.name + '?', { buttonTrueText: 'Delete' }).subscribe(result => {
      if (!result[WAS_CANCELLED]) {
        this.workspaceService.delete(workspace);
      }
    });
  }

  toggleIsExpanded(dirUI: DirectoryUI) {
    this.directoryService.toggleIsExpanded(dirUI);
  }

  toggleIsFilesExpanded(dirUI: DirectoryUI) {
    this.directoryService.toggleIsFilesExpanded(dirUI);
  }

  toggleIsWorkspacesExpanded(dirUI: DirectoryUI) {
    this.directoryService.toggleIsWorkspacesExpanded(dirUI);
  }

  toggleIsDirectoriesExpanded(dirUI: DirectoryUI) {
    this.directoryService.toggleIsDirectoriesExpanded(dirUI);
  }

  openFile(file: ModelFile) {
    // open a file type tab
    this.exerciseService.openTab(file, ExerciseObjectType.FILE);
  }

  openWorkspace(workspace: Workspace) {
    this.exerciseService.openTab(workspace, ExerciseObjectType.WORKSPACE);
  }

  toggleIsWorkspaceExpanded(workspaceId: string) {
    this.workspaceService.toggleIsExpanded(workspaceId);
  }

  isWorkspaceExpanded(workspaceId: string): boolean {
    return this.workspaceService.isExpanded(workspaceId);
  }

  onContextMenu(event: MouseEvent, obj: ModelFile | Workspace | Directory, objectType: ExerciseObjectType) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + 'px';
    this.contextMenuPosition.y = event.clientY + 'px';
    this.contextMenu.menuData = { item: { object: obj, type: objectType } };
    this.contextMenu.menu.focusFirstItem('mouse');
    this.contextMenu.openMenu();
  }

  onContextRename(obj: any) {
    const type = obj.type as ExerciseObjectType;
    switch (type) {
      case ExerciseObjectType.DIRECTORY: {
        this.renameDirectory();
        break;
      }
      case ExerciseObjectType.WORKSPACE: {
        this.renameWorkspace(obj.object as Workspace);
        break;
      }
      case ExerciseObjectType.FILE: {
        this.renameFile(obj.object as ModelFile);
      }
    }
  }

  onContextDelete(obj: any) {
    const type = obj.type as ExerciseObjectType;
    switch (type) {
      case ExerciseObjectType.DIRECTORY: {
        this.deleteDirectory(obj.object as Directory);
        break;
      }
      case ExerciseObjectType.WORKSPACE: {
        this.deleteWorkspace(obj.object as Workspace);
        break;
      }
      case ExerciseObjectType.FILE: {
        this.deleteFile(obj.object as ModelFile);
      }
    }
  }


}

