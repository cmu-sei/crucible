/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Injectable } from '@angular/core';
import { ComnAuthService, ComnSettingsService } from '@crucible/common';
import * as signalR from '@microsoft/signalr';
import { DirectoryService } from 'src/app/directories';
import { FileService } from 'src/app/files/state';
import {
  Directory,
  ModelFile,
  Run,
  Workspace,
} from 'src/app/generated/caster-api';
import { WorkspaceService } from 'src/app/workspace/state';
import { ProjectService } from '../../project/state';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;
  private projectId: string;
  private workspaceIds: string[] = [];
  private joinedWorkspacesAdmin = false;
  private connectionPromise: Promise<void>;

  constructor(
    private fileService: FileService,
    private projectService: ProjectService,
    private directoryService: DirectoryService,
    private workspaceService: WorkspaceService,
    private authService: ComnAuthService,
    private settingsService: ComnSettingsService
  ) {}

  public startConnection(): Promise<void> {
    if (this.connectionPromise) {
      return this.connectionPromise;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.settingsService.settings.ApiUrl}/hubs/project`, {
        accessTokenFactory: () => {
          return this.authService.getAuthorizationToken();
        },
      })
      .withAutomaticReconnect(new RetryPolicy(60, 0, 5))
      .build();

    this.hubConnection.onreconnected(() => {
      this.JoinGroups();
    });

    this.addHandlers();
    this.connectionPromise = this.hubConnection.start();
    this.connectionPromise.then((x) => this.JoinGroups());

    return this.connectionPromise;
  }

  private JoinGroups() {
    if (this.projectId) {
      this.joinProject(this.projectId);
    }

    if (this.workspaceIds) {
      this.workspaceIds.forEach((x) => this.joinWorkspace(x));
    }

    if (this.joinedWorkspacesAdmin) {
      this.joinWorkspacesAdmin();
    }
  }

  public joinProject(projectId: string) {
    this.projectId = projectId;

    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('JoinProject', projectId);
    }
  }

  public leaveProject(projectId: string) {
    this.projectId = null;
    this.hubConnection.invoke('LeaveProject', projectId);
  }

  public joinWorkspace(workspaceId: string) {
    this.workspaceIds.push(workspaceId);

    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('JoinWorkspace', workspaceId);
    }
  }

  public leaveWorkspace(workspaceId: string) {
    this.workspaceIds = this.workspaceIds.filter((x) => x !== workspaceId);
    this.hubConnection.invoke('LeaveWorkspace', workspaceId);
  }

  public joinWorkspacesAdmin() {
    this.joinedWorkspacesAdmin = true;

    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('JoinWorkspacesAdmin');
    }
  }

  public leaveWorkspacesAdmin() {
    this.joinedWorkspacesAdmin = false;
    this.hubConnection.invoke('LeaveWorkspacesAdmin');
  }

  public streamPlanOutput(planId: string) {
    return this.hubConnection.stream('GetPlanOutput', planId);
  }

  public streamApplyOutput(applyId: string) {
    return this.hubConnection.stream('GetApplyOutput', applyId);
  }

  private addHandlers() {
    this.addFileHandlers();
    this.addDirectoryHandlers();
    this.addWorkspaceHandlers();
    this.addRunHandlers();
  }

  private addFileHandlers() {
    this.hubConnection.on('FileUpdated', (file: ModelFile) => {
      this.fileService.fileUpdated(file);
    });

    this.hubConnection.on('FileDeleted', (fileId: string) => {
      this.projectService.closeTab(fileId);
      this.fileService.fileDeleted(fileId);
    });
  }

  private addDirectoryHandlers() {
    this.hubConnection.on('DirectoryUpdated', (directory: Directory) => {
      this.directoryService.updated(directory);
    });

    this.hubConnection.on('DirectoryDeleted', (dirId: string) => {
      this.directoryService.deleted(dirId);
    });
  }

  private addWorkspaceHandlers() {
    this.hubConnection.on('WorkspaceUpdated', (workspace: Workspace) => {
      this.workspaceService.updated(workspace);
    });

    this.hubConnection.on('WorkspaceDeleted', (workspaceId: string) => {
      this.workspaceService.deleted(workspaceId);
    });

    this.hubConnection.on(
      'WorkspaceSettingsUpdated',
      (lockingEnabled: boolean) => {
        this.workspaceService.lockingEnabledUpdated(lockingEnabled);
      }
    );
  }

  private addRunHandlers() {
    this.hubConnection.on('RunUpdated', (run: Run) => {
      this.workspaceService.runUpdated(run);
    });
  }
}

class RetryPolicy {
  constructor(
    private maxSeconds: number,
    private minJitterSeconds: number,
    private maxJitterSeconds: number
  ) {}

  nextRetryDelayInMilliseconds(
    retryContext: signalR.RetryContext
  ): number | null {
    let nextRetrySeconds = Math.pow(2, retryContext.previousRetryCount + 1);

    if (nextRetrySeconds > this.maxSeconds) {
      nextRetrySeconds = this.maxSeconds;
    }

    nextRetrySeconds +=
      Math.floor(
        Math.random() * (this.maxJitterSeconds - this.minJitterSeconds + 1)
      ) + this.minJitterSeconds; // Add Jitter

    return nextRetrySeconds * 1000;
  }
}
