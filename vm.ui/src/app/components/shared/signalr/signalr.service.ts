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
import { VmModel } from '../../../vms/state/vm.model';
import { VmService } from '../../../vms/state/vms.service';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;
  private viewId: string;
  private connectionPromise: Promise<void>;

  constructor(
    private authService: ComnAuthService,
    private settingsService: ComnSettingsService,
    private vmService: VmService
  ) {}

  public startConnection(): Promise<void> {
    if (this.connectionPromise) {
      return this.connectionPromise;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(
        `${this.settingsService.settings.ApiUrl.replace('/api', '')}/hubs/vm`,
        {
          accessTokenFactory: () => {
            return this.authService.getAuthorizationToken();
          },
        }
      )
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
    if (this.viewId) {
      this.joinView(this.viewId);
    }
  }

  public joinView(viewId: string) {
    this.viewId = viewId;

    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('JoinView', viewId);
    }
  }

  public leaveView(viewId: string) {
    this.viewId = null;
    this.hubConnection.invoke('LeaveView', viewId);
  }

  private addHandlers() {
    this.addVmHandlers();
  }

  private addVmHandlers() {
    this.hubConnection.on(
      'VmUpdated',
      (vm: VmModel, modifiedProperties: string[]) => {
        let model: Partial<VmModel> = vm;
        if (modifiedProperties != null) {
          model = {};

          modifiedProperties.forEach((x) => {
            model[x] = vm[x];
          });
        }

        this.vmService.update(vm.id, model);
      }
    );

    this.hubConnection.on('VmCreated', (vm: VmModel) => {
      this.vmService.add(vm);
    });

    this.hubConnection.on('VmDeleted', (id: string) => {
      this.vmService.remove(id);
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
