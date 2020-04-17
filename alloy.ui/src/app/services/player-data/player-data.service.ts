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
import { Exercise, PlayerService } from 'src/app/generated/alloy.api';
import {map, take} from 'rxjs/operators';
import {Observable, combineLatest, BehaviorSubject} from 'rxjs';
import {Router, ActivatedRoute} from '@angular/router';

@Injectable({
  providedIn: 'root'
})

export class PlayerDataService {
  private _views: Exercise[];
  private _viewMask: Observable<string>;
  readonly views = new BehaviorSubject<Exercise[]>(this._views);
  readonly viewList: Observable<Exercise[]>;
  readonly viewFilter = new FormControl();
  readonly selectedView: Observable<Exercise>;
  private _selectedViewId: string;
  private _vmMask: Observable<string>;
  private requestedViewId = this.activatedRoute.queryParamMap.pipe(
    map(params => params.get('viewId') || '')
  );

  constructor(
    private playerService: PlayerService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this._viewMask = activatedRoute.queryParamMap.pipe(
      map(params => params.get('viewMask') || '')
    );
    this.viewFilter.valueChanges.subscribe(term => {
      router.navigate([], { queryParams: { viewMask: term }, queryParamsHandling: 'merge'});
    });
    this.viewList = combineLatest([this.views, this._viewMask]).pipe(
      map(([items, filterTerm]) =>
        items ? (items as Exercise[])
          .filter(item => item.name.toLowerCase().includes(filterTerm.toLowerCase()) ||
            item.id.toLowerCase().includes(filterTerm.toLowerCase()))
        : [])
    );
    this.selectedView = combineLatest([this.viewList, this.requestedViewId]).pipe(
      map(([viewList, requestedViewId]) => {
        if (viewList && viewList.length > 0 && requestedViewId) {
          const selectedView = viewList.find(view => view.id === requestedViewId);
          if (selectedView && selectedView.id !== this._selectedViewId) {
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

  private updateViews(views: Exercise[]) {
    this._views = Object.assign([], views);
    this.views.next(this._views);
  }

  getViewsFromApi() {
    this.playerService.getExercises().pipe(take(1)).subscribe(views => {
      this.updateViews(views.filter(x => x.status === 'Inactive'));
    }, error => {
      this.updateViews([]);
    });
  }

  selectView(viewId: string) {
    this.router.navigate([], { queryParams: { viewId: viewId }, queryParamsHandling: 'merge'});
  }

}
