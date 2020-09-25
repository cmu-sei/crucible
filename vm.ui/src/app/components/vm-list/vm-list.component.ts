/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { HttpEventType } from '@angular/common/http';
import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { ComnAuthQuery, Theme } from '@crucible/common';
import { SelectContainerComponent } from 'ngx-drag-to-select';
import { Observable } from 'rxjs';
import { filter, switchMap, take } from 'rxjs/operators';
import { DialogService } from '../../services/dialog/dialog.service';
import { FileService } from '../../services/file/file.service';
import { TeamsService } from '../../services/teams/teams.service';
import { VmModel } from '../../vms/state/vm.model';
import { VmService } from '../../vms/state/vms.service';

@Component({
  selector: 'app-vm-list',
  templateUrl: './vm-list.component.html',
  styleUrls: ['./vm-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VmListComponent implements OnInit, AfterViewInit {
  public vmModelDataSource = new MatTableDataSource<VmModel>(
    new Array<VmModel>()
  );
  public displayedColumns: string[] = ['name'];

  // MatPaginator Output
  public defaultPageSize = 50;
  public pageEvent: PageEvent;
  public uploading = false;
  public uploadProgress = 0;
  public vmApiResponded = true;
  public filterString = '';
  public showIps = false;
  public ipv4Only = true;
  public selectedVms = new Array<string>();
  public theme$: Observable<Theme>;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(SelectContainerComponent)
  selectContainer: SelectContainerComponent;
  @Output() openVmHere = new EventEmitter<{ [name: string]: string }>();
  @Output() errors = new EventEmitter<{ [key: string]: string }>();

  @Input() set vms(val: VmModel[]) {
    this.vmModelDataSource.data = val;
  }

  constructor(
    public vmService: VmService,
    private fileService: FileService,
    private dialogService: DialogService,
    private teamsService: TeamsService,
    private authQuery: ComnAuthQuery
  ) {
    this.theme$ = authQuery.userTheme$;
  }

  ngOnInit() {
    this.pageEvent = new PageEvent();
    this.pageEvent.pageIndex = 0;
    this.pageEvent.pageSize = this.defaultPageSize;

    // Create a filterPredicate that tells the Search to ONLY search on the name column
    this.vmModelDataSource.filterPredicate = (
      data: VmModel,
      filters: string
    ) => {
      const matchFilter = [];
      const filterArray = filters.split(' ');
      const columns = [data.name];
      // Or if you don't want to specify specifics columns =>
      // const columns = (<any>Object).values(data);
      // Main loop
      filterArray.forEach((f) => {
        const customFilter = [];
        columns.forEach((column) =>
          customFilter.push(column.toLowerCase().includes(f))
        );

        data.ipAddresses.forEach((address) =>
          customFilter.push(address.includes(f))
        );

        matchFilter.push(customFilter.some(Boolean)); // OR
      });
      return matchFilter.every(Boolean); // AND
    };

    this.vmService
      .GetViewVms(true, false)
      .pipe(take(1))
      .subscribe(
        () => {
          this.vmApiResponded = true;
        },
        (error) => {
          console.log('The VM API is not responding.  ' + error.message);
          this.vmApiResponded = false;
        }
      );
  }

  ngAfterViewInit() {
    this.vmModelDataSource.paginator = this.paginator;
  }

  onPage(pageEvent) {
    this.pageEvent = pageEvent;
    this.selectContainer.clearSelection();
  }

  /**
   * Called by UI to add a filter to the vmModelDataSource
   * @param filterValue
   */
  applyFilter(filterValue: string) {
    this.pageEvent.pageIndex = 0;
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.filterString = filterValue;
    this.vmModelDataSource.filter = filterValue;
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

  // Local Component functions
  openInTab(url: string) {
    window.open(url, '_blank');
  }

  openHere($event, vmName: string, url: string) {
    $event.preventDefault();
    const val = <{ [name: string]: string }>{ name: vmName, url };
    this.openVmHere.emit(val);
  }

  uploadIso(fileSelector) {
    if (fileSelector.value === '') {
      console.log('file selector did not have a value');
      return;
    }

    let isAdmin = true;
    this.teamsService
      .GetAllMyTeams(this.vmService.viewId)
      .pipe(take(1))
      .subscribe((teams) => {
        // There should only be 1 primary member, set that value for the current login
        // Determine if the user is an "Admin" if their isPrimary team has canManage == true
        const myPrimaryTeam = teams.filter((t) => t.isPrimary)[0];
        if (myPrimaryTeam !== undefined) {
          isAdmin = myPrimaryTeam.canManage;
        } else {
          isAdmin = false;
          console.log('User does not have a primary team');
        }

        const qf = fileSelector.files[0];

        if (isAdmin) {
          // First prompt the user to confirm if the iso is available for the team or the entire view
          this.dialogService
            .confirm(
              'Upload iso for?',
              'Please choose if you want this iso to be public or for your team only:',
              { buttonTrueText: 'Public', buttonFalseText: 'My Team Only' }
            )
            .pipe(take(1))
            .subscribe((result) => {
              if (result['wasCancelled'] === false) {
                const isForAll = result['confirm'];
                this.sendIsoFile(isForAll, qf);
              }
            });
        } else {
          // The user is not an admin therfore iso's are only uploaded for the team
          this.sendIsoFile(false, qf);
        }
        fileSelector.value = '';
      });
  }

  sendIsoFile(isForAll: boolean, file: File) {
    this.uploading = true;
    this.fileService.uploadIso(isForAll, file).subscribe(
      (event) => {
        if (event.type === HttpEventType.UploadProgress) {
          const percentDone = Math.round((100 * event.loaded) / event.total);
          this.uploadProgress = percentDone;
        }

        if (event.type === HttpEventType.Response) {
          this.uploading = false;
        }
      },
      (err) => {
        console.log(err);
        this.uploading = false;
      }
    );
  }

  public getIpAddresses(vm: VmModel): string[] {
    if (vm.ipAddresses == null) {
      return [];
    }

    if (this.ipv4Only) {
      return vm.ipAddresses.filter((x) => !x.includes(':'));
    } else {
      return vm.ipAddresses;
    }
  }

  public powerOffSelected() {
    this.performAction(VmAction.PowerOff, 'Power Off', 'power off');
  }

  public powerOnSelected() {
    this.performAction(VmAction.PowerOn, 'Power On', 'power on');
  }

  public shutdownSelected() {
    this.performAction(VmAction.Shutdown, 'Shutdown', 'shutdown');
  }

  private performAction(action: VmAction, title: string, actionName: string) {
    this.dialogService
      .confirm(
        `${title}`,
        `Are you sure you want to ${actionName} ${this.selectedVms.length} selected machines?`,
        { buttonTrueText: 'Confirm' }
      )
      .pipe(
        filter((result) => result.wasCancelled === false),
        switchMap(() => {
          this.errors.emit({});

          switch (action) {
            case VmAction.PowerOff:
              return this.vmService.powerOff(this.selectedVms);
            case VmAction.PowerOn:
              return this.vmService.powerOn(this.selectedVms);
            case VmAction.Shutdown:
              return this.vmService.shutdown(this.selectedVms);
          }
        }),
        take(1)
      )
      .subscribe((x) => {
        this.errors.emit(x.errors);
      });
  }

  public trackByVmId(item) {
    return item.id;
  }
}

enum VmAction {
  PowerOn,
  PowerOff,
  Shutdown,
}
