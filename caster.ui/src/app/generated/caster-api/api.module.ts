/**
 * Crucible
 * Copyright 2020 Carnegie Mellon University.
 * NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
 * Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
 * [DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
 * Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
 * DM20-0181
 */

import { NgModule, ModuleWithProviders, SkipSelf, Optional } from '@angular/core';
import { Configuration } from './configuration';
import { HttpClient } from '@angular/common/http';


import { AppliesService } from './api/applies.service';
import { DirectoriesService } from './api/directories.service';
import { FilesService } from './api/files.service';
import { HostsService } from './api/hosts.service';
import { ModulesService } from './api/modules.service';
import { PermissionsService } from './api/permissions.service';
import { PlansService } from './api/plans.service';
import { ProjectsService } from './api/projects.service';
import { ResourcesService } from './api/resources.service';
import { RunsService } from './api/runs.service';
import { TerraformService } from './api/terraform.service';
import { UserPermissionsService } from './api/userPermissions.service';
import { UsersService } from './api/users.service';
import { WorkspacesService } from './api/workspaces.service';

@NgModule({
  imports:      [],
  declarations: [],
  exports:      [],
  providers: [
    AppliesService,
    DirectoriesService,
    FilesService,
    HostsService,
    ModulesService,
    PermissionsService,
    PlansService,
    ProjectsService,
    ResourcesService,
    RunsService,
    TerraformService,
    UserPermissionsService,
    UsersService,
    WorkspacesService ]
})
export class ApiModule {
    public static forRoot(configurationFactory: () => Configuration): ModuleWithProviders {
        return {
            ngModule: ApiModule,
            providers: [ { provide: Configuration, useFactory: configurationFactory } ]
        };
    }

    constructor( @Optional() @SkipSelf() parentModule: ApiModule,
                 @Optional() http: HttpClient) {
        if (parentModule) {
            throw new Error('ApiModule is already loaded. Import in your base AppModule only.');
        }
        if (!http) {
            throw new Error('You need to import the HttpClientModule in your AppModule! \n' +
            'See also https://github.com/angular/angular/issues/20575');
        }
    }
}
