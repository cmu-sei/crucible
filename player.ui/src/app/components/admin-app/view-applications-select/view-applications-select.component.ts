/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { View, ApplicationTemplate, ApplicationService, Application } from '../../../generated/s3.player.api';
import { DialogService } from '../../../services/dialog/dialog.service';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material';

@Component({
  selector: 'app-view-applications-select',
  templateUrl: './view-applications-select.component.html',
  styleUrls: ['./view-applications-select.component.css']
})
export class ViewApplicationsSelectComponent implements OnInit {

  @Input() view: View;
  @ViewChild(ViewApplicationsSelectComponent, { static: false }) child;

  public nameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(3)
  ]);

  public urlFormControl = new FormControl('', [
    Validators.required
  ]);

  public iconFormControl = new FormControl('', [
    Validators.required
  ]);

  public matcher = new AppErrorStateMatcher();

  public applications: Array<Application>;
  public applicationTemplates = new Array<ApplicationTemplate>();
  public currentApp: Application;
  public isLoading: Boolean;

  constructor(
    public applicationService: ApplicationService,
    public dialogService: DialogService
  ) { }

  /**
   * Initialization
   */
  ngOnInit() {
    this.isLoading = false;

    if (!this.view) {
      // either a team or a view must be provided, so roles and permissions will not be functional
      console.log('The applications select component requires either an view, therefore will be non-functional.');
      return;
    } else {
      this.updateApplications();
    }

    this.applicationService.getApplicationTemplates().subscribe(appTmps => {
      this.applicationTemplates = appTmps;
    });
  }


  /**
   * Called to update the list of apps for the view
   */
  updateApplications() {
    this.isLoading = true;
    this.applicationService.getViewApplications(this.view.id).subscribe(appInsts => {
      this.applications = appInsts;
      this.isLoading = false;
    });
  }


  /**
   * Saves the application name
   * @param name New name of the application
   * @param id app Guid
   */
  saveApplicationName(name: string, id: string): void {
      if (name === '') {
        name = null;
      }

      // if (!this.nameFormControl.hasError('minlength') && !this.nameFormControl.hasError('required')) {
      this.applicationService.getApplication(id).subscribe(app => {
        app.name = name;
        this.saveApplication(app);
      });
    // }
  }


  /**
 * Saves the application url
 * @param url New url for the application
 * @param id app Guid
 */
  saveApplicationUrl(url: string, id: string): void {
    if (url === '') {
      url = null;
    }

    // if (!this.urlFormControl.hasError('required')) {
      this.applicationService.getApplication(id).subscribe(app => {
        app.url = url;
        this.saveApplication(app);
      });
    // }
  }


  /**
   * Saves the application icon path
   * @param iconPath New icon path for the application
   * @param id app Guid
   */
  saveApplicationIcon(iconPath: string, id: string): void {
    if (iconPath === '') {
      iconPath = null;
    }

    // if (!this.iconFormControl.hasError('required')) {
      this.applicationService.getApplication(id).subscribe(app => {
        app.icon = iconPath;
        this.saveApplication(app);
      });
    // }
  }


  /**
   * Saves the application embeddable flag
   * @param application The changed application object
   */
  saveApplicationEmbeddable(application: Application): void {
    this.applicationService.getApplication(application.id).subscribe(app => {
      app.embeddable = application.embeddable;
      this.saveApplication(app);
    });
  }


    /**
   * Saves the application load in background flag
   * @param application The changed application object
   */
  saveApplicationLoadInBackground(application: Application): void {
    this.applicationService.getApplication(application.id).subscribe(app => {
      app.loadInBackground = application.loadInBackground;
      this.saveApplication(app);
    });
  }

  saveApplicationTemplateId(application: Application): void {
    this.applicationService.getApplication(application.id).subscribe(app => {
      app.applicationTemplateId = application.applicationTemplateId;
      this.saveApplication(app);
    });
  }


  /**
   * Generically saves the application for the view and updates the applications list
   */
  saveApplication(app: Application) {
    this.applicationService.updateApplication(app.id, app).subscribe(rslt => {
      this.applicationService.getViewApplications(this.view.id).subscribe(appInsts => {
        this.applications = appInsts;
      });
      console.log('Application name updated');
    });
  }


  /**
   * Removes an app from the view
   * @param app The app to delete
   */
  deleteViewApplication(app: Application) {
    this.dialogService.confirm('Delete Application', 'Are you sure that you want to remove the application ' + this.getAppName(app) + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.applicationService.deleteApplication(app.id)
            .subscribe(deleted => {
              console.log('successfully deleted application');
              this.updateApplications();
            });
        }
      });
  }

  getAppName(app: Application) {
    if (app.name != null) {
      return app.name;
    } else if (app.applicationTemplateId != null) {
      const template = this.applicationTemplates.find(x => x.id === app.applicationTemplateId);

      if (template != null) {
        return template.name;
      } else {
        return null;
      }
    } else {
      return null;
    }
  }

  getAppIcon(app: Application) {
    if (app.icon != null) {
      return app.icon;
    } else if (app.applicationTemplateId != null) {
      const template = this.applicationTemplates.find(x => x.id === app.applicationTemplateId);

      if (template != null) {
        return template.icon;
      } else {
        return null;
      }
    } else {
      return null;
    }
  }

  getTemplate(applicationTemplateId: string) {
    const template = this.applicationTemplates.find(x => x.id === applicationTemplateId);
    return template;
  }

}
/** Error when invalid control is dirty, touched, or submitted. */
export class AppErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}
