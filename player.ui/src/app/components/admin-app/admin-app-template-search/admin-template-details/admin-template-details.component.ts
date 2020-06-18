/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ApplicationTemplate, ApplicationService } from '../../../../generated/s3.player.api';
import { DialogService } from '../../../../services/dialog/dialog.service';

@Component({
  selector: 'app-admin-template-details',
  templateUrl: './admin-template-details.component.html',
  styleUrls: ['./admin-template-details.component.css']
})
export class AdminTemplateDetailsComponent implements OnInit {

  @Input() appTemplate: ApplicationTemplate;
  @Output() refresh = new EventEmitter<boolean>();

  constructor(
    public applicationService: ApplicationService,
    public dialogService: DialogService
  ) { }

  ngOnInit() {
  }

  /**
 * Edit an application template
 */
  editAppTemplate() {
    // get new credentials and upload path
    this.applicationService.updateApplicationTemplate(this.appTemplate.id, this.appTemplate).subscribe(result => {
      this.appTemplate = result;
    });
  }

  /**
   * Deletes the application template
   */
  deleteApplicationTemplate() {
    this.dialogService.confirm('Delete Application Template?',
      'Are you sure that you want to delete application template ' + this.appTemplate.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.applicationService.deleteApplicationTemplate(this.appTemplate.id).subscribe(() => {
            this.refresh.emit(true); // True indicates that the template was deleted
          });
        }
      });

  }

}
