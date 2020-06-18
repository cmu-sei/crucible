/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {
  Component,
  OnInit,
  EventEmitter,
  Output,
  NgZone,
  ViewChild,
  Input,
} from '@angular/core';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatStepper } from '@angular/material/stepper';
import {
  FormControl,
  FormGroupDirective,
  NgForm,
  Validators,
} from '@angular/forms';
import { PermissionService, PermissionQuery } from '../../../permissions/state';
import { UserService, UserQuery } from '../../../users/state';
import {
  User,
  Permission,
  UserPermission,
} from '../../../generated/caster-api';
import { Subject, Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';

@Component({
  selector: 'cas-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css'],
})
export class UsersComponent implements OnInit {
  public matcher = new UserErrorStateMatcher();
  public isLinear = false;
  public users$: Observable<User[]>;
  public isLoading$: Observable<boolean>;
  public permissions$: Observable<Permission[]>;

  constructor(
    public zone: NgZone,
    private userService: UserService,
    private userQuery: UserQuery,
    private permissionQuery: PermissionQuery,
    private permissionService: PermissionService
  ) {}

  /**
   * Initialize component
   */
  ngOnInit() {
    this.users$ = this.userQuery.selectAll();
    this.userService.load().pipe(take(1)).subscribe();
    this.permissions$ = this.permissionQuery.selectAll();
    this.permissionService.load().pipe(take(1)).subscribe();
    this.isLoading$ =
      this.userQuery.selectLoading() || this.permissionQuery.selectLoading();
  }

  create(newUser: User) {
    this.userService.create(newUser).pipe(take(1)).subscribe();
  }

  delete(userId: string) {
    this.userService.delete(userId).pipe(take(1)).subscribe();
  }

  addUserPermission(userPermission: UserPermission) {
    this.userService.addUserPermission(
      userPermission.userId,
      userPermission.permissionId
    );
  }

  removeUserPermission(userPermission: UserPermission) {
    this.userService.removeUserPermission(
      userPermission.userId,
      userPermission.permissionId
    );
  }
} // End Class

/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(
    control: FormControl | null,
    form: FormGroupDirective | NgForm | null
  ): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}
