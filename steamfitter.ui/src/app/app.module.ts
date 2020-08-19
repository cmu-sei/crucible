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
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { SettingsService } from './services/settings/settings.service';
import { AuthGuard } from './services/auth/auth-guard.service';
import { AuthService } from './services/auth/auth.service';
import { AuthInterceptor } from './services/auth/auth.interceptor.service';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { APP_INITIALIZER, ErrorHandler } from '@angular/core';
import { ErrorService } from './services/error/error.service';
import { BASE_PATH } from './swagger-codegen/dispatcher.api';
import { ApiModule as SwaggerCodegenApiModule } from './swagger-codegen/dispatcher.api/api.module';
import { AuthCallbackComponent } from './components/auth/auth-callback.component';
import { AuthCallbackSilentComponent } from './components/auth/auth-callback-silent.component';
import { HomeAppComponent } from './components/home-app/home-app.component';
import { AuthLogoutComponent } from './components/auth/auth-logout.component';
import { FlexLayoutModule } from '@angular/flex-layout';
import { VmListComponent } from './components/vm-list/vm-list.component';
import { SystemMessageService } from './services/system-message/system-message.service';
import { SystemMessageComponent } from './components/shared/system-message/system-message.component';
import { VmTaskExecuteComponent } from './components/vm-task-execute/vm-task-execute.component';
import { ScenarioTemplatesComponent } from './components/scenario-templates/scenario-templates.component';
import { ScenarioTemplateEditComponent } from './components/scenario-templates/scenario-template-edit/scenario-template-edit.component';
import { ScenarioTemplateEditDialogComponent } from './components/scenario-templates/scenario-template-edit-dialog/scenario-template-edit-dialog.component';
import { ScenarioTemplateListComponent } from './components/scenario-templates/scenario-template-list/scenario-template-list.component';
import { ScenariosComponent } from './components/scenarios/scenarios.component';
import { ScenarioEditComponent } from './components/scenarios/scenario-edit/scenario-edit.component';
import { ScenarioEditDialogComponent } from './components/scenarios/scenario-edit-dialog/scenario-edit-dialog.component';
import { ScenarioListComponent } from './components/scenarios/scenario-list/scenario-list.component';
import { CdkTableModule } from '@angular/cdk/table';
import { CdkTreeModule } from '@angular/cdk/tree';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ConfirmDialogComponent } from './components/shared/confirm-dialog/confirm-dialog.component';
import {NgxMaterialTimepickerModule} from 'ngx-material-timepicker';
import { ClipboardModule } from 'ngx-clipboard';
import { AdminContainerComponent } from './components/admin/admin-container/admin-container.component';
import { AdminUsersComponent } from './components/admin/admin-users/admin-users.component';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatBadgeModule } from '@angular/material/badge';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatNativeDateModule, MatRippleModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSliderModule } from '@angular/material/slider';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTreeModule } from '@angular/material/tree';
import { ResultsComponent } from './components/results/results.component';
import { DialogService } from './services/dialog/dialog.service';
import { HistoryComponent } from './components/history/history.component';
import { TaskTreeComponent } from './components/tasks/task-tree/task-tree.component';
import { TaskEditComponent } from './components/tasks/task-edit/task-edit.component';
import { TasksComponent } from './components/tasks/tasks.component';
import {AkitaNgDevtools} from '@datorama/akita-ngdevtools';
import {AkitaNgRouterStoreModule} from '@datorama/akita-ng-router-store';

@NgModule({
  declarations: [
    AppComponent,
    AuthCallbackComponent,
    AuthCallbackSilentComponent,
    AuthLogoutComponent,
    HomeAppComponent,
    VmListComponent,
    SystemMessageComponent,
    VmTaskExecuteComponent,
    ScenarioTemplatesComponent,
    ScenarioTemplateEditComponent,
    ScenarioTemplateEditDialogComponent,
    ScenarioTemplateListComponent,
    ScenariosComponent,
    ScenarioEditComponent,
    ScenarioEditDialogComponent,
    ScenarioListComponent,
    ResultsComponent,
    ConfirmDialogComponent,
    HistoryComponent,
    TaskTreeComponent,
    TaskEditComponent,
    TasksComponent,
    AdminContainerComponent,
    AdminUsersComponent
  ],
  imports: [
    AkitaNgDevtools.forRoot(),
    AkitaNgRouterStoreModule.forRoot(),
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    SwaggerCodegenApiModule,
    HttpClientModule,
    FlexLayoutModule,
    FormsModule,
    ReactiveFormsModule,
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
    MatBottomSheetModule,
    MatBadgeModule,
    MatFormFieldModule,
    CdkTableModule,
    MatTreeModule,
    CdkTreeModule,
    NgxMaterialTimepickerModule,
    ClipboardModule
  ],
  exports: [
    MatSortModule
  ],
  providers: [
    AuthService,
    SettingsService,
    AuthGuard,
    AuthService,
    DialogService,
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
    // },
    // {
    //   provide: ErrorHandler,
    //   useClass: ErrorService
    }
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    SystemMessageComponent,
    ConfirmDialogComponent,
    ScenarioTemplateEditDialogComponent,
    ScenarioEditDialogComponent,
    TaskEditComponent
  ]
})
export class AppModule { }

export function initConfig(settings: SettingsService) {
  return () => settings.load();
}

export function getBasePath(settings: SettingsService) {
  return settings.ApiUrl;
}

