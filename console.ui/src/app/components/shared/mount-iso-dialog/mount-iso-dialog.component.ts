/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {MAT_DIALOG_DATA, MatDialogRef, MatDialogModule, MatFormFieldModule} from '@angular/material';
import {Component, Inject, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';

@Component({
    selector: 'mount-iso-dialog',
    templateUrl: './mount-iso-dialog.component.html'
})
export class MountIsoDialogComponent implements OnInit {
  public data: any;
  public publicIsos: any[];
  public teamIsos: any[];
  public filteredPublicIsos: any[];
  public filteredTeamIsos: any[];
  public selectedIso: any;
  public showTeamIsos: boolean;
  public showPublicIsos: boolean;

  constructor(
    @Inject(MAT_DIALOG_DATA) data,
    private dialogRef: MatDialogRef<MountIsoDialogComponent>) {
    this.dialogRef.disableClose = true;
    this.showPublicIsos = true;
    this.showTeamIsos = true;
  }

  ngOnInit() {
    this.publicIsos.sort(function(a, b) {
      return a.filename.toLowerCase().localeCompare(b.filename.toLowerCase());
    });
    this.teamIsos.sort(function(a, b) {
      return a.filename.toLowerCase().localeCompare(b.filename.toLowerCase());
    });
    this.filteredPublicIsos = [];
    this.publicIsos.forEach(iso => {
      const copyIso = Object.assign({}, iso);
      this.filteredPublicIsos.push(copyIso);
    });
    this.filteredTeamIsos = [];
    this.teamIsos.forEach(iso => {
      const copyIso = Object.assign({}, iso);
      this.filteredTeamIsos.push(copyIso);
    });
  }

  selectThisIso(iso: string) {
    this.selectedIso = iso;
  }

  applyFilter(filterValue: string) {
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // default to lowercase matches
    this.filteredPublicIsos = this.publicIsos.filter(iso => iso.filename.toLowerCase().includes(filterValue));
    this.filteredTeamIsos = this.teamIsos.filter(iso => iso.filename.toLowerCase().includes(filterValue));
  }

  close() {
    this.dialogRef.close({});
  }

  done() {
    this.dialogRef.close(this.selectedIso);
  }
}


