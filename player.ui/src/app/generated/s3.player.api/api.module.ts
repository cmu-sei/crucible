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
  NgModule,
  ModuleWithProviders,
  SkipSelf,
  Optional,
} from '@angular/core';
import { Configuration } from './configuration';
import { HttpClient } from '@angular/common/http';

import { ApplicationService } from './api/application.service';
import { PermissionService } from './api/permission.service';
import { RoleService } from './api/role.service';
import { TeamService } from './api/team.service';
import { TeamMembershipService } from './api/teamMembership.service';
import { UserService } from './api/user.service';
import { ViewService } from './api/view.service';
import { ViewMembershipService } from './api/viewMembership.service';

@NgModule({
  imports: [],
  declarations: [],
  exports: [],
  providers: [],
})
export class ApiModule {
  public static forRoot(
    configurationFactory: () => Configuration
  ): ModuleWithProviders<ApiModule> {
    return {
      ngModule: ApiModule,
      providers: [{ provide: Configuration, useFactory: configurationFactory }],
    };
  }

  constructor(
    @Optional() @SkipSelf() parentModule: ApiModule,
    @Optional() http: HttpClient
  ) {
    if (parentModule) {
      throw new Error(
        'ApiModule is already loaded. Import in your base AppModule only.'
      );
    }
    if (!http) {
      throw new Error(
        'You need to import the HttpClientModule in your AppModule! \n' +
          'See also https://github.com/angular/angular/issues/20575'
      );
    }
  }
}
