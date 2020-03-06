/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { APP_INITIALIZER } from '@angular/core';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';

import { ApiModule as SwaggerCodegenApiModule } from './swagger-codegen/alloy.api/api.module';
import { BASE_PATH } from './swagger-codegen/alloy.api';

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
import { LabsListComponent } from './components/home-app/labs-list/labs-list.component';
import { LabInfoComponent } from './components/home-app/lab-info/lab-info.component';
import { DefinitionsComponent } from './components/admin-app/definitions/definitions.component';
import { DefinitionListComponent } from './components/admin-app/definitions/definition-list/definition-list.component';
import { DefinitionEditComponent } from './components/admin-app/definitions/definition-edit/definition-edit.component';
import { ImplementationsComponent } from './components/admin-app/implementations/implementations.component';
import { ImplementationListComponent } from './components/admin-app/implementations/implementation-list/implementation-list.component';
import { ImplementationEditComponent } from './components/admin-app/implementations/implementation-edit/implementation-edit.component';
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
    LabsListComponent,
    LabInfoComponent,
    DefinitionsComponent,
    DefinitionListComponent,
    DefinitionEditComponent,
    ImplementationsComponent,
    ImplementationListComponent,
    ImplementationEditComponent,
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
      deps: [SettingsService],
      multi: true
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
    DefinitionEditComponent,
    ImplementationEditComponent,
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

