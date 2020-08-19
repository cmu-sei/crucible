/**
 * Crucible
 * Copyright 2020 Carnegie Mellon University.
 * NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
 * Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
 * [DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
 * Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
 * DM20-0181
 */

import {
  Component,
  OnInit,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { WorkspaceService, WorkspaceQuery } from '../../state';
import {
  TerraformService,
  TerraformVersionsResult,
  Workspace,
} from 'src/app/generated/caster-api';
import { Observable } from 'rxjs';

@Component({
  selector: 'cas-workspace-edit-container',
  templateUrl: './workspace-edit-container.component.html',
  styleUrls: ['./workspace-edit-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkspaceEditContainerComponent implements OnInit {
  @Input() id: string;
  @Output() editWorkspaceComplete = new EventEmitter<boolean>();

  public workspace$: Observable<Workspace>;
  public versionResult$: Observable<TerraformVersionsResult>;

  constructor(
    private workspaceService: WorkspaceService,
    private workspaceQuery: WorkspaceQuery,
    private terraformService: TerraformService
  ) {}

  ngOnInit(): void {
    this.workspace$ = this.workspaceQuery.selectEntity(this.id);
    this.versionResult$ = this.terraformService.getTerraformVersions();
  }

  onUpdateWorkspace(workspace: Partial<Workspace>) {
    if (workspace != null) {
      this.workspaceService.partialUpdate(this.id, workspace);
    }
    this.editWorkspaceComplete.emit(true);
  }
}
