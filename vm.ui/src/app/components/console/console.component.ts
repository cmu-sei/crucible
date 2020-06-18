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
import { VmService } from '../../services/vm/vm.service';
import { VmModel } from '../../models/vm-model';
import { SettingsService } from '../../services/settings/settings.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-console',
  templateUrl: './console.component.html',
  styleUrls: ['./console.component.css']
})
export class ConsoleComponent implements OnInit {

  constructor(
    private vmService: VmService,
    private settings: SettingsService,
    private route: ActivatedRoute) { }

  ngOnInit() {
    const viewId = this.route.snapshot.params['viewId'];
    const name = this.route.snapshot.params['name'];

    this.vmService.GetViewVmsByName(viewId, name)
      .subscribe(
        vms => {
          if (vms != null) {
            const vm = vms[0];

            if (vm) {
              window.location.href = vm.url;
            }
          }
        },
        err => {
        }
      );
  }
}
