/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource, MatPaginator, PageEvent, MatSort } from '@angular/material';
import { Result, ResultService } from 'src/app/swagger-codegen/dispatcher.api';

@Component({
  selector: 'app-task-history',
  templateUrl: './task-history.component.html',
  styleUrls: ['./task-history.component.css']
})
export class TaskHistoryComponent implements OnInit {
  public displayedColumns: string[] = [
    'status',
    'vmName',
    'inputString',
    'expectedOutput',
    'actualOutput',
    'dateCreated',
    'id'
  ];
  public modelDataSource = new MatTableDataSource<Result>(new Array<Result>());

  // MatPaginator Output
  public defaultPageSize = 5;
  public pageEvent: PageEvent;
  public loading = false;
  public apiResponded = false;

  public historyView = 'User';
  public historyViewSub = 'Me';
  public filterValue = undefined;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private resultService: ResultService) {
    this.loading = true;
    this.apiResponded = false;
    this.resultService.getResults().subscribe(
      res => {
        this.apiResponded = true;
        if (res != null) {
          this.modelDataSource.data = res;
        }
      },
      error => {
        console.log(
          'API (' + this.resultService.configuration.basePath + ') is not responding:',
          error.message
        );
      }
    );
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
}

