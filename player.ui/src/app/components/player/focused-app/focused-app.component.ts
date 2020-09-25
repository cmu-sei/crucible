/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnDestroy } from '@angular/core';
import { FocusedAppService } from '../../../services/focused-app/focused-app.service';
import { SafeUrl, DomSanitizer } from '@angular/platform-browser';
import { combineLatest, Observable, Subject } from 'rxjs';
import { ComnAuthQuery, Theme } from '@crucible/common';
import { map, shareReplay, tap } from 'rxjs/operators';

@Component({
  selector: 'app-focused-app',
  templateUrl: './focused-app.component.html',
  styleUrls: ['./focused-app.component.scss'],
})
export class FocusedAppComponent implements OnDestroy {
  public focusedAppUrl$: Observable<SafeUrl>;
  public theme$: Observable<Theme>;
  private unsubscribe$ = new Subject<null>();

  constructor(
    private focusedAppService: FocusedAppService,
    private sanitizer: DomSanitizer,
    private authQuery: ComnAuthQuery
  ) {

    this.focusedAppUrl$ = this.focusedAppService.focusedAppUrl.pipe(
      map((url) =>
        this.sanitizer.bypassSecurityTrustResourceUrl(url)
      )
    );

    this.focusedAppUrl$ = combineLatest([this.focusedAppService.focusedAppUrl, this.authQuery.userTheme$]).pipe(
      map(([url, theme]) => {
        let themedUrl = url;
        const themeIndex = url.indexOf('?theme=');
        if (themeIndex >= 0) {
          // Only add the theme query param if it already exists
          themedUrl = url.substring(0, themeIndex) + '?theme=' + theme;
        }
        return this.sanitizer.bypassSecurityTrustResourceUrl(themedUrl);
      }),
      shareReplay(1)
    );

  }

  ngOnDestroy() {
    this.unsubscribe$.next(null);
    this.unsubscribe$.complete();
  }

}
