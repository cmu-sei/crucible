/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { CdkTableModule } from '@angular/cdk/table';
import { HttpClientModule } from '@angular/common/http';
import { ErrorHandler, NgModule } from '@angular/core';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
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
import { DragToSelectModule } from 'ngx-drag-to-select';
import { environment } from '../environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AutoDeployComponent } from './components/auto-deploy/auto-deploy.component';
import { ConsoleComponent } from './components/console/console.component';
import { FocusedAppComponent } from './components/focused-app/focused-app.component';
import { ConfirmDialogComponent } from './components/shared/confirm-dialog/confirm-dialog.component';
import { SystemMessageComponent } from './components/shared/system-message/system-message.component';
import { VmListComponent } from './components/vm-list/vm-list.component';
import { VmMainComponent } from './components/vm-main/vm-main.component';
import { WelderComponent } from './components/welder/welder.component';
import { BASE_PATH } from './generated/vm-api';
import { AutoDeployService } from './services/auto-deploy/auto-deploy.service';
import { DialogService } from './services/dialog/dialog.service';
import { ErrorService } from './services/error/error.service';
import { FileService } from './services/file/file.service';
import { SystemMessageService } from './services/system-message/system-message.service';
import { TeamsService } from './services/teams/teams.service';
import { WelderService } from './services/welder/welder.service';
import { VmService } from './vms/state/vms.service';

const settings: ComnSettingsConfig = {
  url: 'assets/config/settings.json',
  envUrl: 'assets/config/settings.env.json',
};

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
    MatTabsModule,
    MatCheckboxModule,
    MatTooltipModule,
  ],
})
export class AngularMaterialModule {}

@NgModule({
  declarations: [
    AppComponent,
    VmListComponent,
    VmMainComponent,
    FocusedAppComponent,
    AutoDeployComponent,
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
    ComnSettingsModule.forRoot(),
    ComnAuthModule.forRoot(),
    [
      environment.production ? [] : AkitaNgDevtools.forRoot(),
      AkitaNgRouterStoreModule,
    ],
    DragToSelectModule.forRoot(),
    // App routing order matters; We must import the AppRoutingModule last in order to maintain the wildcard PageNotFoundComponent.
    AppRoutingModule,
  ],
  providers: [
    VmService,
    AutoDeployService,
    FileService,
    TeamsService,
    DialogService,
    SystemMessageService,
    WelderService,
    {
      provide: ErrorHandler,
      useClass: ErrorService,
    },
    {
      provide: BASE_PATH,
      useFactory: getBasePath,
      deps: [ComnSettingsService],
    },
  ],
  bootstrap: [AppComponent],
  entryComponents: [ConfirmDialogComponent, SystemMessageComponent],
})
export class AppModule {}

export function getBasePath(settingsSvc: ComnSettingsService) {
  return settingsSvc.settings.ApiUrl.replace('/api', '');
}
