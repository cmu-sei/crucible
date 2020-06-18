/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, ViewChild, AfterViewInit, Output, EventEmitter,  } from '@angular/core';
import { VmService } from '../../services/vm/vm.service';
import { VmModel } from '../../models/vm-model';
import { MatTableDataSource, MatPaginator, PageEvent } from '@angular/material';
import { FileService } from '../../services/file/file.service';
import { DialogService } from '../../services/dialog/dialog.service';
import { TeamsService } from '../../services/teams/teams.service';
import { HttpEventType } from '@angular/common/http';

@Component({
  selector: 'app-vm-list',
  templateUrl: './vm-list.component.html',
  styleUrls: ['./vm-list.component.css']
})
export class VmListComponent implements OnInit, AfterViewInit {
  public vmModelDataSource: any;
  public displayedColumns: string[] = ['name'];

  // MatPaginator Output
  public defaultPageSize = 50;
  public pageEvent: PageEvent;
  public uploading = false;
  public uploadProgress = 0;
  public vmApiResponded = true;
  public filterString = '';

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @Output() openVmHere = new EventEmitter<{[name: string]: string}>();

  constructor(
    public vmService: VmService,
    private fileService: FileService,
    private dialogService: DialogService,
    private teamsService: TeamsService) { }

  ngOnInit() {
    this.pageEvent = new PageEvent();
    this.pageEvent.pageIndex = 0;
    this.pageEvent.pageSize = this.defaultPageSize;
    this.vmModelDataSource = new MatTableDataSource<VmModel>(new Array<VmModel>());


    // Create a filterPredicate that tells the Search to ONLY search on the name column
    this.vmModelDataSource.filterPredicate =
      (data: VmModel, filters: string) => {
        const matchFilter = [];
        const filterArray = filters.split(' ');
        const columns = [data.name];
        // Or if you don't want to specify specifics columns =>
        // const columns = (<any>Object).values(data);
        // Main loop
        filterArray.forEach(filter => {
          const customFilter = [];
          columns.forEach(column => customFilter.push(column.toLowerCase().includes(filter)));
          matchFilter.push(customFilter.some(Boolean)); // OR
        });
        return matchFilter.every(Boolean); // AND
      };

    this.vmService.GetViewVms(true, false).subscribe(res => {
      this.vmApiResponded = true;
      if (res != null) {
        res.forEach(vm => {
          vm.url = vm.url + '?viewId=' + this.vmService.viewId;
          vm.state = 'on';
        });

        this.vmModelDataSource.data = res;
      }
    },
    error => {
      console.log('The VM API is not responding.  ' + error.message);
      this.vmApiResponded = false;
    });
  }

  ngAfterViewInit() {
    this.vmModelDataSource.paginator = this.paginator;
  }


  /*onPage(pageEvnt) {
    this.pageEvent = PageEvent;
  }*/

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

  openHere(vmName: string, url: string) {
    const val = <{[name: string]: string}>{ name: vmName, url};
    this.openVmHere.emit(val);
  }

  uploadIso(fileSelector) {
    if (fileSelector.value === '') {
      console.log('file selector did not have a value');
      return;
    }



    let isAdmin = true;
    this.teamsService.GetAllMyTeams(this.vmService.viewId).subscribe(teams => {
      // There should only be 1 primary member, set that value for the current login
      // Determine if the user is an "Admin" if their isPrimary team has canManage == true
      const myPrimaryTeam = teams.filter(t => t.isPrimary)[0];
      if (myPrimaryTeam !== undefined) {
        isAdmin = myPrimaryTeam.canManage;
      } else {
        isAdmin = false;
        console.log('User does not have a primary team');
      }

      const qf = fileSelector.files[0];

      if (isAdmin) {
        // First prompt the user to confirm if the iso is available for the team or the entire view
        this.dialogService.confirm('Upload iso for?',
          'Please choose if you want this iso to be public or for your team only:',
          { buttonTrueText: 'Public', buttonFalseText: 'My Team Only' }).subscribe(result => {
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
    this.fileService.uploadIso(isForAll, file)
      .subscribe(
        event => {
          if (event.type === HttpEventType.UploadProgress) {
            const percentDone = Math.round(100 * event.loaded / event.total);
            this.uploadProgress = percentDone;
          }

          if (event.type === HttpEventType.Response) {
            this.uploading = false;
          }
        },
        err => {
          console.log(err);
          this.uploading = false;
        }
    );
  }

}
