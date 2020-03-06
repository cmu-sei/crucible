/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material';
import {Component, Inject, OnInit, AfterContentInit} from '@angular/core';
import { PermissionService, RoleService } from '../../../../swagger-codegen/s3.player.api';

@Component({
    selector: 'select-role-permissions-dialog',
    templateUrl: './select-role-permissions-dialog.component.html'
})
export class SelectRolePermissionsDialogComponent implements OnInit {
  public title: string;
  public role: any;
  public permissions: any[];
  public selectedPermissions: any[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) data,
    private permissionService: PermissionService,
    private dialogRef: MatDialogRef<SelectRolePermissionsDialogComponent>) {
    this.dialogRef.disableClose = true;
  }

  /**
   * Initialization
   */
  ngOnInit() {
    this.permissions.sort(function(a, b) {
      return a.key.toLowerCase().localeCompare(b.key.toLowerCase());
    });
    this.role.permissions.forEach(permission => {
      this.selectedPermissions.push(permission.id);
    });
  }


  /**
   * Updates the selected permission
   * @param permissionGuid
   */
  updateSelection(permissionGuid) {
    const match = this.role.permissions.find(x => x.id === permissionGuid);
    if (!match) {
      this.permissionService.addPermissionToRole(this.role.id, permissionGuid).subscribe();
    } else {
      this.permissionService.removePermissionFromRole(this.role.id, permissionGuid).subscribe();
    }
  }

  close() {
    this.dialogRef.close({});
  }

  done() {
    this.dialogRef.close({'role': this.role});
  }

}


