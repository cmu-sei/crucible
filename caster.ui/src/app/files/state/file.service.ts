/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { FileStore } from './file.store';
import { FilesService, ModelFile } from '../../generated/caster-api';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { FileQuery } from './file.query';
import HttpHeaderUtils from 'src/app/shared/utilities/http-header-utils';
import { FileDownload } from 'src/app/shared/models/file-download';

@Injectable({
  providedIn: 'root',
})
export class FileService {
  constructor(
    private fileStore: FileStore,
    private fileQuery: FileQuery,
    private filesService: FilesService
  ) {}

  loadFile(fileId: string): Observable<ModelFile> {
    return this.filesService.getFile(fileId).pipe(
      tap((file) => {
        file.editorContent = file.content;
        this.fileStore.upsert(file.id, file);
        this.setSave(fileId, true);
      })
    );
  }

  loadFilesByDirectory(directoryId: string): Observable<ModelFile[]> {
    return this.filesService.getFilesByDirectory(directoryId, false).pipe(
      tap((files) => {
        files.forEach((file) => {
          this.upsertFile(file);
          this.setSave(file.id, true);
        });
      })
    );
  }

  setFiles(files: ModelFile[]) {
    this.fileStore.set(files);
  }

  add(file: ModelFile) {
    this.filesService.createFile(file).subscribe((f) => {
      f.editorContent = f.content;
      this.fileStore.add(f);
    });
  }

  updateFile(file: ModelFile) {
    this.filesService.editFile(file.id, file).subscribe((f) => {
      this.fileUpdated(f);
    });
  }

  updateFileContent(fileId: string, fileContent: string): Observable<any> {
    return this.filesService
      .partialEditFile(fileId, { content: fileContent })
      .pipe(
        tap((f) => {
          this.fileUpdated(f);
        })
      );
  }

  renameFile(fileId: string, newName: string) {
    this.filesService.renameFile(fileId, { name: newName }).subscribe((f) => {
      this.fileStore.update(f.id, (entity) => {
        return {
          ...entity,
          name: f.name,
        };
      });
    });
  }

  lockFile(fileId: string) {
    this.filesService.lockFile(fileId).subscribe((f) => {
      this.fileUpdated(f);
    });
  }

  unlockFile(fileId: string) {
    this.filesService.unlockFile(fileId).subscribe((f) => {
      this.fileUpdated(f);
    });
  }

  adminLockFile(fileId: string) {
    this.filesService.administrativelyLockFile(fileId).subscribe((f) => {
      this.fileUpdated(f);
    });
  }

  adminUnlockFile(fileId: string) {
    this.filesService.administrativelyUnlockFile(fileId).subscribe((f) => {
      this.fileUpdated(f);
    });
  }

  fileUpdated(file: ModelFile) {
    this.upsertFile(file);
    this.setSave(file.id, true);
  }

  filesUpdated(files: ModelFile[]) {
    files.forEach((file) => {
      this.upsertFile(file);
      this.setSave(file.id, true);
    });
  }

  private upsertFile(file: ModelFile) {
    this.fileStore.upsert(file.id, (entity) => {
      return {
        ...entity,
        ...file,
        content: file.content === null ? entity.content : file.content,
        editorContent:
          file.content === null ? entity.editorContent : file.content,
      };
    });
  }

  updateEditorContent(fileId: string, editorContent: string) {
    const file = this.fileQuery.getEntity(fileId);
    const newFile = { ...file, editorContent } as ModelFile;
    this.fileStore.update(fileId, newFile);
    this.setSave(fileId, false);
  }

  delete(file: ModelFile) {
    this.filesService.deleteFile(file.id).subscribe(() => {
      this.fileDeleted(file.id);
    });
  }

  fileDeleted(fileId: string) {
    this.fileStore.remove(fileId);
  }

  setActive(file: ModelFile | null) {
    // if id is null or undefined, a file or folder is active
    if (!file) {
      this.fileStore.setActive(null);
    } else {
      this.fileStore.setActive(file.id);
    }
  }

  setSave(id: string, value: boolean): void {
    this.fileStore.ui.upsert(id, { isSaved: value });
  }

  setSelectedVersionId(fileId: string, versionId: string) {
    this.fileStore.ui.upsert(fileId, (entity) => ({
      selectedVersionId: versionId,
    }));
  }

  export(id: string): Observable<FileDownload> {
    return this.filesService.exportFile(id, 'response').pipe(
      map((response) => {
        return {
          blob: response.body,
          filename: HttpHeaderUtils.getFilename(response.headers),
        };
      })
    );
  }
}
