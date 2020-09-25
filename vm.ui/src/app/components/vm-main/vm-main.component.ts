/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ComnAuthService, Theme } from '@crucible/common';
import { RouterQuery } from '@datorama/akita-ng-router-store';
import { BehaviorSubject, combineLatest, Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import { VmModel } from '../../vms/state/vm.model';
import { VmsQuery } from '../../vms/state/vms.query';
import { SignalRService } from '../shared/signalr/signalr.service';

@Component({
  selector: 'app-vm-main',
  templateUrl: './vm-main.component.html',
  styleUrls: ['./vm-main.component.scss'],
})
export class VmMainComponent implements OnInit, OnDestroy {

  unsubscribe$: Subject<null> = new Subject<null>();

  constructor(
    private vmQuery: VmsQuery,
    private signalRService: SignalRService,
    private routerQuery: RouterQuery,
    private activatedRoute: ActivatedRoute,
    private authService: ComnAuthService
  ) {
    this.activatedRoute.queryParamMap.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
      const selectedTheme = params.get('theme');
      const theme = selectedTheme === Theme.DARK ? Theme.DARK : Theme.LIGHT;
      this.authService.setUserTheme(theme);
    });
  }

  public openVms: Array<{ [name: string]: string }>;
  public selectedTab: number;
  public vms$: Observable<VmModel[]>;
  public vmErrors$ = new BehaviorSubject<Record<string, string>>({});

  ngOnInit() {

    this.openVms = new Array<{ [name: string]: string }>();
    this.selectedTab = 0;

    this.vms$ = combineLatest([this.vmQuery.selectAll(), this.vmErrors$]).pipe(
      map(([vms, errors]) => {
        return vms.map((y) => ({
          ...y,
          lastError: errors[y.id],
        }));
      })
    );

    this.signalRService
      .startConnection()
      .then(() => {
        this.signalRService.joinView(this.routerQuery.getParams('viewId'));
      })
      .catch((err) => {
        console.log(err);
      });
  }

  onOpenVmHere(vmObj: { [name: string]: string }) {
    // Only open if not already
    const index = this.openVms.findIndex((vm) => vm.name === vmObj.name);
    if (index === -1) {
      this.openVms.push(vmObj);
      this.selectedTab = this.openVms.length;
    } else {
      this.selectedTab = index + 1;
    }
  }

  remove(name: string) {
    const index = this.openVms.findIndex((vm) => vm.name === name);
    if (index !== -1) {
      this.selectedTab = 0;
      this.openVms.splice(index, 1);
    }
  }

  openInNewTab(vmObj: { [name: string]: string }) {
    const index = this.openVms.findIndex((vm) => vm.name === vmObj.name);
    if (index !== -1) {
      this.selectedTab = 0;
      this.openVms.splice(index, 1);
      window.open(vmObj.url, '_blank');
    }
  }

  ngOnDestroy() {
    this.signalRService.leaveView(this.routerQuery.getParams('viewId'));
    this.vmErrors$.complete();
    this.unsubscribe$.next(null);
    this.unsubscribe$.complete();
  }

  onErrors(errors: { [key: string]: string }) {
    this.vmErrors$.next(errors);
  }
}
