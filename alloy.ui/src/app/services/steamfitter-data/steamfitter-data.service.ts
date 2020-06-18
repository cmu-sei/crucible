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
import { ScenarioTemplate, SteamfitterService } from 'src/app/generated/alloy.api';
import {map, take} from 'rxjs/operators';
import {Observable, combineLatest, BehaviorSubject} from 'rxjs';
import {Router, ActivatedRoute} from '@angular/router';

@Injectable({
  providedIn: 'root'
})

export class SteamfitterDataService {
  private _scenarioTemplates: ScenarioTemplate[];
  private _scenarioTemplateMask: Observable<string>;
  readonly scenarioTemplates = new BehaviorSubject<ScenarioTemplate[]>(this._scenarioTemplates);
  readonly scenarioTemplateList: Observable<ScenarioTemplate[]>;
  readonly scenarioTemplateFilter = new FormControl();
  readonly selectedScenarioTemplate: Observable<ScenarioTemplate>;
  private _selectedScenarioTemplateId: string;
  private _vmMask: Observable<string>;
  private requestedScenarioTemplateId = this.activatedRoute.queryParamMap.pipe(
    map(params => params.get('stId') || '')
  );

  constructor(
    private steamfitterService: SteamfitterService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this._scenarioTemplateMask = activatedRoute.queryParamMap.pipe(
      map(params => params.get('stMask') || '')
    );
    this.scenarioTemplateFilter.valueChanges.subscribe(term => {
      router.navigate([], { queryParams: { stMask: term }, queryParamsHandling: 'merge'});
    });
    this.scenarioTemplateList = combineLatest([this.scenarioTemplates, this._scenarioTemplateMask]).pipe(
      map(([items, filterTerm]) =>
        items ? (items as ScenarioTemplate[])
          .sort((a: ScenarioTemplate, b: ScenarioTemplate) => a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1)
          .filter(item => item.name.toLowerCase().includes(filterTerm.toLowerCase()) ||
            item.id.toLowerCase().includes(filterTerm.toLowerCase()))
        : [])
    );
    this.selectedScenarioTemplate = combineLatest([this.scenarioTemplateList, this.requestedScenarioTemplateId]).pipe(
      map(([scenarioTemplateList, requestedScenarioTemplateId]) => {
        if (scenarioTemplateList && scenarioTemplateList.length > 0 && requestedScenarioTemplateId) {
          const selectedScenarioTemplate = scenarioTemplateList.find(scenarioTemplate => scenarioTemplate.id === requestedScenarioTemplateId);
          if (selectedScenarioTemplate && selectedScenarioTemplate.id !== this._selectedScenarioTemplateId) {
            this._selectedScenarioTemplateId = selectedScenarioTemplate.id;
          }
          return selectedScenarioTemplate;
        } else {
          this._selectedScenarioTemplateId = '';
          return undefined;
        }
      })
    );
  }

  private updateScenarioTemplates(scenarioTemplates: ScenarioTemplate[]) {
    this._scenarioTemplates = Object.assign([], scenarioTemplates);
    this.scenarioTemplates.next(this._scenarioTemplates);
  }

  getScenarioTemplatesFromApi() {
    this.steamfitterService.getScenarioTemplates().pipe(take(1)).subscribe(scenarioTemplates => {
      this.updateScenarioTemplates(scenarioTemplates);
    }, error => {
      this.updateScenarioTemplates([]);
    });
  }

  selectScenarioTemplate(scenarioTemplateId: string) {
    this.router.navigate([], { queryParams: { stId: scenarioTemplateId }, queryParamsHandling: 'merge'});
  }

}
