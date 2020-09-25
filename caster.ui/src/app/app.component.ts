/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { OverlayContainer } from '@angular/cdk/overlay';
import { Component, HostBinding, OnDestroy } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer, Title } from '@angular/platform-browser';
import { ComnSettingsService, Theme } from '@crucible/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CurrentUserQuery } from './users/state';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnDestroy {
  @HostBinding('class') componentCssClass: string;
  title = 'Caster';
  unsubscribe$: Subject<null> = new Subject<null>();
  constructor(
    public matIconRegistry: MatIconRegistry,
    public overlayContainer: OverlayContainer,
    private currentUserQuery: CurrentUserQuery,
    public sanitizer: DomSanitizer,
    public titleService: Title,
    public settingsService: ComnSettingsService
  ) {
    this.currentUserQuery.userTheme$
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((theme) => {
        this.setTheme(theme);
      });

    titleService.setTitle(settingsService.settings.AppTopBarText);

    matIconRegistry.setDefaultFontSetClass('mdi');

    matIconRegistry.addSvgIcon(
      'ic_apps_white_24px',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_apps_white_24px.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_chevron_left_white_24px',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_chevron_left_white_24px.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_chevron_right_black_24px',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_chevron_right_black_24px.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_chevron_right_white_24px',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_chevron_right_white_24px.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_expand_more_white_24px',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_expand_more_white_24px.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_clear_black_24px',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_clear_black_24px.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_expand_more_black_24px',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_expand_more_black_24px.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_cancel_circle',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_cancel_circle.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_back_arrow',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_back_arrow_24px.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_magnify_search',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_magnify_glass_48px.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_clipboard_copy',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_clipboard_copy.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_trash_can',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_trash_can.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_pencil',
      sanitizer.bypassSecurityTrustResourceUrl(
        '/assets/svg-icons/ic_pencil.svg'
      )
    );
    matIconRegistry.addSvgIcon(
      'ic_crucible_caster',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_crucible_caster.svg'
      )
    );
  }

  setTheme(theme: Theme) {
    const classList = this.overlayContainer.getContainerElement().classList;
    switch (theme) {
      case Theme.LIGHT:
        this.componentCssClass = theme;
        classList.add(theme);
        classList.remove(Theme.DARK);
        break;
      case Theme.DARK:
        this.componentCssClass = theme;
        classList.add(theme);
        classList.remove(Theme.LIGHT);
    }
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next(null);
    this.unsubscribe$.complete();
  }
}
