/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/


import { Injectable } from '@angular/core';
import { throwError as observableThrowError, Observable, Subject, BehaviorSubject } from 'rxjs';
import { User, UserService, RoleService } from '../../swagger-codegen/s3.player.api';

// Used to display Super User text
export const SUPER_USER = 'Super User';

@Injectable()
export class LoggedInUserService {

  public loggedInUser: BehaviorSubject<User> = new BehaviorSubject(null);
  public isSuperUser: BehaviorSubject<Boolean> = new BehaviorSubject(false);

  constructor(
    private userService: UserService,
    private roleService: RoleService) { }

  /**
   * Once a user is logged in, this obtains Player Api specific User data.
   * @param guid
   */
  public setLoggedInUser(guid: string) {
    console.log('user guid: ' + guid);
    this.userService.getUser(guid).subscribe(user => {
      this.isSuperUser.next(user.isSystemAdmin);
      this.loggedInUser.next(user);
    });

  }
}

