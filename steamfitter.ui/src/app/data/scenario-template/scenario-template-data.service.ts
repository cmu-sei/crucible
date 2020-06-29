/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {ScenarioTemplateStore} from './scenario-template.store';
import {ScenarioTemplateQuery} from './scenario-template.query';
import {Injectable} from '@angular/core';
import {FormControl} from '@angular/forms';
import {PageEvent} from '@angular/material';
import {Router, ActivatedRoute} from '@angular/router';
import {ScenarioTemplate, ScenarioTemplateService} from 'src/app/swagger-codegen/dispatcher.api';
import {map, take, tap} from 'rxjs/operators';
import {BehaviorSubject, Observable, combineLatest} from 'rxjs';
import {TaskDataService} from 'src/app/data/task/task-data.service';

@Injectable({
  providedIn: 'root'
})

export class ScenarioTemplateDataService {
  private _requestedScenarioTemplateId: string;
  private _requestedScenarioTemplateId$ = this.activatedRoute.queryParamMap.pipe(
    map(params => params.get('scenarioTemplateId') || ''),
  );
  readonly scenarioTemplateList: Observable<ScenarioTemplate[]>;
  readonly selected: Observable<ScenarioTemplate>;
  readonly filterControl = new FormControl();
  private filterTerm: Observable<string>;
  private sortColumn: Observable<string>;
  private sortIsAscending: Observable<boolean>;
  private _pageEvent: PageEvent = {length: 0, pageIndex: 0, pageSize: 10};
  readonly pageEvent = new BehaviorSubject<PageEvent>(this._pageEvent);
  private pageSize: Observable<number>;
  private pageIndex: Observable<number>;

  constructor(
    private scenarioTemplateStore: ScenarioTemplateStore,
    private scenarioTemplateQuery: ScenarioTemplateQuery,
    private scenarioTemplateService: ScenarioTemplateService,
    private taskDataService: TaskDataService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this.filterTerm = activatedRoute.queryParamMap.pipe(
      map(params => params.get('scenarioTemplatemask') || '')
    );
    this.filterControl.valueChanges.subscribe(term => {
      this.router.navigate([], { queryParams: { scenarioTemplatemask: term }, queryParamsHandling: 'merge'});
    });
    this.sortColumn = activatedRoute.queryParamMap.pipe(
      map(params => params.get('sorton') || 'name')
    );
    this.sortIsAscending = activatedRoute.queryParamMap.pipe(
      map(params => (params.get('sortdir') || 'asc') === 'asc')
    );
    this.pageSize = activatedRoute.queryParamMap.pipe(
      map(params => parseInt((params.get('pagesize') || '20'), 10))
    );
    this.pageIndex = activatedRoute.queryParamMap.pipe(
      map(params => parseInt((params.get('pageindex') || '0'), 10))
    );
    this.scenarioTemplateList = combineLatest([this.scenarioTemplateQuery.selectAll(), this.filterTerm, this.sortColumn, this.sortIsAscending, this.pageSize, this.pageIndex]).pipe(
      map(([items, filterTerm, sortColumn, sortIsAscending, pageSize, pageIndex]) =>
        items ? (items as ScenarioTemplate[])
          .sort((a: ScenarioTemplate, b: ScenarioTemplate) => this.sortScenarioTemplates(a, b, sortColumn, sortIsAscending))
          .filter(scenarioTemplate => ('' + scenarioTemplate.name).toLowerCase().includes(filterTerm.toLowerCase()) ||
            scenarioTemplate.id.toLowerCase().includes(filterTerm.toLowerCase()))
        : [])
    );
    this.selected = combineLatest([this.scenarioTemplateList, this._requestedScenarioTemplateId$]).pipe(
      map(([scenarioTemplateList, requestedScenarioTemplateId]) => {
        let selectedScenarioTemplate: ScenarioTemplate = null;
        if (scenarioTemplateList && scenarioTemplateList.length > 0 && requestedScenarioTemplateId) {
          selectedScenarioTemplate = scenarioTemplateList.find(scenarioTemplate => scenarioTemplate.id === requestedScenarioTemplateId);
          if (selectedScenarioTemplate && selectedScenarioTemplate.id !== this._requestedScenarioTemplateId) {
            this.scenarioTemplateStore.setActive(selectedScenarioTemplate.id);
            this.taskDataService.loadByScenarioTemplate(selectedScenarioTemplate.id);
            this._requestedScenarioTemplateId = requestedScenarioTemplateId;
          }
        } else {
          this._requestedScenarioTemplateId = '';
          this.scenarioTemplateStore.setActive('');
          this.scenarioTemplateStore.update({taskList: []});
        }
        return selectedScenarioTemplate;
      })
    );
  }

