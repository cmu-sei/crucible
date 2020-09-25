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
import { DomSanitizer } from '@angular/platform-browser';
import { ComnAuthQuery, Theme } from '@crucible/common';
import { Subject } from 'rxjs';
import { Observable } from 'rxjs/Observable';
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
    iconRegistry: MatIconRegistry,
    sanitizer: DomSanitizer,
    private overlayContainer: OverlayContainer,
    private authQuery: ComnAuthQuery
  ) {
    iconRegistry.setDefaultFontSetClass('mdi');

    iconRegistry.addSvgIcon(
      'monitor',
      sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/monitor.svg')
    );
    iconRegistry.addSvgIcon(
      'ic_clear',
      sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_clear.svg')
    );
    iconRegistry.addSvgIcon(
      'monkey_wrench',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/monkey_wrench.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_cancel_circle',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_cancel_circle.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_back_arrow',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_back_arrow.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_magnify_search',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_magnify_glass.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_chevron_down',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_chevron_down.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_chevron_right',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_chevron_right.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_square_edit_outline_48px',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_square_edit_outline_48px.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_plus_circle_outline',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_plus_circle_outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_trash_can',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_trash_can.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_expand_more',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_expand_more.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'build_24px',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/build-24px.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'history',
      sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/history.svg')
    );
    iconRegistry.addSvgIcon(
      'menu',
      sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/menu.svg')
    );
    iconRegistry.addSvgIcon(
      'playlist_play',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/playlist_play.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'input',
      sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/input.svg')
    );
    iconRegistry.addSvgIcon(
      'play_circle_outline',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/play_circle_outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'open_in_new',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/open-in-new.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'content_paste',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/content_paste.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_clipboard_copy',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_clipboard_copy.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'storage',
      sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/storage.svg')
    );
    iconRegistry.addSvgIcon(
      'check_box_outline_blank',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/check_box_outline_blank.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'check_box',
      sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/check_box.svg')
    );
    iconRegistry.addSvgIcon(
      'ic_chevron_left',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_chevron_left.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'time',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/alarm.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'alert_outline',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/alert-outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'completion',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/check-circle-outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'succeeded',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/star-circle-outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'success',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/star-circle-outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'clock_time_three_outline',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/clock-time-three-outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'failed',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/close-circle-outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'failure',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/close-circle-outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'manual',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/gesture-tap-button.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'expiration',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/clock-alert-outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'send',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/send.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'pending',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/z-wave.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'queued',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/clock-time-three-outline.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'account_multiple',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/account-multiple.svg'
      )
    );
    iconRegistry.addSvgIcon(
      'ic_crucible_steamfitter',
      sanitizer.bypassSecurityTrustResourceUrl(
        'assets/svg-icons/ic_crucible_steamfitter.svg'
      )
    );

    this.theme$.pipe(takeUntil(this.unsubscribe$)).subscribe((theme) => {
      this.setTheme(theme);
    });
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
    throw new Error('Method not implemented.');
  }
}
