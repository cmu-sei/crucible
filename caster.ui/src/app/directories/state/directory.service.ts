/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { DirectoryStore } from './directory.store';
import { Injectable } from '@angular/core';
import {
  DirectoriesService,
  Directory,
  Workspace,
  ModelFile,
  ArchiveType,
} from '../../generated/caster-api';
import { DirectoryUI } from './directory.model';
import { Observable } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { DirectoryQuery } from './directory.query';
import { FileService } from 'src/app/files/state';
import { WorkspaceService } from 'src/app/workspace/state';
import HttpHeaderUtils from 'src/app/shared/utilities/http-header-utils';
import { FileDownload } from 'src/app/shared/models/file-download';

@Injectable({
  providedIn: 'root',
})
export class DirectoryService {
  constructor(
    private directoryStore: DirectoryStore,
    private directoryQuery: DirectoryQuery,
    private directoriesService: DirectoriesService,
    private fileService: FileService,
    private workspaceService: WorkspaceService
  ) {}

  loadDirectories(projectId: string): Observable<Directory[]> {
    return this.directoriesService
      .getDirectoriesByProject(projectId, true, true, false)
      .pipe(
        tap((directories) => {
          // First capture file and workspace data to load
          let files: ModelFile[] = [];
          let workspaces: Workspace[] = [];
          const directoryUIs = this.directoryQuery.ui.getAll();
          this.directoryStore.set(directories);
          directories.forEach((dir) => {
            const dUI = directoryUIs.find((d) => d.id === dir.id);
            if (dUI) {
              this.directoryStore.ui.upsert(dUI.id, dUI);
            }
            files = files.concat(dir.files);
            workspaces = workspaces.concat(dir.workspaces);
          });
          this.fileService.filesUpdated(files);
          this.workspaceService.setWorkspaces(workspaces);
        })
      );
  }

  add(directory: Directory) {
    this.directoriesService.createDirectory(directory).subscribe((dir) => {
      this.directoryStore.add(dir);
      const dirUI = this.directoryQuery.getEntity(dir.id);
      this.directoryStore.ui.upsert(dirUI.id, dirUI);
    });
  }

  update(directory: Directory) {
    this.directoriesService
      .editDirectory(directory.id, directory)
      .subscribe((dir) => {
        this.directoryStore.update(dir.id, dir);
      });
  }

  partialUpdate(id: string, directory: Partial<Directory>) {
    this.directoriesService
      .partialEditDirectory(id, directory)
      .subscribe((dir) => {
        this.directoryStore.update(dir.id, dir);
      });
  }

  delete(dirId: string) {
    this.directoriesService.deleteDirectory(dirId).subscribe(() => {
      this.deleted(dirId);
    });
  }

  updated(directory: Directory) {
    this.directoryStore.upsert(directory.id, directory);
  }

  deleted(dirId: string) {
    this.directoryStore.remove(dirId);
    this.directoryStore.ui.remove(dirId);
  }

  toggleIsExpanded(directoryUI: DirectoryUI) {
    this.directoryStore.ui.upsert(directoryUI.id, (d) => ({
      isExpanded: !d.isExpanded,
    }));
  }

  toggleIsFilesExpanded(directoryUI: DirectoryUI) {
    this.directoryStore.ui.upsert(directoryUI.id, (d) => ({
      isFilesExpanded: !d.isFilesExpanded,
    }));
  }

  toggleIsWorkspacesExpanded(directoryUI: DirectoryUI) {
    this.directoryStore.ui.upsert(directoryUI.id, (d) => ({
      isWorkspacesExpanded: !d.isWorkspacesExpanded,
    }));
  }

  toggleIsDirectoriesExpanded(directoryUI: DirectoryUI) {
    this.directoryStore.ui.upsert(directoryUI.id, (d) => ({
      isDirectoriesExpanded: !d.isDirectoriesExpanded,
    }));
  }

  export(
    id: string,
    archiveType: ArchiveType,
    includeIds: boolean
  ): Observable<FileDownload> {
    return this.directoriesService
      .exportDirectory(id, archiveType, includeIds, 'response')
      .pipe(
        map((response) => {
          return {
            blob: response.body,
            filename: HttpHeaderUtils.getFilename(response.headers),
          };
        })
      );
  }
}
