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
import { HostListener } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { VmService } from '../../services/vm/vm.service';
import { WmksComponent } from '../wmks/wmks.component';
import { OptionsBarComponent } from '../options-bar/options-bar.component';

@Component({
  selector: 'app-console',
  templateUrl: './console.component.html',
  styleUrls: ['./console.component.css']
})

export class ConsoleComponent implements OnInit {
  public noVmConsoleApi = true;
  private _route: ActivatedRoute;
  private _vmService: VmService;

  constructor(private route: ActivatedRoute, public vmService: VmService) {
    this._route = route;
    this._vmService = vmService;
  }

  ngOnInit() {
    this._route.params.subscribe(
      res => {
        this._vmService.model.id = res.id;
      },
      error => {
        console.log(error.message);
      });
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    // this will re-center the console
    if (this.vmService.wmks) {
      this.vmService.wmks.updateScreen();
    }
  }

}

