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
  ChangeDetectorRef,
  Component,
  EventEmitter,
  HostListener,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import {
  Breadcrumb,
  ProjectObjectType,
  ProjectQuery,
  ProjectService,
  ProjectUI,
  Tab,
} from '../../../state';
import { DirectoryQuery } from 'src/app/directories/state';
import { FileQuery, FileService } from 'src/app/files/state';
import { WorkspaceQuery } from 'src/app/workspace/state';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatTab } from '@angular/material/tabs';
import {
  Directory,
  Project,
  ModelFile,
  Module,
  Workspace,
} from 'src/app/generated/caster-api';
import { EMPTY, iif, merge, Observable, Subject, Subscription } from 'rxjs';
import {
  catchError,
  filter,
  map,
  mergeMap,
  shareReplay,
  switchMap,
  take,
  takeUntil,
  tap,
  withLatestFrom,
} from 'rxjs/operators';
import { ModuleQuery, ModuleService } from 'src/app/modules/state';
import { ConfirmDialogComponent } from 'src/app/sei-cwd-common/confirm-dialog/components/confirm-dialog.component';
import { CanComponentDeactivate } from 'src/app/sei-cwd-common/cwd-route-guards/can-deactivate.guard';
import { CurrentUserQuery } from 'src/app/users/state';

const WAS_CANCELLED = 'wasCancelled';
type TabSubscription = { id: string; subscription: Subscription };

