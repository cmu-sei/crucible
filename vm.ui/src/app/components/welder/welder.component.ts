/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {Component, OnInit} from '@angular/core';
import {WelderService} from '../../services/welder/welder.service';
import {ActivatedRoute} from '@angular/router';
import {MatSnackBar} from '@angular/material';
import {VmService} from '../../services/vm/vm.service';
import {IntervalObservable} from 'rxjs/observable/IntervalObservable';

@Component({
  selector: 'app-welder',
  templateUrl: './welder.component.html',
  styleUrls: ['./welder.component.css']
})
export class WelderComponent implements OnInit {
  public showDeployButton = false;
  public deployButtonDisabled = false;
  public readyVMs = new Set();
  private viewName: string;
  private previousWSResults = [0, 0, 0];

  constructor(
    public welderService: WelderService,
    public vmService: VmService,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.viewName = this.route.snapshot.params['viewName'];
    this.checkForWorkstations();
    IntervalObservable.create(30000).subscribe(() => {
      const firstValue = this.previousWSResults[0];
      const allSame = this.previousWSResults.every(v => v === firstValue);
      // If the values are all zeroes, then I still want to poll for VMs - if they're all non-zero, but still not equal, then I also
      // want to continue polling. If they're all non-zero and also all equal, then it's time to stop polling.
      if (!(firstValue > 0 && allSame)) {
        this.checkForWorkstations();
      }

      // Once we start getting VMs back, that means our request is done or almost done.
      if (this.readyVMs.size === 0) {
        this.welderService.getQueueSize().subscribe(response => {
          if (response != null) {
            this.snackBar.open(response.toString());
          }
        }, err => console.log(err));
      }
    });
    this.showDeployButton = true;
  }

  public autoDeploy() {
    this.deployButtonDisabled = true;
    this.welderService.deployToView(this.viewName).subscribe(
      response => {
        console.log(response);
        this.snackBar.open('Request received. Please wait while your workstations are provisioned.');
      },
      err => {
        console.log(err);
        this.deployButtonDisabled = false;
      });
  }

  private checkForWorkstations() {
    this.vmService.GetTeamVms(true, true).subscribe(
      vms => {
        this.readyVMs.clear();
        vms.forEach(vm => {
          // If we're getting VMs back, then there's no point in letting the user click the deploy button again because it's a waste of
          // network resources.
          this.showDeployButton = false;
          this.readyVMs.add(vm);
        });
        if (this.readyVMs.size === 0) {
          this.showDeployButton = true;
          this.deployButtonDisabled = false;
        } else {
          // Snackbar will likely be open from queue checks, so let's just dismiss that to mitigate confusion.
          this.snackBar.dismiss();
        }
        this.previousWSResults.shift();
        this.previousWSResults.push(this.readyVMs.size);
      },
      err => {
        console.log(err);
      });
  }

  public openVMConsoleTab(vm) {
    window.open(vm.url, '_blank');
  }
}
