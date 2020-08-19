/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { ResultStore } from 'src/app/data/result/result.store';
import { ResultQuery } from 'src/app/data/result/result.query';
import { Injectable } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { Router, ActivatedRoute } from '@angular/router';
import { Result, ResultService } from 'src/app/swagger-codegen/dispatcher.api';
import { map, take, tap } from 'rxjs/operators';
import { BehaviorSubject, Observable, combineLatest } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class ResultDataService {
  private _requestedId$ = new BehaviorSubject<string>('');
  private _apiResults = new BehaviorSubject<Result[]>([]);
  readonly resultList: Observable<Result[]>;
  readonly selected: Observable<Result>;
  readonly filterControl = new FormControl();
  private filterTerm: Observable<string>;
  private _pageEvent: PageEvent = {length: 0, pageIndex: 0, pageSize: 10};
  readonly pageEvent = new BehaviorSubject<PageEvent>(this._pageEvent);

  constructor(
    private resultStore: ResultStore,
    private resultQuery: ResultQuery,
    private resultService: ResultService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this.filterTerm = activatedRoute.queryParamMap.pipe(
      tap(params => {
        const resultId = params.get('resultId') || '';
        if (resultId !== this._requestedId$.getValue()) {
          if (!!resultId) {
            this.loadById(resultId);
          }
          this._requestedId$.next(resultId);
        }
      }),
      map(params => params.get('resultmask') || '')
    );
    this.filterControl.valueChanges.subscribe(term => {
      this.router.navigate([], { queryParams: { resultmask: term }, queryParamsHandling: 'merge'});
    });
    this.resultList = combineLatest([this.resultQuery.selectAll(), this.filterTerm, this.pageEvent]).pipe(
      map(([items, filterTerm, page]) => {
        if (!items || items.length === 0) {
          if (page.length !== 0) {
            page.length = 0;
            page.pageIndex = 0;
          }
          return [];
        }

        let resultList = items ? items as Result[] : [];
        resultList = resultList.filter(item => (item.vmName && item.vmName.toLowerCase().includes(filterTerm.toLowerCase())) ||
          (item.actualOutput && item.actualOutput.toLowerCase().includes(filterTerm.toLowerCase())) ||
          (item.expectedOutput && item.expectedOutput.toLowerCase().includes(filterTerm.toLowerCase())));
        const pgsz = page.pageSize;
        const startIndex = page.pageIndex * pgsz;
        resultList = resultList.splice(startIndex, pgsz);
        // if the resultList length has changed, then a new pageEvent is needed
        if (this._pageEvent.length !== resultList.length) {
          this._pageEvent = {
            length: resultList.length,
            pageIndex: 0,
            pageSize: this._pageEvent.pageSize
          };
          this.pageEvent.next(this._pageEvent);
        }
        return resultList;
      })
    );
    this.selected = combineLatest([this.resultList, this._requestedId$]).pipe(
      map(([resultList, requestedResultId]) => {
        return resultList.find(result => result.id === requestedResultId);
      })
    );
  }

  load() {
    this.resultStore.setLoading(true);
    this.resetStore();
    this.resultService.getResults().pipe(
      tap(() => { this.resultStore.setLoading(false); }),
      take(1)
    ).subscribe(results => {
      results.forEach(r => this.fixDates(r));
      this.setStore(results);
    });
  }

  loadByScenario(scenarioId: string) {
    this.resetStore();
    this.resultService.getScenarioResults(scenarioId).pipe(take(1)).subscribe(results => {
      results.forEach(r => this.fixDates(r));
      this.setStore(results);
    });
  }

  loadByTask(taskId: string) {
    this.resultService.getTaskResults(taskId).pipe(take(1)).subscribe(results => {
      results.forEach(r => this.fixDates(r));
      this.updateStoreMany(results);
      }
    );
  }

  loadByUser(userId: string) {
    this.resetStore();
    this.resultService.getUserResults(userId).pipe(take(1)).subscribe(results => {
      results.forEach(r => this.fixDates(r));
      this.setStore(results);
      }
    );
  }

  loadByView(viewId: string) {
    this.resetStore();
    this.resultService.getViewResults(viewId).pipe(take(1)).subscribe(results => {
      results.forEach(r => this.fixDates(r));
      this.setStore(results);
      }
    );
  }

  loadByVm(vmId: string) {
    this.resultService.getVmResults(vmId).pipe(take(1)).subscribe(results => {
      results.forEach(r => this.fixDates(r));
      this.updateStoreMany(results);
      }
    );
  }

  loadById(id: string) {
    this.resultService.getResult(id).pipe(take(1)).subscribe(result => {
      this.fixDates(result);
      this.updateStore({...result});
    });
  }

  add(result: Result) {
    this.resultStore.setLoading(true);
    this.resultService.createResult(result).pipe(
        tap(() => { this.resultStore.setLoading(false); }),
        take(1)
      ).subscribe(t => {
        this.updateStore(t);
        this.setActive(t.id);
      }
    );
  }

  updateResult(result: Result) {
    this.resultStore.setLoading(true);
    this.resultService.updateResult(result.id, result).pipe(
        tap(() => { this.resultStore.setLoading(false); }),
        take(1)
      ).subscribe(dt => {
        this.updateStore(dt);
      }
    );
  }

  delete(id: string) {
    this.resultService.deleteResult(id).pipe(take(1)).subscribe(dt => {
      this.deleteFromStore(id);
    });
  }

  setActive(id: string) {
    this.router.navigate([], { queryParams: { resultId: id }, queryParamsHandling: 'merge'});
  }

  setPageEvent(pageEvent: PageEvent) {
    this.resultStore.update({pageEvent: pageEvent});
  }

  setStore(results: Result[]) {
    results.forEach(result => this.setAsDates(result));
    this.resultStore.set(results);
  }

  updateStore(result: Result) {
    this.setAsDates(result);
    this.resultStore.upsert(result.id, result);
  }

  updateStoreMany(results: Result[]) {
    results.forEach(result => this.setAsDates(result));
    this.resultStore.upsertMany(results);
  }

  deleteFromStore(id: string) {
    this.resultStore.remove(id);
  }

  resetStore() {
    this.resultStore.set([]);
    if (this._requestedId$.getValue()) {
      this.loadById(this._requestedId$.getValue());
    }
  }

  fixDates(result: Result) {
    // set as date object and handle c# not adding 'Z' to UTC dates.
    result.dateCreated = new Date(result.dateCreated + 'Z');
    result.dateModified = new Date(result.dateModified + 'Z');
    result.statusDate = new Date(result.statusDate + 'Z');
    result.sentDate = new Date(result.sentDate + 'Z');
  }

  setAsDates(result: Result) {
    // set to a date object.
    result.dateCreated = new Date(result.dateCreated);
    result.dateModified = new Date(result.dateModified);
    result.statusDate = new Date(result.statusDate);
    result.sentDate = new Date(result.sentDate);
  }

}


