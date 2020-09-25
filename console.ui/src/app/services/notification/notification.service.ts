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
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { ComnSettingsService } from '@crucible/common';
import { BehaviorSubject } from 'rxjs';
import { NotificationData } from '../../models/notification/notification-model';

@Injectable()
export class NotificationService {
  public progressConnection: HubConnection;
  public tasksInProgress = new BehaviorSubject<Array<NotificationData>>(
    new Array<NotificationData>()
  );

  constructor(private settings: ComnSettingsService) {}

  connectToProgressHub(vmString: string, userToken: string) {
    console.log('Starting connection to ProgressHub');
    this.progressConnection = new HubConnectionBuilder()
      .withUrl(
        `${this.settings.settings.ConsoleApiUrl.replace(
          '/api/',
          '/hubs/'
        )}progress?access_token=${userToken}`
      )
      .build();

    this.progressConnection.on('Progress', (data: [NotificationData]) => {
      this.tasksInProgress.next(data);
    });

    this.progressConnection.on('Complete', (data: [NotificationData]) => {
      this.tasksInProgress.next(data);
    });

    this.progressConnection
      .start()
      .then(() => {
        this.progressConnection.invoke('Join', vmString);
        console.log('Progress connection started');
      })
      .catch(() => {
        console.log(
          'Error while establishing Progress connection with the VM Console API.'
        );
        throw new Error(
          'Error while establishing Progress connection with the VM Console API.'
        );
      });
  }
}
