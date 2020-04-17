/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {CwdAuthService} from '../../services';

@Component({
  selector: 'cas-cwd-auth-callback',
  templateUrl: './cwd-auth-callback.component.html',
  styleUrls: ['./cwd-auth-callback.component.scss']
})
export class CwdAuthCallbackComponent implements OnInit {
  errorMessage: string;
  constructor(private route: ActivatedRoute, private router: Router, private authService: CwdAuthService) { }

  ngOnInit() {
    this.route.fragment.subscribe(frag => {
      this.validate(frag);
    });
  }
  
  validate(frag) {
    this.authService.completeAuthentication(frag)
      .then(
        (user) => {
        if (user && user.state) {
          const userGuid = user.profile.sub;
          this.router.navigateByUrl(user.state || '/');
        }
      },
        (err) => {
          console.log(err);
          this.errorMessage = err;
        }
      );
  }
}

