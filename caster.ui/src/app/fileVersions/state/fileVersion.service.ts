/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { FileVersionStore } from './fileVersion.store';
import { Injectable, InjectionToken } from '@angular/core';
import {
  FilesService,
  FileVersion,
  CreateSnippetCommand,
} from '../../generated/caster-api';
import { tap, take } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class FileVersionService {
  constructor(
    private fileVersionStore: FileVersionStore,
    private filesService: FilesService
  ) {}

  load(fileId: string): Observable<FileVersion[]> {
    this.fileVersionStore.setLoading(true);
    return this.filesService.getFileVersions(fileId).pipe(
      tap((versions: FileVersion[]) => {
        versions.forEach((version) => {
          this.fileVersionStore.upsert(version.id, version);
        });
      }),
      tap(() => {
        this.fileVersionStore.setLoading(false);
      })
    );
  }

  loadFileVersionById(id: string): Observable<FileVersion> {
    this.fileVersionStore.setLoading(true);
    return this.filesService.getFileVersion(id).pipe(
      tap((version: FileVersion) => {
        this.fileVersionStore.upsert(version.id, { ...version });
      }),
      tap(() => {
        this.fileVersionStore.setLoading(false);
      })
    );
  }

  tagFiles(tag: string, fileIds: string[]) {
    this.fileVersionStore.setLoading(true);
    return this.filesService
      .tagFiles({ tag, fileIds })
      .pipe(take(1))
      .subscribe((versions) => {
        versions.forEach((version) => {
          this.fileVersionStore.upsert(version.id, version);
        });
      });
  }

  toggleSelected(id: string) {
    this.fileVersionStore.ui.upsert(id, (entity) => ({
      isSelected: !entity.isSelected,
    }));
  }

  setActive(id) {
    this.fileVersionStore.setActive(id);
    this.fileVersionStore.ui.setActive(id);
  }
}
