/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import { ComnSettingsService } from '@crucible/common';
import { RouterQuery } from '@datorama/akita-ng-router-store';
import { Observable, of, Subject } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';
import { Section } from '../../models/section.model';
import { TopbarView } from '../shared/top-bar/topbar.models';

@Component({
  selector: 'app-admin-app',
  templateUrl: './admin-app.component.html',
  styleUrls: ['./admin-app.component.scss'],
})
export class AdminAppComponent implements OnInit, OnDestroy {
  @ViewChild('sidenav') sidenav: MatSidenav;
  public topbarColor = '#b00';
  public topbarTextColor = '#FFFFFF';
  public TopbarView = TopbarView;
  public queryParams: any = {
    section: Section.ADMIN_VIEWS,
  };
  Section = Section;
  unsubscribe$: Subject<null> = new Subject<null>();
  public section$: Observable<Section> = this.routerQuery.selectQueryParams(
    'section'
  );
  public title = '';

  constructor(
    private settingsService: ComnSettingsService,
    private router: Router,
    private routerQuery: RouterQuery
  ) {}

  /**
   * Initialization
   */
  ngOnInit() {
    this.routerQuery
      .selectQueryParams()
      .pipe(
        switchMap((params: any) => of(params)),
        takeUntil(this.unsubscribe$)
      )
      .subscribe((params: any) => {
        // Redirect if no query params
        const { section } = params;
        this.sectionChangedFn(this.queryParams['section']);
      });

    // Set the topbar color from config file
    this.topbarColor = this.settingsService.settings.AppTopBarHexColor;
    this.topbarTextColor = this.settingsService.settings.AppTopBarHexTextColor;
  }

  addParam(params: any): void {
    this.queryParams = { ...this.queryParams, ...params };
    this.router.navigate([], {
      queryParams: { ...this.queryParams },
      queryParamsHandling: 'merge',
    });
  }

  sectionChangedFn(section: Section) {
    this.addParam({ section });
    switch (section) {
      case Section.ADMIN_VIEWS:
        this.title = 'Views';
        break;
      case Section.ADMIN_USERS:
        this.title = 'Users';
        break;
      case Section.ADMIN_APP_TEMP:
        this.title = 'Application Templates';
        break;
      case Section.ADMIN_ROLE_PERM:
        this.title = 'Roles / Permissions';
        break;
    }
  }

  ngOnDestroy() {
    this.unsubscribe$.next(null);
    this.unsubscribe$.complete();
  }
}
