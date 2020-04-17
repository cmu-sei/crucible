/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {CommonModule} from '@angular/common';
import {CwdAuthCallbackComponent} from './components/cwd-auth-callback/cwd-auth-callback.component';
import {CwdAuthCallbackSilentComponent} from './components/cwd-auth-callback-silent/cwd-auth-callback-silent.component';
import {CwdAuthLogoutComponent} from './components/cwd-auth-logout/cwd-auth-logout.component';
import {CwdAuthGuardService} from './services';

const cwdAuthRoutes: Routes = [
  {path: 'auth-callback', component: CwdAuthCallbackComponent, canActivate: [CwdAuthGuardService]},
  {path: 'auth-callback-silent', component: CwdAuthCallbackSilentComponent},
  {path: 'logout', component: CwdAuthLogoutComponent}
];

@NgModule({
  declarations: [CwdAuthCallbackComponent, CwdAuthCallbackSilentComponent, CwdAuthLogoutComponent],
  imports: [
    CommonModule,
    RouterModule.forChild(cwdAuthRoutes),
  ]
})
export class CwdAuthModule { }

