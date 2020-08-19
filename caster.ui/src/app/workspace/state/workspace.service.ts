/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { WorkspaceStore } from './workspace.store';
import { Injectable } from '@angular/core';
import { StatusFilter } from './workspace.model';
import {
  AppliesService,
  PlansService,
  Resource,
  ResourcesService,
  Run,
  RunsService,
  Workspace,
  WorkspacesService,
  RunStatus,
} from '../../generated/caster-api';
import { concatMap, take, tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import {
  arrayAdd,
  arrayRemove,
  arrayUpsert,
  coerceArray,
} from '@datorama/akita';
import { WorkspaceQuery } from './workspace.query';
import { FileService } from 'src/app/files/state';

@Injectable({
  providedIn: 'root',
})
export class WorkspaceService {
  constructor(
    private workspaceStore: WorkspaceStore,
    private workspacesService: WorkspacesService,
    private workspaceQuery: WorkspaceQuery,
    private fileService: FileService,
    private runsService: RunsService,
    private appliesService: AppliesService,
    private plansService: PlansService,
    private resourceService: ResourcesService
  ) {}

  getWorkspace(workspaceId: string) {
    this.workspacesService.getWorkspace(workspaceId).subscribe((workspace) => {
      let ws = this.workspaceQuery.getEntity(workspace.id);
      if (ws) {
        ws = { ...ws, name: ws.name };
        this.workspaceStore.update(ws.id, ws);
      } else {
        const newWorkspace = {
          ...workspace,
          runs: new Array<Run>(),
        } as Workspace;
        this.workspaceStore.add(newWorkspace);
      }
    });
  }

  setWorkspaces(workspaces: Workspace[]) {
    const workspaceUIs = this.workspaceQuery.ui.getAll();
    this.workspaceStore.set(workspaces);
    workspaces.forEach((w) => {
      const workspaceUI = workspaceUIs.find((wUI) => wUI.id === w.id);
      if (workspaceUI) {
        this.workspaceStore.ui.upsert(workspaceUI.id, workspaceUI);
      }
    });
  }

  add(workspace: Workspace) {
    this.workspacesService.createWorkspace(workspace).subscribe((w) => {
      this.workspaceStore.add(w);
      this.fileService
        .loadFilesByDirectory(w.directoryId)
        .pipe(take(1))
        .subscribe();
    });
  }

  update(workspace: Workspace) {
    this.workspacesService
      .partialEditWorkspace(workspace.id, { ...workspace } as Workspace)
      .subscribe((w) => {
        this.workspaceStore.update(w.id, w);
      });
  }

  partialUpdate(id: string, workspace: Partial<Workspace>) {
    this.workspacesService
      .partialEditWorkspace(id, { ...workspace } as Workspace)
      .subscribe((w) => {
        this.workspaceStore.update(w.id, w);
      });
  }

  updated(workspace: Workspace) {
    this.workspaceStore.upsert(workspace.id, workspace);
  }

  delete(workspace: Workspace) {
    this.workspacesService.deleteWorkspace(workspace.id).subscribe(() => {
      this.deleted(workspace.id);
    });
  }

  deleted(workspaceId: string) {
    this.workspaceStore.remove(workspaceId);
  }

  runUpdated(run: Run) {
    const workspace: Workspace = { id: run.workspaceId, runs: [] };
    this.workspaceStore.add(workspace);

    this.workspaceStore.update(run.workspaceId, (entity) => ({
      runs: arrayUpsert(entity.runs, run.id, { ...run }),
    }));
  }

  planOutputUpdated(workspaceId: string, runId: string, output: string) {
    this.workspaceStore.update(workspaceId, (entity) => ({
      runs: arrayUpsert(entity.runs, runId, { plan: { output } }),
    }));
  }

  applyOutputUpdated(workspaceId: string, runId: string, output: string) {
    this.workspaceStore.update(workspaceId, (entity) => ({
      runs: arrayUpsert(entity.runs, runId, { apply: { output } }),
    }));
  }

  taint(workspaceId: string, items: Resource | Resource[]) {
    items = coerceArray(items);
    const resourceAddresses = items.map((i) => i.address);
    items.forEach((i) => this.resourceAction(workspaceId, i.id));
    return this.resourceService
      .taintResources(workspaceId, { resourceAddresses })
      .pipe(
        tap((resources: Resource[]) => {
          this.workspaceStore.upsert(workspaceId, (entity) => ({
            resources,
          }));
        }),
        tap((resources) =>
          (items as Resource[]).forEach((i) =>
            this.resourceAction(workspaceId, i.id)
          )
        )
      );
  }

  untaint(workspaceId: string, items: Resource | Resource[]) {
    items = coerceArray(items);
    const resourceAddresses = items.map((i) => i.address);
    items.forEach((i) => this.resourceAction(workspaceId, i.id));
    return this.resourceService
      .untaintResources(workspaceId, { resourceAddresses })
      .pipe(
        tap((resources: Resource[]) => {
          this.workspaceStore.upsert(workspaceId, (entity) => ({
            resources,
          }));
        }),
        tap((resources) =>
          (items as Resource[]).forEach((i) =>
            this.resourceAction(workspaceId, i.id)
          )
        )
      );
  }

  setActive(workspace: Workspace | null) {
    // if id is null or undefined, a file or folder is active
    if (!workspace) {
      this.workspaceStore.setActive(null);
      return;
    }
    this.workspaceStore.setActive(workspace.id);
  }

  setStatusFilters(workspaceId: string, statusFilters?: StatusFilter[]) {
    if (statusFilters && statusFilters.length > 0) {
      this.workspaceStore.ui.update(workspaceId, (entity) => ({
        statusFilter: statusFilters,
      }));
    } else {
      // Filters are saved in persistent storage if they exist we will load them instead of the defaults
      const persistedStatusFilters = this.workspaceQuery.ui.getValue()
        .statusFilter;

      if (persistedStatusFilters && persistedStatusFilters.length > 0) {
        this.workspaceStore.ui.update(workspaceId, (state) => ({
          statusFilter: persistedStatusFilters,
        }));
      } else {
        const defaultStatusFilters = Object.keys(RunStatus).map((o) => ({
          key: o,
          filter: false,
        }));
        this.workspaceStore.ui.update(workspaceId, (state) => ({
          statusFilter: defaultStatusFilters,
        }));
      }
    }
  }

  createPlanRun(workspaceId: string, isDestroy: boolean) {
    return this.workspaceQuery
      .selectEntity(workspaceId, (entity) => entity.id)
      .pipe(
        concatMap((_id: string) =>
          this.runsService.createRun({ workspaceId: _id, isDestroy })
        )
      );
  }

  applyRun(workspaceId: string, id: string) {
    return this.appliesService.applyRun(id);
  }

  rejectRun(workspaceId, runId: string) {
    return this.runsService.rejectRun(runId);
  }

  saveState(runId: string) {
    this.workspaceStore.setLoading(true);
    return this.runsService
      .saveState(runId)
      .pipe(tap((run) => this.workspaceStore.setLoading(false)));
  }

  selectRun(workspaceId, runId) {
    this._setSelectedRun(workspaceId, runId);
  }

  private _setSelectedRun(workspaceId: string, runId: string) {
    this.workspaceStore.ui.update(workspaceId, (entity) => ({
      selectedRuns: [runId],
    }));
  }

  loadRunsByWorkspaceId(id: string): Observable<any> {
    return of(id).pipe(
      tap(() => {
        this.workspaceStore.setLoading(true);
      }),
      concatMap((_id) =>
        this.runsService.getRunsByWorkspaceId(_id, null, false, false)
      ),
      tap((runs) => {
        this.workspaceStore.update(id, (entity) => ({
          runs,
        }));
      }),
      tap(() => {
        this.workspaceStore.setLoading(false);
      })
    );
  }

  loadAllActiveRuns(): void {
    this.workspaceStore.setLoading(true);

    this.runsService
      .getRuns(true)
      .pipe(
        tap((runs) => {
          const uniqueWorkspaceids = runs
            .map((r) => r.workspaceId)
            .filter((v, i, a) => a.indexOf(v) === i);

          uniqueWorkspaceids.forEach((x) => {
            const workspace: Workspace = { id: x, runs: [] };
            this.workspaceStore.add(workspace);
          });

          runs.forEach((r) => this.runUpdated(r));
          this.workspaceStore.setLoading(false);
        }),
        take(1)
      )
      .subscribe();
  }

  loadResourcesByWorkspaceId(id: string): Observable<any> {
    return of(id).pipe(
      tap(() => {
        this.workspaceStore.setLoading(true);
      }),
      concatMap((_id) => this.resourceService.getResourcesByWorkspace(_id)),
      tap((resources) => {
        this.workspaceStore.update(id, (entity) => ({
          resources,
        }));
      }),
      tap(() => {
        this.workspaceStore.setLoading(false);
      })
    );
  }

  updateResource(workspaceId: string, item: Resource): void {
    this.resourceService
      .getResource(workspaceId, item.id, item.type)
      .pipe(
        tap((resource) =>
          this.workspaceStore.update(workspaceId, (entity) => ({
            resources: arrayUpsert(entity.resources, item.id, resource),
          }))
        ),
        take(1)
      )
      .subscribe();
  }

  refreshResources(workspaceId: string) {
    this.workspaceStore.setLoading(true);
    return this.resourceService.refreshResources(workspaceId).pipe(
      tap((resources) =>
        this.workspaceStore.update(workspaceId, (entity) => ({
          resources,
        }))
      ),
      tap(() => this.workspaceStore.setLoading(false))
    );
  }

  expandRun(expand, run) {
    this.workspaceStore.ui.upsert(run.workspaceId, (entity) => {
      if (!entity.expandedRuns) {
        return;
      }
      const exists = entity.expandedRuns.includes(run.id);
      if (!exists && expand) {
        return { expandedRuns: arrayAdd(entity.expandedRuns, run.id) };
      }
      if (exists && !expand) {
        return { expandedRuns: arrayRemove(entity.expandedRuns, run.id) };
      }

      return { expandedRuns: entity.expandedRuns };
    });
  }

  resourceAction(workspaceId, resourceId) {
    this.workspaceStore.ui.upsert(workspaceId, (entity) => {
      if (!entity.resourceActions) {
        return;
      }
      const exists = entity.resourceActions.includes(resourceId);
      if (!exists) {
        return {
          resourceActions: arrayAdd(entity.resourceActions, resourceId),
        };
      } else {
        return {
          resourceActions: arrayRemove(entity.resourceActions, resourceId),
        };
      }
    });
  }

  expandResource(expand, workspaceId, resource) {
    this.workspaceStore.ui.upsert(workspaceId, (entity) => {
      if (!entity.expandedResources) {
        return;
      }
      const exists = entity.expandedResources.includes(resource.id);
      if (!exists && expand) {
        return {
          expandedResources: arrayAdd(entity.expandedResources, resource.id),
        };
      }
      if (exists && !expand) {
        return {
          expandedResources: arrayRemove(entity.expandedResources, resource.id),
        };
      }
    });
  }

  toggleIsExpanded(workspaceId: string) {
    this.workspaceStore.ui.upsert(workspaceId, (w) => ({
      isExpanded: !w.isExpanded,
    }));
  }

  isExpanded(workspaceId: string) {
    return this.workspaceQuery.ui.getEntity(workspaceId).isExpanded;
  }

  setWorkspaceView(id: string, view: string) {
    this.workspaceStore.ui.upsert(id, { workspaceView: view });
  }

  loadLockingStatus() {
    this.workspacesService
      .getWorkspaceLockingStatus()
      .pipe(take(1))
      .subscribe((lockingStatus) => {
        this.workspaceStore.update({ lockingEnabled: lockingStatus });
      });
  }

  setLockingEnabled(status: boolean) {
    let result: Observable<boolean>;

    if (status) {
      result = this.workspacesService.enableWorkspaceLocking();
    } else {
      result = this.workspacesService.disableWorkspaceLocking();
    }

    result.pipe(take(1)).subscribe((lockingStatus) => {
      this.lockingEnabledUpdated(lockingStatus);
    });
  }

  lockingEnabledUpdated(status: boolean) {
    this.workspaceStore.update({ lockingEnabled: status });
  }
}
