/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {BrowserModule} from '@angular/platform-browser';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {NgModule, ErrorHandler} from '@angular/core';
import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import {MatButtonModule, MatIconModule, MatMenuModule, MatTooltipModule, MatBottomSheetModule, MatExpansionModule} from '@angular/material';
import {AkitaNgDevtools} from '@datorama/akita-ngdevtools';
import {AkitaNgRouterStoreModule} from '@datorama/akita-ng-router-store';
import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {
  CWD_SETTINGS_TOKEN,
  CwdAuthInterceptorService,
  CwdAuthModule,
  CwdSettingsConfig,
  CwdSettingsModule
} from './sei-cwd-common';
import {AdminAppModule} from './admin-app/admin-app.module';
import {ApiModule, BASE_PATH} from './generated/caster-api';
import {CwdToolbarModule} from './sei-cwd-common/cwd-toolbar/cwd-toolbar.module';

import {TreeModule} from 'angular-tree-component';
import {ExerciseModule} from './exercise/exercise.module';
import {ErrorService} from './sei-cwd-common/cwd-error/error.service';
import {SystemMessageComponent} from './sei-cwd-common/cwd-system-message/components/system-message.component';
import {SystemMessageService} from './sei-cwd-common/cwd-system-message/services/system-message.service';
import { FlexLayoutModule } from '@angular/flex-layout';

export const settings: CwdSettingsConfig = {
  url: `assets/config/settings.json`,
  envUrl: `assets/config/settings.env.json`
};

// since the generated api code is a separate module we will set the BASE_PATH here in the global app module.
export function basePathFactory(_settings) {
  return _settings.settings.ApiUrl;
}

@NgModule({
  declarations: [
    AppComponent,
    SystemMessageComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AkitaNgDevtools.forRoot(),
    AkitaNgRouterStoreModule.forRoot(),
    AppRoutingModule,
    CwdSettingsModule.forRoot(settings),
    CwdAuthModule,
    AdminAppModule,
    ApiModule,
    CwdToolbarModule,
    MatMenuModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatBottomSheetModule,
    MatExpansionModule,
    TreeModule.forRoot(),
    ExerciseModule,
    HttpClientModule,
    FlexLayoutModule,
  ],
  providers: [
    {provide: BASE_PATH, useFactory: basePathFactory, deps: [CWD_SETTINGS_TOKEN]},
    {provide: HTTP_INTERCEPTORS, useClass: CwdAuthInterceptorService, multi: true},
    {provide: ErrorHandler, useClass: ErrorService },
    SystemMessageService
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    SystemMessageComponent,
  ]
})
export class AppModule {
}

