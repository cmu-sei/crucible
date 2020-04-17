/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, Input, OnDestroy } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PageEvent } from '@angular/material';
// Resolve imports when API changes nouns
import { PlayerDataService, View } from 'src/app/services/data/player-data-service';
import { Vm } from 'src/app/swagger-codegen/dispatcher.api';
import { NewTaskService } from 'src/app/services/new-task/new-task.service';

@Component({
  selector: 'app-vm-list',
  templateUrl: './vm-list.component.html',
  styleUrls: ['./vm-list.component.css']
})

export class VmListComponent implements OnDestroy {
  @Input() viewList: View[];
  @Input() selectedView: View;
  @Input() vmList: Vm[];
  @Input() pageEvent: PageEvent;
  displayedColumns: string[] = ['name'];

  uploading = false;
  uploadProgress = 0;
  vmApiResponded = true;
  filterControl: FormControl = this.playerDataService.vmFilter;

  constructor(
    private playerDataService: PlayerDataService,
    private newTaskService: NewTaskService
  ) {
  }

  onViewChange(event: any) {
    if (event && event.value && event.value.id) {
      this.playerDataService.selectView(event.value.id);
    }
  }

  // Local Component functions
  openInTab(url: string) {
    window.open(url, '_blank');
  }

  openHere(url: string) {
    window.location.href = url;
  }

  onCheckBoxChange(event: any, vm: Vm) {
    if (event.checked) {
      this.newTaskService.AddVm(vm);
    } else {
      this.newTaskService.RemoveVm(vm);
    }
  }

  handlePageEvent(pageEvent: PageEvent) {
    this.playerDataService.setVmPageEvent(pageEvent);
  }

  ngOnDestroy() {
    this.playerDataService.selectView('');
  }

}

