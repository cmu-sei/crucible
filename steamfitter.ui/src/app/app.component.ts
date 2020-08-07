/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { MatIconRegistry } from '@angular/material';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) {
    iconRegistry.setDefaultFontSetClass('mdi');

    iconRegistry.addSvgIcon
      ('monitor', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/monitor.svg'));
    iconRegistry.addSvgIcon
      ('ic_clear_black_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_clear_black_24px.svg'));
    iconRegistry.addSvgIcon
      ('monkey_wrench', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/monkey_wrench.svg'));
    iconRegistry.addSvgIcon
      ('magnify_glass', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_magnify_glass_48px.svg'));
    iconRegistry.addSvgIcon
      ('ic_cancel_circle', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_cancel_circle.svg'));
    iconRegistry.addSvgIcon
      ('ic_back_arrow', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_back_arrow_24px.svg'));
    iconRegistry.addSvgIcon
      ('ic_magnify_search', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_magnify_glass_48px.svg'));
    iconRegistry.addSvgIcon
      ('ic_chevron_down_black_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_chevron_down_black_24px.svg'));
    iconRegistry.addSvgIcon
      ('ic_chevron_right_black_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_chevron_right_black_24px.svg'));
    iconRegistry.addSvgIcon
      ('ic_square_edit_outline_48px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_square_edit_outline_48px.svg'));
    iconRegistry.addSvgIcon
      ('ic_plus_circle_outline_48px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_plus_circle_outline_48px.svg'));
    iconRegistry.addSvgIcon
      ('ic_trash_can', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_trash_can.svg'));
    iconRegistry.addSvgIcon
      ('ic_expand_more_white_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_expand_more_white_24px.svg'));
    iconRegistry.addSvgIcon
      ('build_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/build-24px.svg'));
    iconRegistry.addSvgIcon
      ('history_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/history-24px.svg'));
    iconRegistry.addSvgIcon
      ('menu_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/menu-24px.svg'));
    iconRegistry.addSvgIcon
      ('playlist_play_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/playlist_play-24px.svg'));
    iconRegistry.addSvgIcon
      ('input_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/input-24px.svg'));
    iconRegistry.addSvgIcon
      ('play_circle_outline_24px', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/play_circle_outline-24px.svg'));
    iconRegistry.addSvgIcon
      ('open_in_new', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/open_in_new-black-18dp.svg'));
    iconRegistry.addSvgIcon
      ('content_paste', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/content_paste-black-24dp.svg'));
    iconRegistry.addSvgIcon
      ('ic_clipboard_copy', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/ic_clipboard_copy.svg'));
    iconRegistry.addSvgIcon
      ('ic_menu', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/menu_24px.svg'));
    iconRegistry.addSvgIcon
      ('storage_black_18dp', sanitizer.bypassSecurityTrustResourceUrl('assets/svg-icons/storage-black-18dp.svg'));

    }

}

