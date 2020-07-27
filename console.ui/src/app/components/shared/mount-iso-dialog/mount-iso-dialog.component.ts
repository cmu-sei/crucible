/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import {
  Component,
  Inject,
  ChangeDetectionStrategy,
  Input,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
} from '@angular/core';
import { IsoResult, IsoFile } from '../../../models/vm/iso-result';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'mount-iso-dialog',
  templateUrl: './mount-iso-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MountIsoDialogComponent implements OnInit, OnDestroy {
  public data: any;
  public isoResults: IsoResult[];

  @Input()
  set isoResult(val: IsoResult[]) {
    this.isoResults = val;
    this.isoResults.forEach((x) => {
      x.display = this.applyFilter(x.isos);
      x.hide = false;

      x.teamIsoResults.forEach((y) => {
        y.display = this.applyFilter(y.isos);
        y.hide = false;
      });
    });
  }

  public selectedIso: any;

  private filterValue = '';
  private searchSubject$ = new Subject<string>();
  private unsubscribe$ = new Subject();

  constructor(
    private dialogRef: MatDialogRef<MountIsoDialogComponent>,
    private changeDetectorRef: ChangeDetectorRef
  ) {
    this.dialogRef.disableClose = true;
  }

  ngOnInit() {
    this.searchSubject$
      .pipe(
        debounceTime(200),
        distinctUntilChanged(),
        takeUntil(this.unsubscribe$)
      )
      .subscribe((searchTextValue) => {
        this.setFilter(searchTextValue);
      });
  }

  onSearch(searchTextValue: string) {
    this.searchSubject$.next(searchTextValue);
  }

  selectThisIso(iso: string) {
    this.selectedIso = iso;
  }

  setFilter(filterValue: string) {
    this.filterValue = filterValue.trim().toLowerCase();

    this.isoResults.forEach((x) => {
      if (!x.hide) {
        x.display = this.applyFilter(x.isos);
      }

      x.teamIsoResults.forEach((y) => {
        if (!y.hide) {
          y.display = this.applyFilter(y.isos);
        }
      });
    });

    this.changeDetectorRef.markForCheck();
  }

  applyFilter(isos: IsoFile[]): IsoFile[] {
    return isos.filter((x) =>
      x.filename.toLowerCase().includes(this.filterValue)
    );
  }

  close() {
    this.dialogRef.close({});
  }

  done() {
    this.dialogRef.close(this.selectedIso);
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}
