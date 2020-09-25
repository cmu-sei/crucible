/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { OverlayModule } from '@angular/cdk/overlay';
import { HttpClientModule } from '@angular/common/http';
import { ErrorHandler, NgModule } from '@angular/core';
import { FlexLayoutModule } from '@angular/flex-layout';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  ComnAuthModule,
  ComnSettingsConfig,
  ComnSettingsModule,
  ComnSettingsService,
} from '@crucible/common';
import { AkitaNgRouterStoreModule } from '@datorama/akita-ng-router-store';
import { AkitaNgDevtools } from '@datorama/akita-ngdevtools';
import { environment } from '../environments/environment';
import { AdminAppModule } from './admin-app/admin-app.module';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ApiModule, BASE_PATH } from './generated/caster-api';
import { ProjectModule } from './project/project.module';
import { ErrorService } from './sei-cwd-common/cwd-error/error.service';
import { SystemMessageComponent } from './sei-cwd-common/cwd-system-message/components/system-message.component';
import { SystemMessageService } from './sei-cwd-common/cwd-system-message/services/system-message.service';
import { CwdToolbarModule } from './sei-cwd-common/cwd-toolbar/cwd-toolbar.module';

export const settings: ComnSettingsConfig = {
  url: `assets/config/settings.json`,
  envUrl: `assets/config/settings.env.json`,
};

// since the generated api code is a separate module we will set the BASE_PATH here in the global app module.
export function getBasePath(settings: ComnSettingsService) {
  return settings.settings.ApiUrl;
}

@NgModule({
  declarations: [AppComponent, SystemMessageComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    environment.production ? [] : AkitaNgDevtools,
    AkitaNgRouterStoreModule,
    AppRoutingModule,
    ComnSettingsModule.forRoot(),
    ComnAuthModule.forRoot(),
    AdminAppModule,
    ApiModule,
    CwdToolbarModule,
    MatMenuModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatBottomSheetModule,
    MatExpansionModule,
    MatToolbarModule,
    ProjectModule,
    HttpClientModule,
    FlexLayoutModule,
    OverlayModule,
  ],
  providers: [
    {
      provide: BASE_PATH,
      useFactory: getBasePath,
      deps: [ComnSettingsService],
    },
    { provide: ErrorHandler, useClass: ErrorService },
    SystemMessageService,
  ],
  bootstrap: [AppComponent],
  entryComponents: [SystemMessageComponent],
})
export class AppModule {}
