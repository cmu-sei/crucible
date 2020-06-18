/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { ModuleStore } from './module.store';
import { Injectable, InjectionToken } from '@angular/core';
import {
  ModulesService,
  Module,
  CreateSnippetCommand,
  CreateModuleRepositoryCommand,
} from '../../generated/caster-api';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ModuleService {
  constructor(
    private moduleStore: ModuleStore,
    private modulesService: ModulesService
  ) {}

  load(): Observable<Module[]> {
    this.moduleStore.setLoading(true);
    return this.modulesService.getAllModules().pipe(
      tap((modules: Module[]) => {
        this.moduleStore.set(modules);
      }),
      tap(() => {
        this.moduleStore.setLoading(false);
      })
    );
  }

  loadModuleById(id: string): Observable<Module> {
    this.moduleStore.setLoading(true);
    return this.modulesService.getModule(id).pipe(
      tap((_module: Module) => {
        this.moduleStore.upsert(_module.id, { ..._module });
        this.setSaved(_module.id, true);
      }),
      tap(() => {
        this.moduleStore.setLoading(false);
      })
    );
  }

  createOrUpdateModuleById(id: string): Observable<Module> {
    this.moduleStore.setLoading(true);
    const command: CreateModuleRepositoryCommand = { id };
    return this.modulesService
      .createTerrraformModuleFromRepository(command)
      .pipe(
        tap((_module: Module) => {
          this.moduleStore.upsert(_module.id, { ..._module });
          this.setSaved(_module.id, true);
        }),
        tap(() => {
          this.moduleStore.setLoading(false);
        })
      );
  }

  delete(id: string): Observable<string> {
    return this.modulesService.deleteModule(id).pipe(
      tap(() => {
        this.moduleStore.remove(id);
        this.moduleStore.ui.remove(id);
      })
    );
  }

  createVersionSnippet(
    createSnippetCommand: CreateSnippetCommand
  ): Observable<string> {
    return this.modulesService.createSnippet(createSnippetCommand);
  }

  toggleSelected(id: string) {
    this.moduleStore.ui.upsert(id, (entity) => ({
      isSelected: !entity.isSelected,
    }));
  }

  // saved state is not toggled, set explicitly.
  setSaved(id, saved) {
    this.moduleStore.ui.upsert(id, { isSaved: saved });
  }

  setActive(id) {
    this.moduleStore.setActive(id);
    this.moduleStore.ui.setActive(id);
  }
}
