/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-vm-main',
  templateUrl: './vm-main.component.html',
  styleUrls: ['./vm-main.component.css']
})
export class VmMainComponent implements OnInit {

  constructor() { }

  public openVms: Array<{[name: string]: string}>;
  public selectedTab: number;

  ngOnInit() {
    this.openVms = new Array<{[name: string]: string}>();
    this.selectedTab = 0;
  }

  onOpenVmHere(vmObj: {[name: string]: string}) {
    // Only open if not already
    const index = this.openVms.findIndex(vm => vm.name === vmObj.name);
    if (index === -1) {
      this.openVms.push(vmObj);
      this.selectedTab = this.openVms.length;
    } else {
      this.selectedTab = index + 1;
    }
  }

  remove(name: string) {
    const index = this.openVms.findIndex(vm => vm.name === name);
    if (index !== -1) {
      this.selectedTab = 0;
      this.openVms.splice(index, 1);
    }
  }

  openInNewTab(vmObj: {[name: string]: string}) {
    const index = this.openVms.findIndex(vm => vm.name === vmObj.name);
    if (index !== -1) {
      this.selectedTab = 0;
      this.openVms.splice(index, 1);
      window.open(vmObj.url, '_blank');
    }
  }

}

