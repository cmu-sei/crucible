/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, EventEmitter, Output, NgZone, ViewChild } from '@angular/core';
import { ErrorStateMatcher, MatStepper } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm } from '@angular/forms';
import { PlayerDataService } from 'src/app/services/data/player-data-service';

/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

@Component({
  selector: 'app-vm-task-execute',
  templateUrl: './vm-task-execute.component.html',
  styleUrls: ['./vm-task-execute.component.css']
})
export class VmTaskExecuteComponent {

  @Output() editComplete = new EventEmitter<boolean>();
  @ViewChild(VmTaskExecuteComponent) child;
  @ViewChild('stepper') stepper: MatStepper;

  matcher = new UserErrorStateMatcher();
  isLinear = false;
  viewList = this.playerDataService.viewList;
  selectedView = this.playerDataService.selectedView;
  vmList = this.playerDataService.vmList;
  pageEvent = this.playerDataService.vmPageEvent;

  constructor(
    public zone: NgZone,
    private playerDataService: PlayerDataService
  ) {
    this.playerDataService.getViewsFromApi();
  }

  /**
   * Returns the stepper to zero index
   */
  resetStepper() {
    if (this.stepper) {
      console.log('here  ' + this.stepper);
      this.stepper.selectedIndex = 0;
    }
  }

  /**
   * Closes the edit screen
   */
  returnToMain(): void {
    // this.currentTeam = undefined;
    // this.editComplete.emit(true);
  }

  /**
   * Called when the mat-step index has changed to signal an update to the task
   * @param event SelectionChange event
   */
  onTaskStepChange(event: any) {
    // index 2 is the Teams step.  Refresh when selected to ensure latest information updated
    if (event.selectedIndex === 2) {
      // this.updateView();
    } else {
      // Clicked away from teams
      // this.currentTeam = undefined;
      // this.updateApplicationTemplates();
    }
  }

} // End Class
