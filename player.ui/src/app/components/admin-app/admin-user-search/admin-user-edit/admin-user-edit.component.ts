/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnChanges, Input, Output, EventEmitter } from '@angular/core';
import { User, UserService, Role, RoleService, Permission, PermissionService } from '../../../../swagger-codegen/s3.player.api';
import { ErrorStateMatcher } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';

@Component({
  selector: 'app-admin-user-edit',
  templateUrl: './admin-user-edit.component.html',
  styleUrls: ['./admin-user-edit.component.css']
})
export class AdminUserEditComponent implements OnChanges {

  @Input() user: User;
  @Output() editComplete = new EventEmitter<boolean>();

  public nameFormControl = new FormControl('', [
    Validators.required,
    Validators.minLength(4)
  ]);

  public matcher = new UserErrorStateMatcher();
  public originalUser: User;
  public permissions: Permission[] = [];
  public selectedPermissions: string[] = [];
  public roles: Role[] = [];

  constructor(
    private userService: UserService,
    private permissionService: PermissionService,
    private roleService: RoleService
  ) { }

  /**
   * Called when the form changes
   */
  ngOnChanges() {
    this.originalUser = this.user;
    this.permissionService.getPermissions().subscribe(permissions => {
      this.permissions = permissions;
    });
    this.roleService.getRoles().subscribe(roles => {
      this.roles = roles;
    });
    this.selectedPermissions = [];
    if (!!this.user && !!this.user.permissions) {
      this.user.permissions.forEach(permission => {
        this.selectedPermissions.push(permission.id);
      });
    }
  }

  /**
   * Returns the edit form to the user search screen
   */
  returnToUserSearch(): void {
    this.editComplete.emit(true);
  }

  /**
   * Saves the current user
   */
  save() {
    console.log(this.nameFormControl.value);
    if (this.user.name !== this.nameFormControl.value) {
      this.user.name = this.nameFormControl.value;

      this.userService.updateUser(this.user.id, this.user)
        .subscribe(user => {
          this.user = user;
        });
    }
  }

  /**
   * Updates the user permissions
   * @param permission
   */
  updatePermissions(permission) {
    const index = this.user.permissions.findIndex(x => x.id === permission.id);
    if (index === -1) {
      this.user.permissions.push(permission);
      this.permissionService.addPermissionToUser(this.user.id, permission.id).subscribe();
    } else {
      this.user.permissions.slice(index);
      this.permissionService.removePermissionFromUser(this.user.id, permission.id).subscribe();
    }
  }

  /**
   * Updates the user role
   */
  updateRole() {
    if (!this.user.roleId) {
      this.user.roleId = null;
      this.user.roleName = null;
    } else {
      this.user.roleName = this.roles.find(x => x.id === this.user.roleId).name;
    }
    this.userService.updateUser(this.user.id, this.user).subscribe();
  }

}


/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

