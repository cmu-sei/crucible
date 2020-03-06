/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

import { AuthGuard } from './services/auth/auth-guard.service';
import { AuthCallbackComponent } from './components/auth/auth-callback.component';
import { VmMainComponent } from './components/vm-main/vm-main.component';
import { AuthLogoutComponent } from './components/auth/auth-logout.component';
import { ConsoleComponent } from './components/console/console.component';
import { AutoDeployComponent } from './components/auto-deploy/auto-deploy.component';
import { AuthCallbackSilentComponent } from './components/auth/auth-callback-silent.component';
import { WelderComponent } from './components/welder/welder.component';

export const ROUTES: Routes = [
  { path: 'auth-callback', component: AuthCallbackComponent },
  { path: 'auth-callback-silent', component: AuthCallbackSilentComponent },
  { path: 'logout', component: AuthLogoutComponent },
  { path: 'exercises/:exerciseId/auto-deploy', component: AutoDeployComponent, canActivate: [AuthGuard] },
  { path: 'exercises/:exerciseName/:teamId/welder', component: WelderComponent, canActivate: [AuthGuard] },
  { path: 'exercises/:exerciseId/vms/:name/console', component: ConsoleComponent, canActivate: [AuthGuard] },
  { path: 'exercises/:exerciseId', component: VmMainComponent, canActivate: [AuthGuard] },
  { path: '**', component: VmMainComponent, canActivate: [AuthGuard] },
];

@NgModule({
  exports: [
    RouterModule
  ],
  imports: [
    CommonModule,
    RouterModule.forRoot(ROUTES)
  ]
})
export class AppRoutingModule { }

