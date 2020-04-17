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
import { Observable, BehaviorSubject } from 'rxjs';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { SettingsService } from '../settings/settings.service';
import { NotificationData } from '../../models/notification-data';



@Injectable()
export class NotificationService {

  public exerciseNotification = new BehaviorSubject<NotificationData>(<NotificationData>{});
  public notificationHistory = new BehaviorSubject<Array<NotificationData>>(new Array<NotificationData>());
  public canSendMessage = new BehaviorSubject<boolean>(false);
  public exerciseConnection: HubConnection;
  public teamConnection: HubConnection;
  public userConnection: HubConnection;


  constructor(
    private settings: SettingsService
  ) { }


  connectToNotificationServer(exerciseGuid: string, teamGuid: string, userGuid: string, userToken: string) {
    this.exerciseConnection = new HubConnectionBuilder()
      .withUrl(`${this.settings.NotificationsSettings.url}/exercise?bearer=${userToken}`)
      .build();
    this.teamConnection = new HubConnectionBuilder()
      .withUrl(`${this.settings.NotificationsSettings.url}/team?bearer=${userToken}`)
      .build();
    this.userConnection = new HubConnectionBuilder()
      .withUrl(`${this.settings.NotificationsSettings.url}/user?bearer=${userToken}`)
      .build();

    this.exerciseConnection.on('Reply', (data: NotificationData) => {
      let validatedData = this.validateNotificationData(data);
      if (validatedData != null) {
        this.exerciseNotification.next(validatedData);
      }
    });

    this.exerciseConnection.on('History', (data: [NotificationData]) => {
      this.notificationHistory.next(data)
    });

    this.teamConnection.on('Reply', (data: NotificationData) => {
      let validatedData = this.validateNotificationData(data);
      if (validatedData != null) {
        this.exerciseNotification.next(validatedData);
      }
    });

    this.teamConnection.on('History', (data: [NotificationData]) => {
      this.notificationHistory.next(data)
    });

    this.userConnection.on('Reply', (data: NotificationData) => {
      let validatedData = this.validateNotificationData(data);
      if (validatedData != null) {
        this.exerciseNotification.next(validatedData);
      }
    });

    this.userConnection.on('History', (data: [NotificationData]) => {
      this.notificationHistory.next(data)
    });

    this.exerciseConnection.start()
      .then(() => {
        this.exerciseConnection.invoke('Join', exerciseGuid);
        this.exerciseConnection.invoke('GetHistory', exerciseGuid);
        console.log('Exercise connection started');
      })
      .catch(() => {
        console.log('Error while establishing Exercise connection');
      });

    this.teamConnection.start()
      .then(() => {
        this.teamConnection.invoke('Join', teamGuid);
        this.teamConnection.invoke('GetHistory', teamGuid);
        console.log('Team connection started');
      })
      .catch(() => {
        console.log('Error while establishing Team connection');
      });

    this.userConnection.start()
      .then(() => {
        this.userConnection.invoke('Join', exerciseGuid, userGuid);
        this.userConnection.invoke('GetHistory', exerciseGuid, userGuid);
        console.log('User connection started');
      })
      .catch(() => {
        console.log('Error while establishing User connection');
      });

  }

  sendNotification(guid: string, msg: string) {
    console.log("Sending Notification  " + msg);

    this.exerciseConnection.invoke('Post', guid, msg);
  }


  validateNotificationData(data: NotificationData): NotificationData {
    if (data.priority == 'System') {
      this.canSendMessage.next(data.canPost);
      return null;  // Indicates that a System message was broadcast and should be ignored by user
    }

    if (data.subject == undefined) {
      data.subject = "Player Notification"
    }
    if (data.iconUrl == undefined) {
      data.iconUrl = 'assets/img/SP_Icon_Alert.png'
    }

    if (data.broadcastTime == undefined) {
      return null;
    }

    return data;
  }

}

