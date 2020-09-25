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
import { ComnAuthQuery, Theme, ComnSettingsService } from '@crucible/common';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnDestroy {
  @HostBinding('class') componentCssClass: string;
  theme$: Observable<Theme> = this.authQuery.userTheme$;
  unsubscribe$: Subject<null> = new Subject<null>();

  constructor(
    private iconRegistry: MatIconRegistry,
    private sanitizer: DomSanitizer,
    private overlayContainer: OverlayContainer,
    private authQuery: ComnAuthQuery,
    private settingsService: ComnSettingsService,
    private titleService: Title
  ) {
    this.theme$.pipe(takeUntil(this.unsubscribe$)).subscribe((theme) => {
      this.setTheme(theme);
    });
    this.registerIcons();

    // Set the Title for when the VM app is in it's own browser tab.
    titleService.setTitle(settingsService.settings.AppTitle);
  }

  registerIcons() {
    this.iconRegistry.addSvgIcon(
      'ic_apps_white_24px',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_apps_white_24px.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_chevron_left_white_24px',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_chevron_left_white_24px.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_chevron_right_white_24px',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_chevron_right_white_24px.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_chevron_right_black_24px',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_chevron_right_black_24px.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_expand_more_white_24px',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_expand_more_white_24px.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_clear_black_24px',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_clear_black_24px.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_expand_more_black_24px',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_expand_more_black_24px.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_cancel_circle',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_cancel_circle.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_back_arrow',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_back_arrow_24px.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_magnify_search',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_magnify_glass_48px.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_clipboard_copy',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_clipboard_copy.svg'
      )
    );
    this.iconRegistry.addSvgIcon(
      'ic_crucible_player',
      this.sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_crucible_player.svg'
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

  ngOnDestroy() {
    this.unsubscribe$.next(null);
    this.unsubscribe$.complete();
  }
}
