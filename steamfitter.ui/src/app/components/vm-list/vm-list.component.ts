/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, Input, Output, OnDestroy, EventEmitter } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PlayerDataService } from 'src/app/data/player/player-data-service';
import { Vm, View } from 'src/app/swagger-codegen/dispatcher.api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-vm-list',
  templateUrl: './vm-list.component.html',
  styleUrls: ['./vm-list.component.css']
})

export class VmListComponent implements OnDestroy {
  @Input() selectedVms: string[];
  @Output() updateVmList = new EventEmitter<string[]>();
  vmList: Vm[];
  displayedColumns: string[] = ['name'];
  private unsubscribe$ = new Subject();
  uploading = false;
  uploadProgress = 0;
  vmApiResponded = true;
  filterControl: FormControl = this.playerDataService.vmFilter;
  showSelectedOnly = false;

  constructor(
    private playerDataService: PlayerDataService
  ) {
    this.playerDataService.vmList.pipe(takeUntil(this.unsubscribe$)).subscribe(vms => {
      this.vmList = vms;
    });
  }

  // Local Component functions
  openInTab(url: string) {
    window.open(url, '_blank');
  }

  onCheckBoxChange(event: any, vmId: string) {
    if (event.checked) {
      this.selectedVms.push(vmId);
    } else {
      this.selectedVms = this.selectedVms.filter(id => id !== vmId);
    }
    this.updateVmList.emit(this.selectedVms);
  }

  checkAll() {
    this.vmList.forEach(vm => {
      this.selectedVms.push(vm.id);
    });
    this.updateVmList.emit(this.selectedVms);
  }

  uncheckAll() {
    this.vmList.forEach(vm => {
      this.selectedVms = this.selectedVms.filter(id => id !== vm.id);
    });
    this.updateVmList.emit(this.selectedVms);
    this.showSelectedOnly = this.showSelectedOnly && this.selectedVms.length > 0;
  }

  clearFilter() {
    this.filterControl.setValue('');
  }

  selectedVmList() {
    return this.vmList.filter(vm => this.selectedVms.includes(vm.id));
  }

  ngOnDestroy() {
    this.playerDataService.vmFilter.setValue('');
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

}