  private sortScenarioTemplates(a: ScenarioTemplate, b: ScenarioTemplate, column: string, isAsc: boolean) {
    switch (column) {
      case 'name': return (a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1) * (isAsc ? 1 : -1);
      case 'description': return (a.description.toLowerCase() < b.description.toLowerCase() ? -1 : 1) * (isAsc ? 1 : -1);
      case 'durationHours': return (a.durationHours < b.durationHours ? -1 : 1) * (isAsc ? 1 : -1);
      case 'dateCreated': return (a.dateCreated.valueOf() < b.dateCreated.valueOf() ? -1 : 1) * (isAsc ? 1 : -1);
      default: return 0;
    }
  }

  load() {
    this.scenarioTemplateStore.setLoading(true);
    this.scenarioTemplateService.getScenarioTemplates().pipe(
      tap(() => { this.scenarioTemplateStore.setLoading(false); }),
      take(1)
    ).subscribe(scenarioTemplates => {
      this.scenarioTemplateStore.set(scenarioTemplates);
    }, error => {
      this.scenarioTemplateStore.set([]);
    });
  }

  loadById(id: string): Observable<ScenarioTemplate> {
    this.scenarioTemplateStore.setLoading(true);
    return this.scenarioTemplateService.getScenarioTemplate(id).pipe(
      tap((_scenarioTemplate: ScenarioTemplate) => {
        this.scenarioTemplateStore.upsert(_scenarioTemplate.id, {..._scenarioTemplate});
      }),
      tap(() => { this.scenarioTemplateStore.setLoading(false); })
    );
  }

  add(scenarioTemplate: ScenarioTemplate) {
    this.scenarioTemplateStore.setLoading(true);
    this.scenarioTemplateService.createScenarioTemplate(scenarioTemplate).pipe(
        tap(() => { this.scenarioTemplateStore.setLoading(false); }),
        take(1)
      ).subscribe(s => {
        this.scenarioTemplateStore.add(s);
        this.setActive(s.id);
      }
    );
  }

  copyScenarioTemplate(scenarioTemplateId: string) {
    this.scenarioTemplateStore.setLoading(true);
    this.scenarioTemplateService.copyScenarioTemplate(scenarioTemplateId).pipe(
        tap(() => { this.scenarioTemplateStore.setLoading(false); }),
        take(1)
      ).subscribe(s => {
        this.scenarioTemplateStore.add(s);
        this.setActive(s.id);
      }
    );
  }

  updateScenarioTemplate(scenarioTemplate: ScenarioTemplate) {
    this.scenarioTemplateStore.setLoading(true);
    this.scenarioTemplateService.updateScenarioTemplate(scenarioTemplate.id, scenarioTemplate).pipe(
        tap(() => { this.scenarioTemplateStore.setLoading(false); }),
        take(1)
      ).subscribe(n => {
        this.updateStore(n);
      }
    );
  }

  delete(id: string) {
    this.scenarioTemplateService.deleteScenarioTemplate(id).pipe(take(1)).subscribe(r => {
      this.deleteFromStore(id);
      this.setActive('');
    });
  }

  setActive(id: string) {
    this.router.navigate([], { queryParams: { scenarioTemplateId: id }, queryParamsHandling: 'merge'});
  }

  setPageEvent(pageEvent: PageEvent) {
    this.scenarioTemplateStore.update({pageEvent: pageEvent});
  }

  updateStore(scenarioTemplate: ScenarioTemplate) {
    this.scenarioTemplateStore.upsert(scenarioTemplate.id, scenarioTemplate);
  }

  deleteFromStore(id: string) {
    this.scenarioTemplateStore.remove(id);
  }

}


