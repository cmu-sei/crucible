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
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  ViewEncapsulation,
} from '@angular/core';
import { FileVersion } from '../../../generated/caster-api';
import { MatTableDataSource } from '@angular/material/table';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FileVersionQuery } from 'src/app/fileVersions/state';

@Component({
  selector: 'cas-version-list',
  templateUrl: './version-list.component.html',
  styleUrls: ['./version-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class VersionListComponent implements OnInit, OnDestroy {
  @Input() fileId: string;
  @Input() selectedVersionId: string;
  @Output() getVersion: EventEmitter<{ id: string }> = new EventEmitter();
  @Output() revertToVersion: EventEmitter<{
    fileVersion: FileVersion;
  }> = new EventEmitter();
  public versions: FileVersion[];
  public dataSource = new MatTableDataSource();
  public filterString = '';
  private unsubscribe$ = new Subject();

  constructor(
    private fileVersionQuery: FileVersionQuery,
    private changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.fileVersionQuery
      .selectAll()
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((versions) => {
        if (this.fileId) {
          const fileVersions = versions.filter((v) => {
            return v.fileId === this.fileId;
          });
          this.dataSource.data = fileVersions;
          this.dataSource.data.sort((a: any, b: any) => {
            if (a.dateSaved < b.dateSaved) {
              return 1;
            } else if (a.dateSaved > b.dateSaved) {
              return -1;
            } else {
              return 0;
            }
          });
          this.changeDetectorRef.markForCheck();
        }
      });
  }

  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.dataSource.filter = filterValue;
  }

  clearFilter() {
    this.applyFilter('');
  }

  selectVersionFn(version: FileVersion) {
    this.getVersion.emit({ id: version.id });
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

  revertFile(version: FileVersion) {
    this.revertToVersion.emit({ fileVersion: version });
  }
}
