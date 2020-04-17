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
import {MatTableDataSource, MatPaginator, PageEvent} from '@angular/material';
import { Exercise, PlayerService, Vm } from 'src/app/swagger-codegen/dispatcher.api';
import {map, take, switchMap} from 'rxjs/operators';
import {Observable, combineLatest, BehaviorSubject} from 'rxjs';
import {AuthService} from 'src/app/services/auth/auth.service';
import {Router, ActivatedRoute} from '@angular/router';

export interface View extends Exercise {}

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
  private _vmMask: Observable<string>;
  readonly vms = new BehaviorSubject<View[]>(this._vms);
  readonly vmList: Observable<Vm[]>;
  readonly vmFilter = new FormControl();
  private _vmPageEvent: PageEvent = {length: 0, pageIndex: 0, pageSize: 10};
  readonly vmPageEvent = new BehaviorSubject<PageEvent>(this._vmPageEvent);
  private requestedViewId = this.activatedRoute.queryParamMap.pipe(
    map(params => params.get('exId') || '')
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
    this._vmMask = activatedRoute.queryParamMap.pipe(
      map(params => params.get('vmmask') || '')
    );
    this.vmFilter.valueChanges.subscribe(term => {
      router.navigate([], { queryParams: { vmmask: term }, queryParamsHandling: 'merge'});
    });
    this.viewList = combineLatest([this.views, this._viewMask]).pipe(
      map(([items, filterTerm]) =>
        items ? (items as View[])
          .filter(item => item.name.toLowerCase().includes(filterTerm.toLowerCase()) ||
            item.id.toLowerCase().includes(filterTerm.toLowerCase()))
        : [])
    );
    this.vmList = combineLatest([this.vms, this._vmMask, this.vmPageEvent]).pipe(
      map(([items, filterTerm, page]) => {
        if (!items || items.length === 0) {
          if (page.length !== 0) {
            page.length = 0;
            page.pageIndex = 0;
            this._vmPageEvent = page;
            this.vmPageEvent.next(page);
          }
          return [];
        }

        let vmList = items ? items as Vm[] : [];
        vmList = vmList.filter(item => item.name.toLowerCase().includes(filterTerm.toLowerCase()) ||
          item.id.toLowerCase().includes(filterTerm.toLowerCase()));
        const pgsz = page.pageSize;
        const startIndex = page.pageIndex * pgsz;
        vmList = vmList.splice(startIndex, pgsz);
        // if the vmList length has changed, then a new pageEvent is needed
        if (this._vmPageEvent.length !== vmList.length) {
          this._vmPageEvent = {
            length: vmList.length,
            pageIndex: 0,
            pageSize: this._vmPageEvent.pageSize
          };
          this.vmPageEvent.next(this._vmPageEvent);
        }
        return vmList;
      })
    );
    this.selectedView = combineLatest([this.viewList, this.requestedViewId]).pipe(
      map(([viewList, requestedViewId]) => {
        if (viewList && viewList.length > 0 && requestedViewId) {
          const selectedView = viewList.find(view => view.id === requestedViewId);
          if (selectedView && selectedView.id !== this._selectedViewId) {
            this.getVmsFromApi(selectedView.id);
            this._selectedViewId = selectedView.id;
          }
          return selectedView;
        } else {
          this._selectedViewId = '';
          this.updateVms([]);
          return undefined;
        }
      })
    );
  }

  private updateViews(views: View[]) {
    this._views = Object.assign([], views);
    this.views.next(this._views);
  }

  getViewsFromApi() {
    this.playerService.getExercises().pipe(take(1)).subscribe(views => {
      this.updateViews(views);
    }, error => {
      this.updateViews([]);
    });
  }

  selectView(viewId: string) {
    this.router.navigate([], { queryParams: { exId: viewId }, queryParamsHandling: 'merge'});
  }

  setVmPageEvent(pageEvent: PageEvent) {
    this._vmPageEvent = pageEvent;
    this.vmPageEvent.next(pageEvent);
  }

  private updateVms(vms: Vm[]) {
    this._vms = Object.assign([], vms);
    this.vms.next(this._vms);
  }

  getVmsFromApi(viewId: string) {
    return this.playerService.getVms(viewId).pipe(take(1)).subscribe(vms => {
      this.updateVms(vms);
    }, error => {
      this.updateVms([]);
    });
  }


}
