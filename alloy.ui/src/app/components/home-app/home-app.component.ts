/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { ComnAuthService, ComnSettingsService } from '@crucible/common';
import { take } from 'rxjs/operators';
import { EventService } from 'src/app/generated/alloy.api';
import { LoggedInUserService } from '../../services/logged-in-user/logged-in-user.service';
import { TopbarView } from '../shared/top-bar/topbar.models';

@Component({
  selector: 'app-home-app',
  templateUrl: './home-app.component.html',
  styleUrls: ['./home-app.component.scss'],
})
export class HomeAppComponent implements OnInit {
  username: string;
  titleText: string;
  isSuperUser: Boolean;
  topBarColor = '#719F94';
  topBarTextColor = '#FFFFFF';
  eventTemplateId = '';
  TopbarView = TopbarView;

  constructor(
    private route: ActivatedRoute,
    private settingsService: ComnSettingsService,
    private titleService: Title,
    private eventService: EventService
  ) {}

  ngOnInit() {
    // Set the topbar color from config file
    this.topBarColor = this.settingsService.settings.AppTopBarHexColor ? this.settingsService.settings.AppTopBarHexColor : this.topBarColor;
    this.topBarTextColor = this.settingsService.settings.AppTopBarHexTextColor ? this.settingsService.settings.AppTopBarHexTextColor : this.topBarTextColor;

    // Set the page title from configuration file
    this.titleText = this.settingsService.settings.AppTopBarText;

    this.titleService.setTitle(this.settingsService.settings.AppTitle);
    this.username = '';

    // Get the event GUID from the URL that the user is entering the web page on
    this.route.params.subscribe((params) => {
      this.eventTemplateId = params['id'];

      if (!this.eventTemplateId) {
        // If there is no eventTemplateId, then check to see if a ViewId for player is present
        const viewId = params['viewId'];
        if (viewId) {
          console.log('ViewId:  ', viewId);
          this.eventService
            .getMyViewEvents(viewId)
            .pipe(take(1))
            .subscribe((imp) => {
              const index = imp.findIndex((i) => i.viewId === viewId);
              if (index > -1) {
                this.eventTemplateId = imp[0].eventTemplateId;
              }
            });
        }
      }
    });
  }

  isIframe(): boolean {
    if (window.location !== window.parent.location) {
      // The page is in an iframe
      return true;
    } else {
      // The page is not in an iframe
      return false;
    }
  }
}
