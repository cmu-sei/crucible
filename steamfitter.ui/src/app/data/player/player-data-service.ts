/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {Injectable} from '@angular/core';
import {FormControl} from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { View, PlayerService, Vm } from 'src/app/swagger-codegen/dispatcher.api';
import {map, take, switchMap} from 'rxjs/operators';
import {Observable, combineLatest, BehaviorSubject} from 'rxjs';
import {Router, ActivatedRoute} from '@angular/router';

@Injectable({
  providedIn: 'root'
})

export class PlayerDataService {
  private _views: View[];
  private _viewMask: Observable<string>;
  readonly views = new BehaviorSubject<View[]>(this._views);
  readonly viewList: Observable<View[]>;
  readonly viewFilter = new FormControl();
  readonly selectedView: Observable<View>;
  private _selectedViewId: string;
  private _vms: Vm[];
  private _vmMask = new BehaviorSubject<string>('');
  readonly vms = new BehaviorSubject<Vm[]>(this._vms);
  readonly vmList: Observable<Vm[]>;
  readonly vmFilter = new FormControl();
  private _selectedVms: string[] = [];
  readonly selectedVms = new BehaviorSubject<string[]>(this._selectedVms);
  private requestedViewId = this.activatedRoute.queryParamMap.pipe(
    map(params => params.get('viewId') || '')
  );

  constructor(
    private playerService: PlayerService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this._viewMask = activatedRoute.queryParamMap.pipe(
      map(params => params.get('exmask') || '')
    );
    this.viewFilter.valueChanges.subscribe(term => {
      router.navigate([], { queryParams: { exmask: term }, queryParamsHandling: 'merge'});
    });
    this.vmFilter.valueChanges.subscribe(term => {
      this._vmMask.next(term);
    });
    this.viewList = combineLatest([this.views, this._viewMask]).pipe(
      map(([items, filterTerm]) =>
        items ? (items as View[])
          .sort((a: View, b: View) => a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1 )
          .filter(item => item.name.toLowerCase().includes(filterTerm.toLowerCase()) ||
            item.id.toLowerCase().includes(filterTerm.toLowerCase()))
        : [])
    );
    this.vmList = combineLatest([this.vms, this._vmMask]).pipe(
      map(([items, filterTerm]) => {
        let vmList = items ? items as Vm[] : [];
        vmList = vmList
          .sort((a: Vm, b: Vm) => a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1)
          .filter(item => item.name.toLowerCase().includes(filterTerm.toLowerCase()) ||
                  item.id.toLowerCase().includes(filterTerm.toLowerCase()));
        return vmList;
      })
    );
    this.selectedView = combineLatest([this.viewList, this.requestedViewId]).pipe(
      map(([viewList, requestedViewId]) => {
        if (viewList && viewList.length > 0 && requestedViewId) {
          const selectedView = viewList.find(view => view.id === requestedViewId);
          if (selectedView && selectedView.id !== this._selectedViewId) {
            this.getViewVmsFromApi(selectedView.id, true);
            this._selectedViewId = selectedView.id;
          }
          return selectedView;
        } else {
          this._selectedViewId = '';
          return undefined;
        }
      })
    );
  }

  getViewsFromApi() {
    this.playerService.getViews().pipe(take(1)).subscribe(views => {
      this._views = Object.assign([], views);
      this.views.next(this._views);
    }, error => {
      this.views.next([]);
    });
  }

  selectView(viewId: string) {
    this.router.navigate([], { queryParams: { viewId: viewId }, queryParamsHandling: 'merge'});
  }

  getAllVmsFromApi() {
    this._vms = [];
    this.vms.next([]);
    this._views.forEach(view => {
      this.getViewVmsFromApi(view.id, false);
    });
  }

  getViewVmsFromApi(viewId: string, clearFirst: boolean) {
    if (clearFirst) {
      this._vms = [];
      this.vms.next([]);
    }
    return this.playerService.getVms(viewId).pipe(take(1)).subscribe(vms => {
      this._vms = this._vms.concat(vms);
      this.vms.next(this._vms);
    });
  }

  addSelectedVm(id: string) {
    if (this._vms.some(vm => vm.id === id)) {
      const vmList = this.selectedVms.getValue();
      vmList.push(id);
      this.selectedVms.next(vmList);
    }
  }

  removeSelectedVm(id: string) {
    const vmList = this.selectedVms.getValue().filter(vmId => vmId !== id);
    this.selectedVms.next(vmList);
  }

  resetSelectedVms() {
    this.selectedVms.next([]);
  }

}
