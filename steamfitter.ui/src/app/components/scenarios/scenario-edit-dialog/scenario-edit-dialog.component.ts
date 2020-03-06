/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon� and CERT� are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, EventEmitter, Output, Inject } from '@angular/core';
import { DialogService } from '../../../services/dialog/dialog.service';
import { MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-scenario-edit-dialog',
  templateUrl: './scenario-edit-dialog.component.html',
  styleUrls: ['./scenario-edit-dialog.component.css']
})

export class ScenarioEditDialogComponent {

  @Output() editComplete = new EventEmitter<boolean>();

  constructor(
    public dialogService: DialogService,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  /**
   * Closes the edit screen
   */
  handleEditComplete(changesWereMade: boolean): void {
    this.editComplete.emit(changesWereMade);
  }

}
