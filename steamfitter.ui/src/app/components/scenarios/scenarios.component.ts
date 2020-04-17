/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, EventEmitter, OnInit, Output, NgZone, ViewChild } from '@angular/core';
import { ErrorStateMatcher, MatStepper } from '@angular/material';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { Sort } from '@angular/material/sort';
import { PlayerDataService } from 'src/app/services/data/player-data-service';
import { Scenario } from '../../swagger-codegen/dispatcher.api';
import { ScenarioDataService } from 'src/app/data/scenario/scenario-data.service';
import { ScenarioQuery } from 'src/app/data/scenario/scenario.query';
import { Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { Router, ActivatedRoute } from '@angular/router';
import { PageEvent } from '@angular/material/paginator';

@Component({
  selector: 'app-scenarios',
  templateUrl: './scenarios.component.html',
  styleUrls: ['./scenarios.component.css']
})
export class ScenariosComponent implements OnInit {

  @Output() editComplete = new EventEmitter<boolean>();
  @ViewChild(ScenariosComponent) child;
  @ViewChild('stepper') stepper: MatStepper;

  isLinear = false;
  scenarioList = this.scenarioDataService.scenarioList;
  selectedScenario = this.scenarioDataService.selected;
  scenarioPageEvent = this.scenarioDataService.pageEvent;
  isLoading = this.scenarioQuery.selectLoading();
  filterControl: FormControl = this.scenarioDataService.filterControl;
  filterString: Observable<string>;
  pageSize: Observable<number>;
  pageIndex: Observable<number>;
  views = this.playerDataService.viewList;
  statuses: Observable<string>;

  constructor(
    public zone: NgZone,
    private playerDataService: PlayerDataService,
    private scenarioDataService: ScenarioDataService,
    private scenarioQuery: ScenarioQuery,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this.scenarioDataService.load();
    this.playerDataService.getViewsFromApi();
    this.filterString = activatedRoute.queryParamMap.pipe(
      map(params => (params.get('scenariomask') || ''))
    );
    this.pageSize = activatedRoute.queryParamMap.pipe(
      map(params => (parseInt(params.get('pagesize') || '20', 10)))
    );
    this.pageIndex = activatedRoute.queryParamMap.pipe(
      map(params => (parseInt(params.get('pageindex') || '0', 10)))
    );
    this.statuses = activatedRoute.queryParamMap.pipe(
      map(params => (params.get('statuses') || 'active,ready'))
    );
  }

  ngOnInit() {
    const statuses: string = this.activatedRoute.snapshot.queryParamMap.get('statuses');
    const secondParam: string = this.activatedRoute.snapshot.queryParamMap.get('secondParamKey');
  }
  setActive(id: string) {
    this.scenarioDataService.setActive(id);
  }

  sortChangeHandler(sort: Sort) {
    this.router.navigate([], { queryParams: { sorton: sort.active, sortdir: sort.direction }, queryParamsHandling: 'merge'});
  }

  filterStatusChangeHandler(statusList: any) {
    let statuses = statusList.active ? 'active' : 'x';
    statuses = statusList.ready ? statuses + ',ready' : statuses;
    statuses = statusList.ended ? statuses + ',ended' : statuses;
    this.router.navigate([], { queryParams: { statuses: statuses }, queryParamsHandling: 'merge'});
  }

  pageChangeHandler(page: PageEvent) {
    this.router.navigate([], { queryParams: { pageindex: page.pageIndex, pagesize: page.pageSize }, queryParamsHandling: 'merge'});
  }

  saveScenario(scenario: Scenario) {
    if (!scenario.id) {
      this.scenarioDataService.add(scenario);
    } else {
      this.scenarioDataService.updateScenario(scenario);
    }
  }

} // End Class
