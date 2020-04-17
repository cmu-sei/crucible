/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { NgModule, ErrorHandler } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  MatButtonModule,
  MatSlideToggleModule,
  MatListModule,
  MatTableModule,
  MatInputModule,
  MatProgressSpinnerModule,
  MatIconModule,
  MatMenuModule,
  MatPaginatorModule,
  MatGridListModule,
  MatCardModule,
  MatSnackBarModule,
  MatBottomSheetModule,
  MatDialogModule,
  MatTabsModule
} from '@angular/material';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CdkTableModule } from '@angular/cdk/table';
import { FlexLayoutModule } from '@angular/flex-layout';
import { HttpClientModule, HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppComponent } from './app.component';
import { VmListComponent } from './components/vm-list/vm-list.component';
import { VmMainComponent } from './components/vm-main/vm-main.component';
import { FocusedAppComponent } from './components/focused-app/focused-app.component';
import { VmService } from './services/vm/vm.service';
import { SettingsService } from './services/settings/settings.service';
import { APP_INITIALIZER } from '@angular/core';
import { AuthInterceptor } from './services/auth/auth.interceptor.service';
import { AuthGuard } from './services/auth/auth-guard.service';
import { AuthService } from './services/auth/auth.service';
import { AppRoutingModule } from './app-routing.module';
import { AuthCallbackComponent } from './components/auth/auth-callback.component';
import { AuthLogoutComponent } from './components/auth/auth-logout.component';
import { ConsoleComponent } from './components/console/console.component';
import { AutoDeployComponent } from './components/auto-deploy/auto-deploy.component';
import { AutoDeployService } from './services/auto-deploy/auto-deploy.service';
import { AuthCallbackSilentComponent } from './components/auth/auth-callback-silent.component';
import { FileService } from './services/file/file.service';
import { ConfirmDialogComponent } from './components/shared/confirm-dialog/confirm-dialog.component';
import { DialogService } from './services/dialog/dialog.service';
import { TeamsService } from './services/teams/teams.service';
import { ErrorService } from './services/error/error.service';
import { SystemMessageComponent } from './components/shared/system-message/system-message.component';
import { SystemMessageService } from './services/system-message/system-message.service';
import { WelderComponent } from './components/welder/welder.component';
import {WelderService} from './services/welder/welder.service';

export function initConfig(settings: SettingsService) {
  return () => settings.load();
}

@NgModule({
  exports: [
    CdkTableModule,
    MatButtonModule,
    MatListModule,
    MatTableModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatMenuModule,
    MatPaginatorModule,
    MatGridListModule,
    MatSlideToggleModule,
    MatCardModule,
    MatSnackBarModule,
    MatBottomSheetModule,
    MatDialogModule,
    MatTabsModule
  ]
})
export class AngularMaterialModule { }

@NgModule({
  declarations: [
    AppComponent,
    VmListComponent,
    VmMainComponent,
    FocusedAppComponent,
    AutoDeployComponent,
    AuthCallbackComponent,
    AuthCallbackSilentComponent,
    AuthLogoutComponent,
    ConsoleComponent,
    ConfirmDialogComponent,
    SystemMessageComponent,
    WelderComponent,
  ],
  imports: [
    HttpClientModule,
    BrowserModule,
    BrowserAnimationsModule,
    AngularMaterialModule,
    ReactiveFormsModule,
    FormsModule,
    FlexLayoutModule,
    AppRoutingModule
  ],
  providers: [
    VmService,
    AutoDeployService,
    AuthGuard,
    AuthService,
    SettingsService,
    FileService,
    TeamsService,
    DialogService,
    SystemMessageService,
    WelderService,
    {
      provide: APP_INITIALIZER,
      useFactory: initConfig,
      deps: [SettingsService],
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    {
      provide: ErrorHandler,
      useClass: ErrorService
    }
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    ConfirmDialogComponent,
    SystemMessageComponent,
  ]
})
export class AppModule { }

