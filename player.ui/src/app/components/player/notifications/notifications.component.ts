/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, Input, OnInit } from '@angular/core';
import { ComnSettingsService } from '@crucible/common';
import { PushNotificationsService } from 'ng-push-ivy';
import { NotificationData } from '../../../models/notification-data';
import { DialogService } from '../../../services/dialog/dialog.service';
import { NotificationService } from '../../../services/notification/notification.service';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss'],
})
export class NotificationsComponent implements OnInit {
  @Input() viewGuid: string;
  @Input() teamGuid: string;
  @Input() userGuid: string;
  @Input() userToken: string;
  @Input() userName: string;

  public notification: NotificationData;
  public messageToSend: string;
  public userData: any;
  public showSystemNotifications: boolean;
  public showSendMessage: boolean;
  public sendMessagePlaceholder: string;
  public notificationsList: Array<NotificationData>;
  public notificationsHistory: Array<NotificationData>;
  public startIndex: number;
  public endIndex: number;
  public showUp: boolean;
  public showDown: boolean;

  public constructor(
    private notificationService: NotificationService,
    private pushNotificationService: PushNotificationsService,
    private settingsService: ComnSettingsService,
    private dialogService: DialogService
  ) {}

  ngOnInit() {
    this.showSystemNotifications = true;
    this.notification = undefined;
    this.messageToSend = '';
    this.showUp = false;
    this.showDown = false;
    this.startIndex = 0;
    this.endIndex = 0;
    this.notificationsHistory = new Array<NotificationData>();
    this.showSendMessage = false;
    this.sendMessagePlaceholder = 'Send system wide notification';

    this.notificationService.canSendMessage.subscribe((data) => {
      this.showSendMessage = data;
    });

    this.notificationService.notificationHistory.subscribe((data) => {
      if (data != undefined && data.length > 0) {
        this.notificationsHistory = this.notificationsHistory.concat(
          <Array<NotificationData>>data
        );
        this.notificationsHistory = this.notificationsHistory.sort(
          (a: NotificationData, b: NotificationData) => {
            if (a.broadcastTime < b.broadcastTime) {
              return -1;
            } else if (a.broadcastTime > b.broadcastTime) {
              return 1;
            } else {
              return 0;
            }
          }
        );
        this.showNotificationPage(0);
      }
    });

    this.notificationService.viewNotification.subscribe((msg) => {
      // Check to see if a valid notification came across.
      if (msg.broadcastTime != undefined) {
        this.notification = msg;
        this.notificationsHistory.push(this.notification);
        this.showNotificationPage(0);

        if (this.pushNotificationService.permission == 'granted') {
          this.pushNotificationService
            .create(this.notification.subject, {
              icon: this.notification.iconUrl,
              body: this.notification.text,
            })
            .subscribe(
              (res) => {
                if (
                  this.notification.link != null &&
                  this.notification.link != ''
                ) {
                  if (res.event.type === 'click') {
                    this.openLink(this.notification.link);
                  }
                }
                console.log(res);
              },
              (err) => console.log(err)
            );
        } else {
          console.log('Notifications have not been granted by user.');
          this.pushNotificationService.requestPermission();
        }
      }
    });

    this.notificationService.connectToNotificationServer(
      this.viewGuid,
      this.teamGuid,
      this.userGuid,
      this.userToken
    );
  }

  public openLink(link: string) {
    window.open(link, '_blank');
  }

  public sendMessage(): void {
    if (this.messageToSend.trim().length > 0) {
      this.dialogService
        .confirm(
          'Confirm Message Send',
          'Are you sure that you want to send a system wide message to all users logged into this view?',
          null
        )
        .subscribe((result) => {
          if (result['confirm'] == true) {
            if (this.messageToSend.trim().length > 225) {
              // Trim after 225 characters
              this.messageToSend = this.messageToSend.trim().substring(0, 225);
            }
            this.notificationService.sendNotification(
              this.viewGuid,
              this.messageToSend
            );
            this.messageToSend = '';
          }
        });
    }
  }

  public showNotificationPage(direction: number) {
    // Determine indexes to show
    let maxDisplay = this.settingsService.settings.NotificationsSettings
      .number_to_display;
    if (maxDisplay > this.notificationsHistory.length) {
      maxDisplay = this.notificationsHistory.length;
    }

    if (direction == 0) {
      this.startIndex = this.notificationsHistory.length - maxDisplay;
      this.endIndex = this.notificationsHistory.length;
    } else if (direction == 1) {
      this.startIndex -= maxDisplay;
      this.endIndex -= maxDisplay;
      while (this.startIndex < 0) {
        this.startIndex++;
        if (this.endIndex < this.notificationsHistory.length) {
          this.endIndex++;
        }
      }
    } else if (direction == -1) {
      this.startIndex += maxDisplay;
      this.endIndex += maxDisplay;
      while (this.endIndex > this.notificationsHistory.length) {
        this.endIndex--;
        if (this.startIndex > 0) {
          this.startIndex--;
        }
      }
    }

    this.showUp = this.startIndex > 0;
    this.showDown = this.endIndex < this.notificationsHistory.length;
  }

  public onSubmit() {
    const message = this.userData;
    message.message = this.sendMessage;
  }
}
