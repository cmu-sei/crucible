/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ComnSettingsService } from '@crucible/common';
import { ID } from '@datorama/akita';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  BulkPowerOperation,
  BulkPowerOperationResponse,
  VmsService,
} from '../../generated/vm-api';
import { VmModel } from './vm.model';
import { VmsStore } from './vms.store';

@Injectable({ providedIn: 'root' })
export class VmService {
  private vmUrl: string;
  private teamUrl: string;
  public viewId: string;
  public teamId: string;

  constructor(
    private vmsStore: VmsStore,
    private http: HttpClient,
    private settings: ComnSettingsService,
    private router: Router,
    private vmsService: VmsService
  ) {
    this.viewId = this.router.routerState.snapshot.root.firstChild.params[
      'viewId'
    ];
    this.teamId = this.router.routerState.snapshot.root.firstChild.params[
      'teamId'
    ];

    this.vmUrl = `${settings.settings.ApiUrl}/views/${this.viewId}/vms`;
    this.teamUrl = `${settings.settings.ApiUrl}/teams/${this.teamId}/vms`;
  }

  add(vm: VmModel) {
    this.vmsStore.add(vm);
  }

  update(id, vm: Partial<VmModel>) {
    this.vmsStore.update(id, vm);
  }

  remove(id: ID) {
    this.vmsStore.remove(id);
  }

  public GetViewVms(
    includePersonal: boolean,
    onlyMine: boolean
  ): Observable<Array<VmModel>> {
    let params = new HttpParams();
    params = params.append('includePersonal', includePersonal.toString());
    params = params.append('onlyMine', onlyMine.toString());
    return this.http
      .get<Array<VmModel>>(this.vmUrl, { params: params })
      .pipe(
        tap((entities) => {
          this.vmsStore.set(entities);
        })
      );
  }

  public GetTeamVms(
    includePersonal: boolean,
    onlyMine: boolean
  ): Observable<Array<VmModel>> {
    let params = new HttpParams();
    params = params.append('includePersonal', includePersonal.toString());
    params = params.append('onlyMine', onlyMine.toString());
    return this.http.get<Array<VmModel>>(this.teamUrl, { params: params });
  }

  public GetViewVmsByName(
    viewId: string,
    name: string
  ): Observable<Array<VmModel>> {
    const url = `${this.settings.settings.ApiUrl}/views/${viewId}/vms?name=${name}`;
    return this.http.get<Array<VmModel>>(url);
  }

  public powerOn(ids: string[]): Observable<BulkPowerOperationResponse> {
    const operation: BulkPowerOperation = { ids: ids };
    return this.vmsService.bulkPowerOn(operation);
  }

  public powerOff(ids: string[]): Observable<BulkPowerOperationResponse> {
    const operation: BulkPowerOperation = { ids: ids };
    return this.vmsService.bulkPowerOff(operation);
  }

  public shutdown(ids: string[]): Observable<BulkPowerOperationResponse> {
    const args: BulkPowerOperation = { ids: ids };
    return this.vmsService.bulkShutdown(args);
  }
}
