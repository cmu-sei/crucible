/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Injectable } from '@angular/core';
import { DefinitionService, Definition } from 'src/app/swagger-codegen/alloy.api';
import { Observable, BehaviorSubject, combineLatest } from 'rxjs';
import { FormControl } from '@angular/forms';
import { map, shareReplay, take } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class DefinitionsService {

  public definitionList$: Observable<Definition[]>;
  public searchControl$ = new FormControl();
  public selectedDefinitionId: string | undefined = undefined;
  public fullDefinitionList$ = new BehaviorSubject<Definition[]>([]);

  private searchTerm$ = new BehaviorSubject<string>('');


  constructor(
    private definitionService: DefinitionService
  ) {

    this.updatelist();

    this.searchControl$.valueChanges.subscribe(searchString => {
      this.searchTerm$.next(searchString.trim().toLowerCase());
    });

    this.definitionList$ = combineLatest([this.fullDefinitionList$, this.searchTerm$]).pipe(
      map(([defs, srcTerm]) => {
        if (srcTerm === '') {
          return defs;
        } else {
          return defs.filter(d => d.name.toLowerCase().includes(srcTerm.toLowerCase()));
        }
      }),
      shareReplay(1)
    );

  }

  addNew(definition: Definition) {
    this.definitionService.createDefinition(definition).pipe(take(1)).subscribe((def) => {
      this.selectedDefinitionId = def.id;
      this.updatelist();
    });
  }

  update(definition: Definition) {
    this.definitionService.updateDefinition(definition.id, definition).pipe(take(1)).subscribe(() => {
      this.updatelist();
    });
  }

  delete(definitionId: string) {
    this.definitionService.deleteDefinition(definitionId).pipe(take(1)).subscribe(() => {
      this.updatelist();
    });
  }

  private updatelist() {
    this.definitionService.getDefinitions().pipe(take(1)).subscribe(defs => this.fullDefinitionList$.next(defs));
  }
}

