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
  OnInit,
  ChangeDetectionStrategy,
  OnDestroy,
} from '@angular/core';
import { WorkspaceService, WorkspaceQuery } from 'src/app/workspace/state';
import { Observable } from 'rxjs';
import { Run } from 'src/app/generated/caster-api';
import { SignalRService } from 'src/app/shared/signalr/signalr.service';

@Component({
  selector: 'cas-admin-workspaces',
  templateUrl: './admin-workspaces.component.html',
  styleUrls: ['./admin-workspaces.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminWorkspacesComponent implements OnInit, OnDestroy {
  public lockingEnabled$: Observable<boolean>;
  public activeRuns$: Observable<Run[]>;
  public expandedRuns$: Observable<string[]>;

  constructor(
    private workspaceService: WorkspaceService,
    private workspaceQuery: WorkspaceQuery,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    this.signalRService
      .startConnection()
      .then(() => {
        this.signalRService.joinWorkspacesAdmin();
      })
      .catch((err) => {
        console.log(err);
      });

    this.workspaceService.loadLockingStatus();
    this.lockingEnabled$ = this.workspaceQuery.select(
      (state) => state.lockingEnabled
    );

    this.workspaceService.loadAllActiveRuns();
    this.activeRuns$ = this.workspaceQuery.activeRuns$();
    this.expandedRuns$ = this.workspaceQuery.expandedRuns$();
  }

  setLockingEnabled(status: boolean) {
    this.workspaceService.setLockingEnabled(status);
  }

  expandRun(event: { expand: boolean; item: Run }) {
    this.workspaceService.expandRun(event.expand, event.item);
  }

  planOutput(event: { output: string; item: Run }) {
    this.workspaceService.planOutputUpdated(
      event.item.workspaceId,
      event.item.id,
      event.output
    );
  }

  applyOutput(event: { output: string; item: Run }) {
    this.workspaceService.applyOutputUpdated(
      event.item.workspaceId,
      event.item.id,
      event.output
    );
  }

  ngOnDestroy() {
    this.signalRService.leaveWorkspacesAdmin();
  }
}
