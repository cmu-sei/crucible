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
  ChangeDetectionStrategy,
  Component,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import { Observable } from 'rxjs';
import {
  Resource,
  Run,
  RunStatus,
  Workspace,
} from '../../../generated/caster-api';
import { StatusFilter, WorkspaceQuery, WorkspaceService } from '../../state';
import { shareReplay, take, tap } from 'rxjs/operators';
import { SignalRService } from 'src/app/shared/signalr/signalr.service';
import { Breadcrumb } from 'src/app/project/state';

@Component({
  selector: 'cas-workspace-container',
  templateUrl: './workspace-container.component.html',
  styleUrls: ['./workspace-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkspaceContainerComponent
  implements OnInit, OnDestroy, OnChanges {
  @Input() workspaceId: string;
  @Input() breadcrumb: Breadcrumb[];
  workspaceRuns: Run[];
  workspaceResources: Resource[];
  output: string[];
  isStateView$: Observable<boolean>;
  workspaceView$: Observable<string>;
  loading$: Observable<boolean>;
  expandedRunIds$: Observable<string[]>;
  expandedResourceIds$: Observable<string[]>;
  selectedRunIds$: Observable<any>;
  resourceActionIds$: Observable<string[]>;
  workspaceRuns$: Observable<Run[]>;
  workspaceResources$: Observable<Resource[]>;
  workspace$: Observable<Workspace>;
  statusFilter$: Observable<StatusFilter[]>;
  breadcrumbString = '';

  constructor(
    private workspaceService: WorkspaceService,
    private workspaceQuery: WorkspaceQuery,
    private signalrService: SignalRService
  ) {}

  ngOnInit() {
    this.loading$ = this.workspaceQuery.selectLoading().pipe(shareReplay(1));
    this.workspace$ = this.workspaceQuery.selectEntity(this.workspaceId);
    this.workspaceRuns$ = this.workspaceQuery
      .workspaceRuns$(this.workspaceId)
      .pipe(tap((runs) => (this.workspaceRuns = runs)));
    this.workspaceResources$ = this.workspaceQuery
      .workspaceResources$(this.workspaceId)
      .pipe(
        tap((resources) => {
          this.workspaceResources = resources;
        })
      );
    this.expandedRunIds$ = this.workspaceQuery
      .expandedRuns$(this.workspaceId)
      .pipe(
        tap(() => console.log(`Updating expandedRunIds`)),
        shareReplay(1)
      );
    this.expandedResourceIds$ = this.workspaceQuery
      .expandedResources$(this.workspaceId)
      .pipe(shareReplay(1));
    this.selectedRunIds$ = this.workspaceQuery.selectedRuns$(this.workspaceId);
    this.resourceActionIds$ = this.workspaceQuery
      .resourceActions$(this.workspaceId)
      .pipe(shareReplay(1));
    this.statusFilter$ = this.workspaceQuery.filters$(this.workspaceId);
    // This value is used in multiple times in the template. So we use the shareReplay() operator to prevent multiple
    // subscriptions.
    this.workspaceView$ = this.workspaceQuery
      .getWorkspaceView(this.workspaceId)
      .pipe(shareReplay(1));

    this.signalrService.joinWorkspace(this.workspaceId);
  }

  ngOnChanges(changes: SimpleChanges) {
    if (this.breadcrumb && this.breadcrumb.length > 0) {
      this.breadcrumbString = '';
      this.breadcrumb.forEach((bc) => {
        this.breadcrumbString = this.breadcrumbString + '  >  ' + bc.name;
      });
    }
  }

  plan(event: Event) {
    event.stopPropagation();
    this.workspaceService
      .createPlanRun(this.workspaceId, false)
      .pipe(
        take(1)
        // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
      )
      .subscribe();
  }

  reject(event: Event, run: Run) {
    event.stopPropagation();
    this.workspaceService
      .rejectRun(this.workspaceId, run.id)
      .pipe(
        take(1)
        // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
      )
      .subscribe();
  }

  apply(event: Event, run: Run) {
    event.stopPropagation();
    // TODO: Unsubscribe from this.
    this.workspaceService.applyRun(this.workspaceId, run.id).pipe().subscribe();
  }

  destroy(event: Event) {
    event.stopPropagation();
    // TODO: Unsubscribe from this.
    this.workspaceService.createPlanRun(this.workspaceId, true).subscribe();
  }

  saveState(event: Event, run: Run) {
    event.stopPropagation();
    this.workspaceService
      .saveState(run.id)
      .pipe(
        take(1)
        // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
      )
      .subscribe();
  }

  taint(event: Event, item: Resource) {
    event.stopPropagation();
    this.workspaceService
      .taint(this.workspaceId, item)
      .pipe(take(1))
      .subscribe();
  }

  untaint(event: Event, item: Resource) {
    event.stopPropagation();
    this.workspaceService
      .untaint(this.workspaceId, item)
      .pipe(take(1))
      .subscribe();
  }

  refreshResources() {
    this.workspaceService
      .refreshResources(this.workspaceId)
      .pipe(take(1))
      .subscribe();
  }

  expandRun(event) {
    const { expand, item } = event;
    this.workspaceService.expandRun(expand, item);
  }

  expandResource(event) {
    const { expand, item } = event;
    this.workspaceService.updateResource(this.workspaceId, item);
    this.workspaceService.expandResource(expand, this.workspaceId, item);
  }

  enablePlanApply(): boolean {
    if (this.workspaceRuns === undefined) {
      return true;
    }
    if (this.workspaceRuns.length === 0) {
      return true;
    }

    const alreadyHasOne = this.workspaceRuns.some((run) => {
      if (
        run.status === RunStatus.Queued ||
        run.status === RunStatus.Planning ||
        run.status === RunStatus.Planned ||
        run.status === RunStatus.Applying ||
        run.status === RunStatus.AppliedStateError ||
        run.status === RunStatus.FailedStateError
      ) {
        return true;
      } else {
        return false;
      }
    });
    return !alreadyHasOne;
  }

  hasPlan(run?: Run) {
    return this.hasStatus(RunStatus.Planned, run);
  }

  hasStateError(run?: Run) {
    return (
      this.hasStatus(RunStatus.AppliedStateError, run) ||
      this.hasStatus(RunStatus.FailedStateError, run)
    );
  }

  private hasStatus(status: RunStatus, run?: Run) {
    if (run) {
      return run.status === status ? true : false;
    } else {
      if (this.workspaceRuns && this.workspaceRuns.length > 0) {
        const result = this.workspaceRuns.filter(() => run.status === status);
        return result.length > 0 ? true : false;
      } else {
        return true;
      }
    }
  }

  filterRuns(filters: StatusFilter[]) {
    this.workspaceService.setStatusFilters(this.workspaceId, filters);
  }

  setRowStyle = (item: Run | Resource) => {
    if (!item) {
      return {};
    }
    if ('isDestroy' in item) {
      return item.isDestroy ? 'isDestroy' : '';
    }
    if ('tainted' in item) {
      return item.tainted ? 'isDestroy' : '';
    }
  };

  viewChangedFn(event) {
    if (event) {
      switch (event) {
        case 'runs':
          this.workspaceService
            .loadRunsByWorkspaceId(this.workspaceId)
            .pipe(take(1))
            .subscribe();
          break;
        case 'state':
          this.workspaceService
            .loadResourcesByWorkspaceId(this.workspaceId)
            .pipe(take(1))
            .subscribe();
          break;
      }
      this.workspaceService.setWorkspaceView(this.workspaceId, event);
    }
  }

  planOutput(output: string, item: Run) {
    this.workspaceService.planOutputUpdated(item.workspaceId, item.id, output);
  }

  applyOutput(output: string, item: Run) {
    this.workspaceService.applyOutputUpdated(item.workspaceId, item.id, output);
  }

  ngOnDestroy() {
    this.signalrService.leaveWorkspace(this.workspaceId);
  }
}
