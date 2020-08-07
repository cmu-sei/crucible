/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { MatTableDataSource, MatPaginator, PageEvent, MatSort } from '@angular/material';
import { Result, User, View, Vm } from 'src/app/swagger-codegen/dispatcher.api';
import { ResultQuery } from 'src/app/data/result/result.query';
import { ResultDataService } from 'src/app/data/result/result-data.service';
import { TaskDataService } from 'src/app/data/task/task-data.service';
import { UserDataService } from 'src/app/data/user/user-data.service';
import { PlayerDataService } from 'src/app/data/player/player-data-service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

enum HistoryView {
  user = 'User',
  view = 'View',
  vm = 'VM'
}

@Component({
  selector: 'app-history',
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({ height: '0px', minHeight: '0' })),
      state('expanded', style({ height: '*' })),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],})

export class HistoryComponent implements OnInit, OnDestroy {
  HistoryView = HistoryView;
  displayedColumns: string[] = [
    'id',
    'statusDate',
    'vmName',
    'status',
    'actualOutput',
    'expectedOutput'
  ];
  modelDataSource = new MatTableDataSource<Result>(new Array<Result>());
  // MatPaginator Output
  defaultPageSize = 10;
  pageEvent: PageEvent;
  loading = false;
  apiResponded = false;
  historyView = HistoryView.user;
  filterValue = undefined;
  userList: User[] = [];
  selectedUser: User;
  viewList: View[] = [];
  selectedView: View;
  vmList: Vm[] = [];
  selectedVm: Vm;
  expandedResult: Result;
  private unsubscribe$ = new Subject();
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  constructor(
    private resultQuery: ResultQuery,
    private taskDataService: TaskDataService,
    private userDataService: UserDataService,
    private playerDataService: PlayerDataService,
    private resultDataService: ResultDataService
  ) {
    this.loading = true;
    this.apiResponded = false;

    this.resultQuery.selectAll().pipe(takeUntil(this.unsubscribe$)).subscribe(
      res => {
        this.apiResponded = true;
        if (res != null) {
          this.modelDataSource.data = res.sort((a: Result, b: Result) => a.statusDate <= b.statusDate ? 1 : -1);
        }
      },
      error => {
        console.log(
          'API is not responding:',
          error.message
        );
      }
    );
    this.userDataService.users.pipe(takeUntil(this.unsubscribe$)).subscribe(users => {
      this.userList = !!users && users.length > 0 ? users.sort((a: User, b: User) => a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1) : [];
      const loggedInUser = this.userDataService.loggedInUser.getValue();
      this.selectedUser = this.userList.find(u => u.id === loggedInUser.profile.sub);
      if (this.historyView === HistoryView.user) {
        this.handleUserChange(this.selectedUser);
      }
    });
    this.userDataService.getUsersFromApi();
    this.playerDataService.viewList.pipe(takeUntil(this.unsubscribe$)).subscribe(views => {
      this.viewList = !!views && views.length > 0 ? views.sort((a: User, b: User) => a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1) : [];
    });
    this.playerDataService.vms.pipe(takeUntil(this.unsubscribe$)).subscribe(vms => {
      this.vmList = !!vms && vms.length > 0 ? vms.sort((a: User, b: User) => a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1) : [];
    });
    this.playerDataService.selectView('');
  }

  ngOnInit() {
    this.modelDataSource.paginator = this.paginator;
    this.modelDataSource.sort = this.sort;

    this.pageEvent = new PageEvent();
    this.pageEvent.pageIndex = 0;
    this.pageEvent.pageSize = this.defaultPageSize;
  }

  applyFilter(filterValue: string) {
    this.pageEvent.pageIndex = 0;
    this.modelDataSource.filter = filterValue.trim().toLowerCase();
  }

  handleHistoryViewChange(historyView: HistoryView) {
    switch (historyView) {
      case HistoryView.user:
        this.handleUserChange(this.selectedUser);
        break;
      case HistoryView.view:
        this.handleViewChange(this.selectedView);
        break;
      case HistoryView.vm:
        this.playerDataService.getAllVmsFromApi();
        this.handleVmChange(this.selectedVm);
        break;
      default:
        this.vmList = [];
        break;
    }
  }

  handleUserChange(user: User) {
    this.selectedUser = user;
    if (!user || !user.id) {
      this.modelDataSource.data = [];
    } else {
      this.resultDataService.loadByUser(user.id);
    }
  }

  handleViewChange(view: View) {
    this.selectedView = view;
    if (!view || !view.id) {
      this.modelDataSource.data = [];
    } else {
      this.resultDataService.loadByView(view.id);
    }
  }

  handleVmChange(vm: Vm) {
    this.selectedVm = vm;
    if (!vm || !vm.id) {
      this.modelDataSource.data = [];
    } else {
      this.resultDataService.loadByVm(vm.id);
    }
  }

  copyTask(resultId: string) {
    this.taskDataService.setClipboard({ id: undefined, resultId: resultId, isCut: false });
  }

  showDetail(result: Result) {
    this.expandedResult = this.expandedResult === result ? null : result;
  }

  ngOnDestroy() {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

}

