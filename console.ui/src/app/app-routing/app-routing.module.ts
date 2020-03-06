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
import { RouterModule, Routes } from '@angular/router';
import { ConsoleComponent } from '../components/console/console.component';
import { PageNotFoundComponent } from '../components/page-not-found/page-not-found.component';
import { AuthCallbackComponent } from '../components/auth/auth-callback/auth-callback.component';
import { AuthLogoutComponent } from '../components/auth/auth-logout/auth-logout.component';
import { AuthGuard } from '../services/auth/auth-guard.service';
import { AuthCallbackSilentComponent } from '../components/auth/auth-callback-silent/auth-callback-silent.component';

const routes: Routes = [
  { path: 'vm/:id/console', component: ConsoleComponent, canActivate: [AuthGuard] },
  { path: 'auth-callback', component: AuthCallbackComponent },
  { path: 'auth-callback-silent', component: AuthCallbackSilentComponent },
  { path: 'logout', component: AuthLogoutComponent },
  { path: '**', component: PageNotFoundComponent, canActivate: [AuthGuard] }
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forRoot(routes)
  ],
  declarations: []
})
export class AppRoutingModule { }

