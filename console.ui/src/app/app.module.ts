/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { BrowserModule } from '@angular/platform-browser';
import { NgModule, APP_INITIALIZER, ErrorHandler } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  MatProgressSpinnerModule,
  MatButtonModule,
  MatDialogModule,
  MatMenuModule,
  MatListModule,
  MatIconModule,
  MatInputModule,
  MatBottomSheetModule,
  MatSnackBarModule
} from '@angular/material';

import { AppComponent } from './app.component';
import { OptionsBarComponent } from './components/options-bar/options-bar.component';
import { WmksComponent } from './components/wmks/wmks.component';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';
import { VmService } from './services/vm/vm.service';
import { AppRoutingModule } from './app-routing/app-routing.module';
import { ConsoleComponent } from './components/console/console.component';
import { SettingsService } from './services/settings/settings.service';
import { AuthCallbackComponent } from './components/auth/auth-callback/auth-callback.component';
import { AuthCallbackSilentComponent } from './components/auth/auth-callback-silent/auth-callback-silent.component';
import { AuthLogoutComponent } from './components/auth/auth-logout/auth-logout.component';
import { AuthGuard } from './services/auth/auth-guard.service';
import { AuthService } from './services/auth/auth.service';
import { AuthInterceptor } from './services/auth/auth.interceptor.service';
import { DialogService } from './services/dialog/dialog.service';
import { NotificationService } from './services/notification/notification.service';
import { MessageDialogComponent } from './components/shared/message-dialog/message-dialog.component';
import { SendTextDialogComponent } from './components/shared/send-text-dialog/send-text-dialog.component';
import { FileUploadInfoDialogComponent } from './components/shared/file-upload-info-dialog/file-upload-info-dialog.component';
import { MountIsoDialogComponent } from './components/shared/mount-iso-dialog/mount-iso-dialog.component';
import { KeysPipe } from './components/options-bar/options-bar.component';
import { ErrorService } from './services/error/error.service';
import { SystemMessageComponent } from './components/shared/system-message/system-message.component';
import { SystemMessageService } from './services/system-message/system-message.service';
import { FlexLayoutModule } from '@angular/flex-layout';

export function initConfig(settings: SettingsService) {
  return () => settings.load();
 }

 @NgModule({
  exports: [
    MatButtonModule,
    MatListModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatMenuModule,
    MatBottomSheetModule,
    MatDialogModule,
    MatSnackBarModule
  ]
})
export class AngularMaterialModule { }

@NgModule({
  declarations: [
    AppComponent,
    OptionsBarComponent,
    WmksComponent,
    PageNotFoundComponent,
    ConsoleComponent,
    AuthCallbackComponent,
    AuthCallbackSilentComponent,
    AuthLogoutComponent,
    MessageDialogComponent,
    SendTextDialogComponent,
    FileUploadInfoDialogComponent,
    MountIsoDialogComponent,
    KeysPipe,
    SystemMessageComponent,
  ],
  imports: [
    BrowserModule,
    HttpModule,
    HttpClientModule,
    RouterModule,
    AppRoutingModule,
    FormsModule,
    AngularMaterialModule,
    FlexLayoutModule,
    BrowserAnimationsModule
  ],
  providers: [
    VmService,
    SettingsService,
    SystemMessageService,
    {
      provide: APP_INITIALIZER,
      useFactory: initConfig,
      deps: [SettingsService],
      multi: true
    },
    AuthGuard,
    AuthService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    DialogService,
    NotificationService,
    {
      provide: ErrorHandler,
      useClass: ErrorService
    }
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    MessageDialogComponent,
    SendTextDialogComponent,
    FileUploadInfoDialogComponent,
    MountIsoDialogComponent,
    SystemMessageComponent,
  ]
})
export class AppModule { }


