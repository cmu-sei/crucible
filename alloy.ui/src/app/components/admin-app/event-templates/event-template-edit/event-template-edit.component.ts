/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, NgZone, Input, Output, EventEmitter } from '@angular/core';
import { ErrorStateMatcher } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { Definition, Exercise } from 'src/app/generated/alloy.api';
import { DialogService } from 'src/app/services/dialog/dialog.service';
import { EventTemplatesService } from 'src/app/services/event-templates/event-templates.service';
import { PlayerDataService } from 'src/app/services/player-data/player-data.service';
import { SteamfitterDataService } from 'src/app/services/steamfitter-data/steamfitter-data.service';
import { CasterDataService } from 'src/app/services/caster-data/caster-data.service';

/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

/** Error when control isn't an integer */
export class NotIntegerErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isNotAnInteger = parseInt(control.value, 10).toString() !== control.value;
    if (isNotAnInteger) {
      control.setErrors({notAnInteger: true});
    }
    const isSubmitted = form && form.submitted;
    return !!(control && (control.invalid || isNotAnInteger) && (control.dirty || isSubmitted));
  }
}

@Component({
  selector: 'app-event-template-edit',
  templateUrl: './event-template-edit.component.html',
  styleUrls: ['./event-template-edit.component.css']
})

export class EventTemplateEditComponent implements OnInit {

  @Input() eventTemplate: Definition;
  @Output() closePanel = new EventEmitter<boolean>();

  public eventTemplateNameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(4)
  ]);
  public descriptionFormControl = new FormControl('', [
    Validators.required
  ]);
  public durationHoursFormControl = new FormControl('', []);
  public viewIdFormControl = new FormControl('', []);
  public directoryIdFormControl = new FormControl('', []);
  public scenarioTemplateIdFormControl = new FormControl('', []);
  public isPublishedFormControl = new FormControl('', []);
  public useDynamicHostFormControl = new FormControl('', []);
  public matcher = new UserErrorStateMatcher();
  public notAnIntegerErrorState = new NotIntegerErrorStateMatcher();
  public viewList = this.playerDataService.viewList;
  public scenarioTemplateList = this.steamfitterDataService.scenarioTemplateList;
  public directoryList = this.casterDataService.directoryList;

  constructor(
    public eventTemplatesService: EventTemplatesService,
    private playerDataService: PlayerDataService,
    private steamfitterDataService: SteamfitterDataService,
    private casterDataService: CasterDataService,
    public dialogService: DialogService,
    public zone: NgZone
  ) {
    playerDataService.getViewsFromApi();
    steamfitterDataService.getScenarioTemplatesFromApi();
    casterDataService.getDirectoriesFromApi();
  }

  /**
   * Initialize component
   */
  ngOnInit() {
    this.eventTemplateNameFormControl.setValue(this.eventTemplate.name);
    this.descriptionFormControl.setValue(this.eventTemplate.description);
    this.durationHoursFormControl.setValue(this.eventTemplate.durationHours);
    this.viewIdFormControl.setValue(this.eventTemplate.exerciseId);
    this.directoryIdFormControl.setValue(this.eventTemplate.directoryId);
    this.scenarioTemplateIdFormControl.setValue(this.eventTemplate.scenarioId);
    this.isPublishedFormControl.setValue(this.eventTemplate.isPublished);
    this.useDynamicHostFormControl.setValue(this.eventTemplate.useDynamicHost);
  }

  /**
   * Delete an event template after confirmation
   */
  deleteEventTemplate(): void {
    this.dialogService.confirm('Delete Event Template', 'Are you sure that you want to delete Event Template ' + this.eventTemplate.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          this.eventTemplatesService.delete(this.eventTemplate.id);
          this.closePanel.emit(true);
        }
      });
  }

  /**
   * Saves the current event template
   */
  saveEventTemplate(changedField): void {
    let shouldUpdate = false;
    switch (changedField) {
      case 'name':
        if (!this.eventTemplateNameFormControl.hasError('minlength') && !this.eventTemplateNameFormControl.hasError('required')
            && this.eventTemplate.name !== this.eventTemplateNameFormControl.value) {
          this.eventTemplate.name = this.eventTemplateNameFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'description':
        if (this.eventTemplate.description !== this.descriptionFormControl.value) {
          this.eventTemplate.description = this.descriptionFormControl.value;
            shouldUpdate = true;
        }
        break;
      case 'durationHours':
        if (parseInt(this.durationHoursFormControl.value.toString(), 10).toString() === this.durationHoursFormControl.value.toString()
            && this.eventTemplate.durationHours !== this.durationHoursFormControl.value) {
          this.eventTemplate.durationHours = this.durationHoursFormControl.value;
            shouldUpdate = true;
        }
        break;
      case 'exerciseId':
        shouldUpdate = true;
        break;
      case 'directoryId':
        shouldUpdate = true;
        break;
      case 'scenarioTemplateId':
        shouldUpdate = true;
        break;
      case 'isPublished':
        if (this.eventTemplate.isPublished !== this.isPublishedFormControl.value) {
          this.eventTemplate.isPublished = this.isPublishedFormControl.value;
          shouldUpdate = true;
        }
        break;
      case 'useDynamicHost':
        if (this.eventTemplate.useDynamicHost !== this.useDynamicHostFormControl.value) {
          this.eventTemplate.useDynamicHost = this.useDynamicHostFormControl.value;
          shouldUpdate = true;
        }
        break;
      default:
        break;
    }
    if (shouldUpdate) {
      this.eventTemplatesService.update(this.eventTemplate);
    }
  }

  /**
   * Clone an event template after confirmation
   */
  cloneEventTemplate(): void {
    this.dialogService.confirm('Clone Event Template', 'Are you sure that you want to create a new Event Template from '
      + this.eventTemplate.name + '?')
      .subscribe(result => {
        if (result['confirm']) {
          const newEventTemplate = {
            name: this.eventTemplate.name + ' - clone',
            description: this.eventTemplate.description,
            durationHours: this.eventTemplate.durationHours,
            exerciseId: this.eventTemplate.exerciseId,
            directoryId: this.eventTemplate.directoryId,
            scenarioId: this.eventTemplate.scenarioId
          };
          this.eventTemplatesService.addNew(newEventTemplate);
          this.closePanel.emit(true);
        }
      });
  }

} // End Class
