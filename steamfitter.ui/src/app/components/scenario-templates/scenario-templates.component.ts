/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {
  Component,
  EventEmitter,
  NgZone,
  Output,
  ViewChild,
} from '@angular/core';
import { FormControl, FormGroupDirective, NgForm } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { MatStepper } from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ScenarioTemplateDataService } from 'src/app/data/scenario-template/scenario-template-data.service';
import { ScenarioTemplateQuery } from 'src/app/data/scenario-template/scenario-template.query';
import { ScenarioTemplate } from 'src/app/swagger-codegen/dispatcher.api';

/** Error when invalid control is dirty, touched, or submitted. */
export class UserErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(
    control: FormControl | null,
    form: FormGroupDirective | NgForm | null
  ): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}

@Component({
  selector: 'app-scenario-templates',
  templateUrl: './scenario-templates.component.html',
  styleUrls: ['./scenario-templates.component.scss'],
})
export class ScenarioTemplatesComponent {
  @Output() editComplete = new EventEmitter<boolean>();
  @ViewChild(ScenarioTemplatesComponent) child;
  @ViewChild('stepper') stepper: MatStepper;

  matcher = new UserErrorStateMatcher();
  isLinear = false;
  scenarioTemplateList = this.scenarioTemplateDataService.scenarioTemplateList;
  selectedScenarioTemplate = this.scenarioTemplateDataService.selected;
  scenarioTemplatePageEvent = this.scenarioTemplateDataService.pageEvent;
  isLoading = this.scenarioTemplateQuery.selectLoading();
  filterControl: FormControl = this.scenarioTemplateDataService.filterControl;
  filterString: Observable<string>;
  pageSize: Observable<number>;
  pageIndex: Observable<number>;

  constructor(
    public zone: NgZone,
    private scenarioTemplateDataService: ScenarioTemplateDataService,
    private scenarioTemplateQuery: ScenarioTemplateQuery,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this.scenarioTemplateDataService.load();
    this.filterString = activatedRoute.queryParamMap.pipe(
      map((params) => params.get('scenarioTemplatemask') || '')
    );
    this.pageSize = activatedRoute.queryParamMap.pipe(
      map((params) => parseInt(params.get('pagesize') || '20', 10))
    );
    this.pageIndex = activatedRoute.queryParamMap.pipe(
      map((params) => parseInt(params.get('pageindex') || '0', 10))
    );
  }

  setActive(id: string) {
    this.scenarioTemplateDataService.setActive(id);
  }

  sortChangeHandler(sort: Sort) {
    this.router.navigate([], {
      queryParams: { sorton: sort.active, sortdir: sort.direction },
      queryParamsHandling: 'merge',
    });
  }

  pageChangeHandler(page: PageEvent) {
    this.router.navigate([], {
      queryParams: { pageindex: page.pageIndex, pagesize: page.pageSize },
      queryParamsHandling: 'merge',
    });
  }

  saveScenarioTemplate(scenarioTemplate: ScenarioTemplate) {
    if (!scenarioTemplate.id) {
      this.scenarioTemplateDataService.add(scenarioTemplate);
    } else {
      this.scenarioTemplateDataService.updateScenarioTemplate(scenarioTemplate);
    }
  }
} // End Class
