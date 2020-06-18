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
  OnDestroy,
  OnInit,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { Observable, Subject } from 'rxjs';
import {
  ProjectObjectType,
  ProjectQuery,
  ProjectService,
  ProjectStore,
} from '../../../state';
import {
  DirectoryQuery,
  DirectoryService,
} from '../../../../directories/state';
import { Directory, Project } from '../../../../generated/caster-api';
import { map, take, takeUntil } from 'rxjs/operators';
import { RouterQuery } from '@datorama/akita-ng-router-store';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { NameDialogComponent } from 'src/app/sei-cwd-common/name-dialog/name-dialog.component';
import { ProjectExportComponent } from '../project-export/project-export.component';

const WAS_CANCELLED = 'wasCancelled';
const NAME_VALUE = 'nameValue';

@Component({
  selector: 'cas-project-navigation',
  templateUrl: './project-navigation-container.component.html',
  styleUrls: ['./project-navigation-container.component.scss'],
})
export class ProjectNavigationContainerComponent implements OnInit, OnDestroy {
  public project$: Observable<Project>;
  public projectDirectories$: Observable<Directory[]>;
  public projectId: string;
  public objType = ProjectObjectType.PROJECT;

  private unsubscribe$ = new Subject<void>();

  @ViewChild('exportDialog') exportDialog: TemplateRef<ProjectExportComponent>;
  private exportDialogRef: MatDialogRef<ProjectExportComponent>;

  constructor(
    private routerQuery: RouterQuery,
    private projectQuery: ProjectQuery,
    private projectService: ProjectService,
    private projectStore: ProjectStore,
    private directoryQuery: DirectoryQuery,
    private directoryService: DirectoryService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.routerQuery
      .select('state')
      .pipe(
        map((state) => {
          return state.params;
        }),
        takeUntil(this.unsubscribe$)
      )
      .subscribe((params) => {
        this.projectId = params.id;

        if (this.projectId) {
          this.project$ = this.projectQuery.selectActive();
          this.projectDirectories$ = this.directoryQuery.selectAll({
            filterBy: (dir) =>
              dir.projectId === this.projectId && dir.parentId === null,
          });

          // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
          this.projectService
            .loadProject(this.projectId)
            .pipe(take(1))
            .subscribe();
          // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
          this.directoryService
            .loadDirectories(this.projectId)
            .pipe(take(1))
            .subscribe();

          this.projectStore.setActive(this.projectId);
          this.projectStore.ui.setActive(this.projectId);
        }
      });
  }

  nameDialog(title: string, message: string, data?: any): Observable<boolean> {
    let dialogRef: MatDialogRef<NameDialogComponent>;
    dialogRef = this.dialog.open(NameDialogComponent, { data: data || {} });
    dialogRef.componentInstance.title = title;
    dialogRef.componentInstance.message = message;

    return dialogRef.afterClosed();
  }

  createNewDirectory(dirId?: string) {
    // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
    this.nameDialog('Create New Directory?', '', { nameValue: '' })
      .pipe(take(1))
      .subscribe((result) => {
        if (!result[WAS_CANCELLED]) {
          this.directoryService.add({
            name: result[NAME_VALUE],
            projectId: this.projectId,
            parentId: null,
          } as Directory);
        }
      });
  }

  exportProject() {
    this.exportDialogRef = this.dialog.open(this.exportDialog);
  }

  onExportComplete() {
    this.exportDialogRef.close();
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}
