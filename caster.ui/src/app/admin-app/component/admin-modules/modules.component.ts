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
  OnInit,
  EventEmitter,
  Output,
  NgZone,
  ViewChild,
  Input,
} from '@angular/core';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatStepper } from '@angular/material/stepper';
import {
  FormControl,
  FormGroupDirective,
  NgForm,
  Validators,
} from '@angular/forms';
import { ModuleService, ModuleQuery } from '../../../modules/state';
import { Module } from '../../../generated/caster-api';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';

@Component({
  selector: 'cas-admin-modules',
  templateUrl: './modules.component.html',
  styleUrls: ['./modules.component.css'],
})
export class AdminModulesComponent implements OnInit {
  public modules$: Observable<Module[]>;
  public isLoading$: Observable<boolean>;
  public matcher = new ModuleErrorStateMatcher();
  public isLinear = false;

  constructor(
    public zone: NgZone,
    private moduleService: ModuleService,
    private moduleQuery: ModuleQuery
  ) {}

  /**
   * Initialize component
   */
  ngOnInit() {
    this.modules$ = this.moduleQuery.selectAll();
    this.moduleService
      .load()
      .pipe(
        take(1)
        // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
      )
      .subscribe();
    this.isLoading$ = this.moduleQuery.selectLoading();
  }

  load() {
    // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
    this.moduleService.load().pipe(take(1)).subscribe();
  }

  loadById(id: string) {
    // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
    this.moduleService.loadModuleById(id).pipe(take(1)).subscribe();
  }

  createOrUpdateById(id: string) {
    // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
    this.moduleService.createOrUpdateModuleById(id).pipe(take(1)).subscribe();
  }

  delete(id: string) {
    // tslint:disable-next-line: rxjs-prefer-angular-takeuntil
    this.moduleService.delete(id).pipe(take(1)).subscribe();
  }
} // End Class

/** Error when invalid control is dirty, touched, or submitted. */
export class ModuleErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(
    control: FormControl | null,
    form: FormGroupDirective | NgForm | null
  ): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || isSubmitted));
  }
}
