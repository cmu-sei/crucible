/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {ChangeDetectionStrategy, Component, HostListener, OnDestroy, OnInit} from '@angular/core';
import {Breadcrumb, ProjectObjectType, ProjectQuery, ProjectService, ProjectUI} from '../../../state';
import {Exercise as Project} from 'src/app/generated/caster-api';
import {combineLatest, Observable, Subject} from 'rxjs';
import {CurrentUserQuery, CurrentUserState, UserService} from 'src/app/users/state';
import {CwdAuthService, CwdSettingsService} from 'src/app/sei-cwd-common';
import {CanComponentDeactivate} from 'src/app/sei-cwd-common/cwd-route-guards/can-deactivate.guard';
import {FileQuery} from 'src/app/files/state';
import {filter, switchMap, takeUntil, tap} from 'rxjs/operators';
import {SignalRService} from 'src/app/shared/signalr/signalr.service';

@Component({
  selector: 'cas-project-collapse-container',
  templateUrl: './project-collapse-container.component.html',
  styleUrls: ['./project-collapse-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectCollapseContainerComponent implements OnInit, OnDestroy, CanComponentDeactivate {

  public project$: Observable<Project>;
  public project: Project;
  public projectUI$: Observable<ProjectUI>;
  private projectUI: ProjectUI;
  public loading$: Observable<boolean>;
  public leftSidebarOpen: boolean;
  public leftSidebarWidth: number;
  public currentUser$: Observable<CurrentUserState>;

  public topBarColor = '#0FABEA';
  public titleText = 'Caster';

  private unsubscribe$ = new Subject();

  constructor(
    private projectQuery: ProjectQuery,
    private projectService: ProjectService,
    private userService: UserService,
    private currentUserQuery: CurrentUserQuery,
    private authService: CwdAuthService,
    private settingsService: CwdSettingsService,
    private fileQuery: FileQuery,
    private signalRService: SignalRService
    ) { }

  ngOnInit() {
    this.loading$ = this.projectQuery.selectLoading();
    this.project$ = this.projectQuery.selectActive();
    this.projectUI$ =  this.projectQuery.ui.selectActive();
    this.projectUI$.pipe(takeUntil(this.unsubscribe$)).subscribe(exUi => this.projectUI = exUi);

    this.currentUser$ = this.currentUserQuery.select();
    this.userService.setCurrentUser();

    // Set the topbar color from config file
    this.topBarColor = this.settingsService.settings.AppTopBarHexColor;

    // Set the page title from configuration file
    this.titleText = this.settingsService.settings.AppTopBarText;

    this.project$.pipe(
      // Sometimes the project variable is undefined. We filter for those since were dependent on the project object.
      filter(e => e !== undefined),
      // Use switchMap to get the value of project.id since sidebar properties are dependant.
      switchMap((e: Project) => {
        return combineLatest(
          this.projectQuery.getLeftSidebarOpen(e.id),
          this.projectQuery.getLeftSidebarWidth(e.id),
          // Use a results selector to return all 3 values.
          (open: boolean, width: number) => {
            return {e, open, width};
          }
        );
      }),
      // set the values of the sidebar without affecting the subscription.
      tap(({e, open, width}) => {
         this.leftSidebarOpen = open;
         this.leftSidebarWidth = width;
      }),
      takeUntil(this.unsubscribe$)
    ).subscribe(({e, open, width}) => {
      if (e) {
        this.project = e;
        this.signalRService.startConnection().then(() => {
          this.signalRService.joinExercise(e.id);
        })
        .catch(err => {
          console.log(err);
        });
      }
    });
  }

  closeTab(id: string) {
    this.projectService.closeTab(id);
  }

  tabChangedFn({index, tab}) {
    this.projectService.setSelectedTab(index);
  }

  breadcrumbClickedFn(breadcrumb: Breadcrumb) {
    // Note:  Breadcrumbclicked is fired when user clicks but nothing is done at this time.
  }

  resizingFn(event) {
    this.leftSidebarWidth = event.rectangle.width;
  }

  resizeEndFn(event) {
    this.projectService.setLeftSidebarWidth(this.project.id, event.rectangle.width);
  }

  leftSidebarOpenFn(event) {
    this.projectService.setLeftSidebarOpen(this.project.id, event);
  }

  logout(): void {
    this.authService.logout();
  }

  isFileContentChanged(fileId: string): boolean {
    const file = this.fileQuery.getEntity(fileId);
    return file && file.editorContent !== file.content;
  }

  // @HostListener handles browser refresh, close, etc.
  @HostListener('window:beforeunload')
  canDeactivate(): Observable<boolean> | Promise<boolean> | boolean {
    // check if there are pending changes
    // returning true will navigate without confirmation
    // returning false will show a confirm dialog before navigating away
    const thereAreUnsavedChanges = this.projectUI.openTabs.some(tab => {
      return tab.type === ProjectObjectType.FILE && this.isFileContentChanged(tab.id);
    });
    return !thereAreUnsavedChanges;
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
    this.signalRService.leaveExercise(this.projectQuery.getActive().id);
  }
}

