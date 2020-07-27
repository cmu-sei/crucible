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
  ChangeDetectionStrategy,
  Component,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { ComnAuthService } from '@crucible/common';
import { User } from 'oidc-client';
import { Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import { ApplicationData } from '../../../models/application-data';
import { TeamData } from '../../../models/team-data';
import { ApplicationsService } from '../../../services/applications/applications.service';
import { FocusedAppService } from '../../../services/focused-app/focused-app.service';

@Component({
  selector: 'app-application-list',
  templateUrl: './application-list.component.html',
  styleUrls: ['./application-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApplicationListComponent implements OnInit, OnChanges, OnDestroy {
  @Input() viewId: string;
  @Input() user: User;
  @Input() teams: TeamData[];

  public applications$: Observable<ApplicationData[]>;
  public viewGUID: string;
  public titleText: string;
  private unsubscribe$: Subject<null> = new Subject<null>();

  constructor(
    private applicationsService: ApplicationsService,
    private focusedAppService: FocusedAppService,
    private authService: ComnAuthService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit() {
    this.refreshApps();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.teams) {
      this.refreshApps();
    }
  }

  // Local Component functions
  openInTab(url: string) {
    window.open(url, '_blank');
  }

  refreshApps() {
    this.applications$ = this.applicationsService
      .getApplicationsByTeam(
        this.teams.find((t) => t.isPrimary).id,
        this.viewId
      )
      .pipe(
        map((apps) => ({ apps })),
        map(({ apps }) => {
          apps.forEach(
            (app) =>
              (app.safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
                app.url
              ))
          );
          return apps;
        }),
        takeUntil(this.unsubscribe$)
      );
  }

  openInFocusedApp(name: string, url: string) {
    this.authService.isAuthenticated().then((isAuthenticated) => {
      if (!isAuthenticated) {
        console.log(
          'User is not authenticated and must not have been redirected to login.'
        );
        window.location.reload();
      } else {
        this.focusedAppService.focusedAppUrl.next(url);
      }
    });
  }

  ngOnDestroy() {
    this.unsubscribe$.next(null);
    this.unsubscribe$.complete();
  }
}
