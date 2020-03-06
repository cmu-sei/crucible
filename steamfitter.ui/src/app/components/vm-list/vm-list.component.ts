/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
// import { VmService } from '../../services/vm/vm.service';
// import { VmModel } from '../../models/vm-model';
import { MatTableDataSource, MatPaginator, PageEvent } from '@angular/material';
// import { FileService } from '../../services/file/file.service';
// import { DialogService } from '../../services/dialog/dialog.service';
// import { TeamsService } from '../../services/teams/teams.service';
import { HttpEventType } from '@angular/common/http';
import { Exercise, PlayerService, Vm } from '../../swagger-codegen/dispatcher.api';
import { NewDispatchTaskService } from '../../services/new-dispatch-task/new-dispatch-task.service';

@Component({
  selector: 'app-vm-list',
  templateUrl: './vm-list.component.html',
  styleUrls: ['./vm-list.component.css']
})
export class VmListComponent implements OnInit, AfterViewInit {
  public vmModelDataSource: any;
  public displayedColumns: string[] = ['name'];
  public exercises: Exercise[];
  public selectedExercise: Exercise;

  // MatPaginator Output
  public defaultPageSize = 10;
  public pageEvent: PageEvent;
  public uploading = false;
  public uploadProgress = 0;
  public vmApiResponded = true;

  @ViewChild(MatPaginator) paginator: MatPaginator;

  constructor(
    private playerService: PlayerService,
    private newDispatchTaskService: NewDispatchTaskService
  ) { }

  ngOnInit() {
    this.pageEvent = new PageEvent();
    this.pageEvent.pageIndex = 0;
    this.pageEvent.pageSize = this.defaultPageSize;
    this.vmModelDataSource = new MatTableDataSource<Vm>(new Array<Vm>());

    // Create a filterPredicate that tells the Search to ONLY search on the name column
    this.vmModelDataSource.filterPredicate =
      (data: Vm, filters: string) => {
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

      // get exercises available to this user
      this.playerService.getExercises().subscribe(exercises => {
        this.exercises = exercises.sort((x1, x2) => {
          return (x1.name > x2.name) ? 1 : ((x1.name < x2.name) ? -1 : 0);
        });
      },
      error => {
        console.log('The Player API is not responding.  ' + error.message);
      });

  }

  onExerciseChange() {
    this.newDispatchTaskService.vmList.next([]);
    this.playerService.getVms(this.selectedExercise.id).subscribe(res => {
      this.vmApiResponded = true;
      if (res != null) {
        res.forEach(vm => {
          vm.url = vm.url + '?exerciseId=' + this.selectedExercise.id;
        });
        console.log(res);
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


  /**
 * Called by UI to add a filter to the vmModelDataSource
 */
  applyFilter(filterValue: string) {
    this.pageEvent.pageIndex = 0;
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.vmModelDataSource.filter = filterValue;
  }


  // Local Component functions
  openInTab(url: string) {
    window.open(url, '_blank');
  }

  openHere(url: string) {
    window.location.href = url;
  }

  onCheckBoxChange(event: any, vm: Vm) {
    if (event.checked) {
      this.newDispatchTaskService.AddVm(vm);
    } else {
      this.newDispatchTaskService.RemoveVm(vm);
    }
  }

}

