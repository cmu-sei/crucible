/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Component, Inject } from '@angular/core';

@Component({
  selector: 'name-dialog',
  templateUrl: './name-dialog.component.html',
  styleUrls: ['./name-dialog.component.css'],
})
export class NameDialogComponent {
  public title: string;
  public message: string;
  public removeArtifacts = true;
  public name: string;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<NameDialogComponent>
  ) {
    this.dialogRef.disableClose = true;
    this.name = this.data.nameValue;
  }

  onClick(): void {
    this.data.artifacts && this.data.artifacts.length > 0
      ? (this.data.removeArtifacts = this.removeArtifacts)
      : (this.data.removeArtifacts = false);
    this.data.nameValue = this.name;
    this.data.wasCancelled = false;
    this.dialogRef.close(this.data);
  }

  onCancel(): void {
    this.data.artifacts && this.data.artifacts.length > 0
      ? (this.data.removeArtifacts = this.removeArtifacts)
      : (this.data.removeArtifacts = false);
    this.data.wasCancelled = true;
    this.dialogRef.close(this.data);
  }
}