@Component({
  selector: 'cas-project-tab',
  templateUrl: './project-tab.component.html',
  styleUrls: ['./project-tab.component.scss'],
})
export class ProjectTabComponent
  implements OnInit, OnDestroy, OnChanges, CanComponentDeactivate {
  @Input() project: Project;
  @Input() projectUI: ProjectUI;
  @Output() closeTab: EventEmitter<string> = new EventEmitter<string>();
  @Output() tabChanged: EventEmitter<{
    index: number;
    tab: MatTab;
  }> = new EventEmitter<{ index: number; tab: MatTab }>();

  public tabType = ProjectObjectType;
  // public selectedTab: Tab;
  public selectedTab$: Subscription;
  public modules$: Observable<Module[]>;
  public showTabs: boolean;
  public projectObjectNames = new Map<string, string>();
  private nonactiveTabSubscriptions = new Array<TabSubscription>();
  private tabChangeRequested$ = new Subject();
  private unsubscribe$ = new Subject();
  public sidebarOpen$: Observable<boolean>;
  public sidebarView$: Observable<string>;
  public sidebarWidth$: Observable<number>;
  public breadcrumb$: Observable<Breadcrumb[]>;

  constructor(
    private projectService: ProjectService,
    private projectQuery: ProjectQuery,
    private moduleService: ModuleService,
    private moduleQuery: ModuleQuery,
    private directoryQuery: DirectoryQuery,
    private workspaceQuery: WorkspaceQuery,
    private fileQuery: FileQuery,
    private fileService: FileService,
    private dialog: MatDialog,
    private currentUserQuery: CurrentUserQuery,
    private changeDetectorRef: ChangeDetectorRef
  ) {}

  /**
   * ngOninit handles initialization required for all open tabs
   */
  ngOnInit() {
    this.moduleService.load().pipe(take(1)).subscribe();
    this.modules$ = this.moduleQuery.selectAll();
    this.sidebarOpen$ = this.projectQuery.getRightSidebarOpen$(this.project.id);
    this.sidebarView$ = this.projectQuery.getRightSidebarView$(this.project.id);
    this.sidebarWidth$ = this.projectQuery.getRightSidebarWidth(
      this.project.id
    );

    // Subscribe when the tab changes.
    this.selectedTab$ = this.projectQuery
      .selectSelectedTab(this.project.id)
      .pipe(
        filter((selectedTab) => selectedTab >= 0),
        // since the selected tab is only an index we need to get the open tabs data from the openTabs array.
        // we use withLatestFrom() so this is only triggered the the selected tab is changed.
        withLatestFrom(this.projectQuery.selectOpenTabs(this.project.id)),
        // return the Tabs data
        map(([selectedTab, openTabs]) => {
          const tab = openTabs[selectedTab];
          this.showTabs = !!openTabs && openTabs.length > 0 ? true : false;

          if (tab) {
            this.updateBreadcrumb(tab);
          }

          return tab;
        }),
        filter((tab) => tab != null),
        // Set the breadcrumb based on the currently selected tab.
        tap((tab: Tab) => {
          this.breadcrumb$ = this.projectQuery.selectTabBreadcrumb(
            this.project.id,
            tab.id
          );
          this.updateBreadcrumb(tab);
        }),
        // Use a switchMap to detect the tab type and call the appropriate query.
        switchMap((tab: Tab) => {
          switch (tab.type) {
            case ProjectObjectType.FILE:
              return this.fileQuery.selectEntity(tab.id).pipe(
                // replaces the results selector and returns the tab along with the file entity.
                map((file) => [tab, file])
              );
            case ProjectObjectType.WORKSPACE:
              return this.workspaceQuery.selectEntity(tab.id).pipe(
                // replaces the results selector and returns the tab along with the workspace entity.
                map((workspace) => [tab, workspace])
              );
            case ProjectObjectType.DIRECTORY:
              return this.directoryQuery.selectEntity(tab.id).pipe(
                // replaces the results selector and returns the tab along with the directory entity.
                map((directory) => [tab, directory])
              );
          }
        }),
        // Filter for undefined entities. Typically undefined on initial load. (prevents errors)
        filter(([tab, entity]) => entity !== undefined),
        switchMap(([tab, entity]: [Tab, any]) => {
          switch (tab.type) {
            case ProjectObjectType.FILE:
              return iif(
                () => entity.content == null,
                this.fileService.loadFile(entity.id).pipe(take(1)),
                EMPTY
              );

            case ProjectObjectType.WORKSPACE:
              return EMPTY;
          }
        }),
        shareReplay(),
        // unsubscribe automatically when the component is destroyed.
        takeUntil(this.unsubscribe$)
      )
      .subscribe();

    // Subscribe to each open tab for changes.
    this.projectQuery
      .selectOpenTabs(this.project.id)
      .pipe(
        tap((openTabs) => {
          openTabs.forEach((tab) => this.updateBreadcrumb(tab));
        }),
        switchMap((openTabs) => {
          const subs = openTabs.map((tab) =>
            this.projectQuery
              .selectTabBreadcrumb(this.project.id, tab.id)
              .pipe(take(1))
          );
          return merge(subs);
        }),
        mergeMap((tabSub) => tabSub),
        tap((obj) => {
          this.watchForChanges();
        }),
        shareReplay(),
        takeUntil(this.unsubscribe$),
        catchError((err) => {
          console.log(err);
          return EMPTY;
        })
      )
      .subscribe();

    this.setProjectObjectNames();
  }

  ngOnChanges(changes: SimpleChanges) {}

  watchForChanges() {
    // unsubscribe from closed tab changes
    for (let i = this.nonactiveTabSubscriptions.length - 1; i >= 0; i--) {
      const naTab = this.nonactiveTabSubscriptions[i];
      const isClosed =
        !this.showTabs ||
        !this.projectUI.openTabs.some((t) => t.id === naTab.id);
      if (isClosed) {
        naTab.subscription.unsubscribe();
        this.nonactiveTabSubscriptions.splice(i, 1);
      }
    }
    // subscribe to open tab changes
    if (this.showTabs) {
      for (let i = 0; i < this.projectUI.openTabs.length; i++) {
        if (i === this.projectUI.selectedTab) {
          // subscribe to the total path for the selected tab to get path changes for breadcrumb
          const tabBreadcrumbs = this.projectUI.openTabs[i].breadcrumb;
          if (tabBreadcrumbs) {
            tabBreadcrumbs.forEach((bc) => {
              switch (bc.type) {
                case ProjectObjectType.FILE:
                  this.fileQuery
                    .selectEntity(bc.id)
                    .pipe(takeUntil(this.tabChangeRequested$))
                    .subscribe((f) => {
                      this.fileChangeHandler(f);
                    });
                  break;
                case ProjectObjectType.DIRECTORY:
                  this.fileQuery
                    .selectEntity(bc.id)
                    .pipe(takeUntil(this.tabChangeRequested$))
                    .subscribe((d) => {
                      this.directoryChangeHandler(d);
                    });
                  break;
                case ProjectObjectType.WORKSPACE:
                  this.workspaceQuery
                    .selectEntity(bc.id)
                    .pipe(takeUntil(this.tabChangeRequested$))
                    .subscribe((w) => {
                      this.workspaceChangeHandler(w);
                    });
                  break;
                default:
                  break;
              }
            });
          }
        } else {
          // subscribe to the file or workspace name for other open tabs
          const tab = this.projectUI.openTabs[i];
          const alreadyWatching = this.nonactiveTabSubscriptions.some(
            (ts) => ts.id === tab.id
          );
          if (!alreadyWatching) {
            switch (tab.type) {
              case ProjectObjectType.FILE:
                this.nonactiveTabSubscriptions[
                  tab.id
                ] = this.fileQuery.selectEntity(tab.id).subscribe((f) => {
                  this.fileChangeHandler(f);
                });
                break;
              case ProjectObjectType.WORKSPACE:
                this.nonactiveTabSubscriptions[
                  tab.id
                ] = this.workspaceQuery.selectEntity(tab.id).subscribe((f) => {
                  this.workspaceChangeHandler(f);
                });
                break;
              default:
                break;
            }
          }
        }
      }
    }
  }

  setProjectObjectNames() {
    if (this.showTabs) {
      this.projectUI.openTabs.forEach((tab) => {
        this.projectObjectNames[tab.id] = tab.name;
        if (!!tab.breadcrumb) {
          tab.breadcrumb.forEach((bc) => {
            this.projectObjectNames[bc.id] = bc.name;
          });
        }
      });
      this.changeDetectorRef.markForCheck();
    }
  }

  fileChangeHandler(file: ModelFile) {
    if (file && file.name !== this.projectObjectNames[file.id]) {
      this.projectObjectNames[file.id] = file.name;
      // this.updateBreadcrumb();
      this.changeDetectorRef.markForCheck();
    }
  }

  directoryChangeHandler(d: Directory) {
    if (d && d.name !== this.projectObjectNames[d.id]) {
      this.projectObjectNames[d.id] = d.name;
      // this.updateBreadcrumb();
      this.changeDetectorRef.markForCheck();
    }
  }

  workspaceChangeHandler(ws: Workspace) {
    if (ws && ws.name !== this.projectObjectNames[ws.id]) {
      this.projectObjectNames[ws.id] = ws.name;
      // this.updateBreadcrumb();
      this.changeDetectorRef.markForCheck();
    }
  }

  /**
   * This method is used to CREATE and UPDATE breadcrumbs
   * @param updatedProjectObjectId the ID of the project object that was changed that necessitated the change
   *                                A null value indicates that all breadcrumbs should be created.
   */
  updateBreadcrumb(tab: Tab) {
    let newBreadcrumb: Breadcrumb[];
    let obj: ModelFile | Workspace;

    switch (tab.type) {
      case ProjectObjectType.FILE:
        obj = this.fileQuery.getEntity(tab.id);
        break;
      case ProjectObjectType.WORKSPACE:
        obj = this.workspaceQuery.getEntity(tab.id);
        break;
    }
    if (obj) {
      this.projectObjectNames[tab.id] = obj.name;
      newBreadcrumb = this.projectService.createBreadcrumb(
        this.project,
        obj,
        tab.type
      );
      this.projectService.updateTabBreadcrumb(tab.id, newBreadcrumb);
      /*
      this.projectQuery.selectTabBreadcrumb(this.project.id, obj.id).pipe(takeUntil(this.tabChangeRequested$)).subscribe(breadcrumb => {
        this.watchForChanges();
      });
      this.changeDetectorRef.markForCheck();
      */
    } else {
      console.log(
        tab.type.toString() + ': ' + tab.id + ' was not found with the query.'
      );
    }
  }

  closeTabClickHandler(tab: Tab) {
    switch (tab.type) {
      case ProjectObjectType.FILE:
        if (this.isFileContentChanged(tab.id)) {
          this.confirmDialog(
            'Not all changes have been saved!!!',
            'Would you like to continue closing and discard your changes?',
            { buttonTrueText: 'Discard Changes', buttonFalseText: 'Cancel' }
          ).subscribe((result) => {
            if (!result[WAS_CANCELLED]) {
              this.closeFileTab(tab.id);
            }
          });
        } else {
          this.closeFileTab(tab.id);
        }
        break;
      case ProjectObjectType.WORKSPACE:
        this.closeTab.emit(tab.id);
        break;
      default:
        break;
    }
  }

  closeFileTab(fileId: string) {
    const file = this.fileQuery.getEntity(fileId);

    if (file && file.lockedById) {
      const userId = this.currentUserQuery.getValue().id;

      if (file.lockedById === userId) {
        this.fileService.unlockFile(fileId);
      }
    }

    // discard changes
    this.fileService.updateEditorContent(file.id, file.content);
    this.fileService.setSave(file.id, true);

    this.closeTab.emit(fileId);
  }

  tabChangeClickHandler($event) {
    this.tabChanged.emit($event);
    this.tabChangeRequested$.next();
  }

  isFileContentChanged(fileId: string): boolean {
    const file = this.fileQuery.getEntity(fileId);
    return file && file.editorContent !== file.content;
  }

  confirmDialog(
    title: string,
    message: string,
    data?: any
  ): Observable<boolean> {
    let dialogRef: MatDialogRef<ConfirmDialogComponent>;
    dialogRef = this.dialog.open(ConfirmDialogComponent, { data: data || {} });
    dialogRef.componentInstance.title = title;
    dialogRef.componentInstance.message = message;

    return dialogRef.afterClosed();
  }

  sidebarChangedFn(event) {
    this.projectService.setRightSidebarOpen(this.project.id, event);
  }

  sidebarViewChangedFn(event) {
    this.projectService.setRightSidebarView(this.project.id, event);
  }

  sidebarWidthChangedFn(event) {
    this.projectService.setRightSidebarWidth(this.project.id, event);
  }

  // @HostListener handles browser refresh, close, etc.
  @HostListener('window:beforeunload')
  canDeactivate(): Observable<boolean> | Promise<boolean> | boolean {
    // check if there are pending changes
    // returning true will navigate without confirmation
    // returning false will show a confirm dialog before navigating away
    const thereAreUnsavedChanges =
      this.projectUI.openTabs &&
      this.projectUI.openTabs.some((tab) => {
        return (
          tab.type === ProjectObjectType.FILE &&
          this.isFileContentChanged(tab.id)
        );
      });
    return !thereAreUnsavedChanges;
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
    this.tabChangeRequested$.next();
    this.tabChangeRequested$.complete();
  }
}
