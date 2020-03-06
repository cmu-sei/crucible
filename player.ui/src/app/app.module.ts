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
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, APP_INITIALIZER, ErrorHandler } from '@angular/core';
import { HttpModule } from '@angular/http';
import { CdkTableModule } from '@angular/cdk/table';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { PushNotificationsModule } from 'ng-push';
import { FlexLayoutModule } from '@angular/flex-layout';
import { ClipboardModule } from 'ngx-clipboard';

import { AppComponent } from './app.component';

import { ApiModule as SwaggerCodegenApiModule } from './swagger-codegen/s3.player.api/api.module';
import { BASE_PATH } from './swagger-codegen/s3.player.api';

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
import { ApplicationListComponent } from './components/player/application-list/application-list.component';
import { NotificationsComponent } from './components/player/notifications/notifications.component';
import { FocusedAppComponent } from './components/player/focused-app/focused-app.component';
import { PlayerComponent } from './components/player/player.component';
import { HomeAppComponent } from './components/home-app/home-app.component';
import { AppRoutingModule } from './app-routing.module';
import { ExerciseListComponent } from './components/home-app/exercise-list/exercise-list.component';
import { AuthCallbackComponent } from './components/auth/auth-callback.component';
import { AuthCallbackSilentComponent } from './components/auth/auth-callback-silent.component';
import { AppService } from './app.service';
import { SettingsService } from './services/settings/settings.service';
import { AuthGuard } from './services/auth/auth-guard.service';
import { AuthService } from './services/auth/auth.service';
import { AuthInterceptor } from './services/auth/auth.interceptor.service';
import { FocusedAppService } from './services/focused-app/focused-app.service';
import { NotificationService } from './services/notification/notification.service';
import { TeamsService } from './services/teams/teams.service';
import { LoggedInUserService } from './services/logged-in-user/logged-in-user.service';
import { ExercisesService } from './services/exercises/exercises.service';
import { ApplicationsService } from './services/applications/applications.service';
import { DialogService } from './services/dialog/dialog.service';
import { ConfirmDialogComponent } from './components/shared/confirm-dialog/confirm-dialog.component';
import { CreatePermissionDialogComponent } from './components/admin-app/admin-role-permission-search/create-permission-dialog/create-permission-dialog.component';
import { CreateRoleDialogComponent } from './components/admin-app/admin-role-permission-search/create-role-dialog/create-role-dialog.component';
import { SelectRolePermissionsDialogComponent } from './components/admin-app/admin-role-permission-search/select-role-permissions-dialog/select-role-permissions-dialog.component';
import { SystemMessageComponent } from './components/shared/system-message/system-message.component';
import { SystemMessageService } from './services/system-message/system-message.service';
import { AuthLogoutComponent } from './components/auth/auth-logout.component';
import { AdminAppComponent } from './components/admin-app/admin-app.component';
import { AdminExerciseSearchComponent } from './components/admin-app/admin-exercise-search/admin-exercise-search.component';
import { AdminUserSearchComponent } from './components/admin-app/admin-user-search/admin-user-search.component';
import { AdminAppTemplateSearchComponent } from './components/admin-app/admin-app-template-search/admin-app-template-search.component';
import { AdminUserEditComponent } from './components/admin-app/admin-user-search/admin-user-edit/admin-user-edit.component';
import { AdminRolePermissionSearchComponent } from './components/admin-app/admin-role-permission-search/admin-role-permission-search.component';
import { AdminExerciseEditComponent } from './components/admin-app/admin-exercise-search/admin-exercise-edit/admin-exercise-edit.component';
import { AddRemoveUsersDialogComponent } from './components/shared/add-remove-users-dialog/add-remove-users-dialog.component';
import { RolesPermissionsSelectComponent } from './components/admin-app/roles-permissions-select/roles-permissions-select.component';
import { TeamApplicationsSelectComponent } from './components/admin-app/team-applications-select/team-applications-select.component';
import { ExerciseApplicationsSelectComponent } from './components/admin-app/exercise-applications-select/exercise-applications-select.component';
import { ErrorService } from './services/error/error.service';
import { AdminTemplateDetailsComponent } from './components/admin-app/admin-app-template-search/admin-template-details/admin-template-details.component';

declare var require: any;

@NgModule({
  exports: [
    CdkTableModule,
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
    AppComponent,
    ApplicationListComponent,
    NotificationsComponent,
    FocusedAppComponent,
    PlayerComponent,
    HomeAppComponent,
    ExerciseListComponent,
    AuthCallbackComponent,
    AuthCallbackSilentComponent,
    AuthLogoutComponent,
    ConfirmDialogComponent,
    CreatePermissionDialogComponent,
    CreateRoleDialogComponent,
    SelectRolePermissionsDialogComponent,
    SystemMessageComponent,
    AdminAppComponent,
    AdminExerciseSearchComponent,
    AdminUserSearchComponent,
    AdminAppTemplateSearchComponent,
    AdminRolePermissionSearchComponent,
    AdminUserEditComponent,
    AdminExerciseEditComponent,
    AddRemoveUsersDialogComponent,
    RolesPermissionsSelectComponent,
    TeamApplicationsSelectComponent,
    ExerciseApplicationsSelectComponent,
    AdminTemplateDetailsComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpModule,
    HttpClientModule,
    AngularMaterialModule,
    MatNativeDateModule,
    ReactiveFormsModule,
    NgbModule.forRoot(),
    AppRoutingModule,
    PushNotificationsModule,
    FlexLayoutModule,
    SwaggerCodegenApiModule,
    ClipboardModule
  ],
  providers: [
    AppService,
    SettingsService,
    AuthGuard,
    AuthService,
    FocusedAppService,
    NotificationService,
    TeamsService,
    LoggedInUserService,
    ExercisesService,
    DialogService,
    ApplicationsService,
    SystemMessageService,
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
      provide: BASE_PATH,
      useFactory: getBasePath,
      deps: [SettingsService],
      multi: true
    },
    {
      provide: ErrorHandler,
      useClass: ErrorService
    }
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    AppComponent,
    ConfirmDialogComponent,
    CreatePermissionDialogComponent,
    CreateRoleDialogComponent,
    SelectRolePermissionsDialogComponent,
    SystemMessageComponent,
    AdminUserEditComponent,
    AdminExerciseEditComponent,
    AddRemoveUsersDialogComponent,
    RolesPermissionsSelectComponent,
    TeamApplicationsSelectComponent,
    ExerciseApplicationsSelectComponent,
    AdminTemplateDetailsComponent
  ],
})
export class AppModule { }

export function initConfig(settings: SettingsService) {
  return () => settings.load();
}

export function getBasePath(settings: SettingsService) {
  return settings.ApiUrl;
}

