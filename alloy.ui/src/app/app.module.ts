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
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { APP_INITIALIZER } from '@angular/core';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';

import { ApiModule as SwaggerCodegenApiModule } from './generated/alloy.api/api.module';
import { BASE_PATH } from './generated/alloy.api';

import {
  MatAutocompleteModule,
  MatButtonModule,
  MatButtonToggleModule,
  MatCardModule,
  MatCheckboxModule,
  MatChipsModule,
  MatDatepickerModule,
  MatDialogModule,
  MatExpansionModule,
  MatGridListModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatNativeDateModule,
  MatPaginatorModule,
  MatProgressBarModule,
  MatProgressSpinnerModule,
  MatRadioModule,
  MatRippleModule,
  MatSelectModule,
  MatSidenavModule,
  MatSliderModule,
  MatSlideToggleModule,
  MatSnackBarModule,
  MatSortModule,
  MatTableModule,
  MatTabsModule,
  MatToolbarModule,
  MatTooltipModule,
  MatStepperModule,
  MatTableDataSource,
  MatBottomSheetModule,
  MatTree,
  MatTreeModule,
  MatBadgeModule,
} from '@angular/material';

import { AuthInterceptor } from './services/auth/auth.interceptor.service';
import { AuthCallbackComponent } from './components/auth/auth-callback.component';
import { AuthCallbackSilentComponent } from './components/auth/auth-callback-silent.component';
import { AuthLogoutComponent } from './components/auth/auth-logout.component';
import { AuthGuard } from './services/auth/auth-guard.service';
import { AuthService } from './services/auth/auth.service';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AdminAppComponent } from './components/admin-app/admin-app.component';
import { HomeAppComponent } from './components/home-app/home-app.component';
import { SettingsService } from './services/settings/settings.service';
import { LoggedInUserService } from './services/logged-in-user/logged-in-user.service';
import { EventListComponent } from './components/home-app/event-list/event-list.component';
import { EventInfoComponent } from './components/home-app/event-info/event-info.component';
import { EventTemplatesComponent } from './components/admin-app/event-templates/event-templates.component';
import { EventTemplateListComponent } from './components/admin-app/event-templates/event-template-list/event-template-list.component';
import { EventTemplateEditComponent } from './components/admin-app/event-templates/event-template-edit/event-template-edit.component';
import { EventsComponent } from './components/admin-app/events/events.component';
import { AdminEventListComponent } from './components/admin-app/events/event-list/event-list.component';
import { EventEditComponent } from './components/admin-app/events/event-edit/event-edit.component';
import { ConfirmDialogComponent } from './components/shared/confirm-dialog/confirm-dialog.component';
import { DialogService } from './services/dialog/dialog.service';
import { ClipboardModule } from 'ngx-clipboard';


@NgModule({
  exports: [
    MatAutocompleteModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatStepperModule,
    MatDatepickerModule,
    MatDialogModule,
    MatExpansionModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatNativeDateModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatRippleModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
    MatTabsModule,
    MatToolbarModule,
    MatTooltipModule,
    MatBottomSheetModule,
    MatTreeModule,
    MatBadgeModule
  ]
})
export class AngularMaterialModule { }



@NgModule({
  declarations: [
    AuthCallbackComponent,
    AuthCallbackSilentComponent,
    AuthLogoutComponent,
    AppComponent,
    AdminAppComponent,
    HomeAppComponent,
    EventListComponent,
    EventInfoComponent,
    EventTemplatesComponent,
    EventTemplateListComponent,
    EventTemplateEditComponent,
    EventsComponent,
    AdminEventListComponent,
    EventEditComponent,
    ConfirmDialogComponent
  ],
  imports: [
    HttpClientModule,
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    AngularMaterialModule,
    FlexLayoutModule,
    FormsModule,
    ReactiveFormsModule,
    SwaggerCodegenApiModule,
    ClipboardModule
  ],
  providers: [
    AuthGuard,
    AuthService,
    SettingsService,
    LoggedInUserService,
    {
      provide: APP_INITIALIZER,
      useFactory: initConfig,
      deps: [SettingsService],
      multi: true
    },
    {
      provide: BASE_PATH,
      useFactory: getBasePath,
      deps: [SettingsService]
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    DialogService
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    EventTemplateEditComponent,
    EventEditComponent,
    ConfirmDialogComponent
  ]
})
export class AppModule { }

export function initConfig(settings: SettingsService) {
  return () => settings.load();
}

export function getBasePath(settings: SettingsService) {
  return settings.ApiUrl;
}
