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
import { MatTableDataSource, MatSort, MatSortable } from '@angular/material';

import { DefinitionService } from '../../../generated/alloy.api/api/definition.service';
import { Definition } from '../../../generated/alloy.api/model/definition';




@Component({
  selector: 'app-event-list',
  templateUrl: './event-list.component.html',
  styleUrls: ['./event-list.component.css']
})
export class EventListComponent implements OnInit {

  @ViewChild(MatSort, {static: true}) sort: MatSort;

  public eventsDataSource: MatTableDataSource<Definition>;
  public displayedColumns: string[] = ['name', 'description', 'durationHours'];

  public filterString: string;
  public isLoading: Boolean;

  constructor(
    private eventTemplateService: DefinitionService
  ) {
    this.eventsDataSource = new MatTableDataSource<Definition>(new Array<Definition>());
  }

  ngOnInit() {
    this.filterString = '';

    // Initial datasource
    this.eventTemplateService.getDefinitions().subscribe(defs => {
      this.eventsDataSource.data = defs;
      this.sort.sort(<MatSortable>({id: 'name', start: 'asc'}));
      this.eventsDataSource.sort = this.sort;
    });

    // Subscribe to the service
    this.isLoading = false;


  }


  /**
   * Called by UI to add a filter to the DataSource
   * filterValue
   */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.eventsDataSource.filter = filterValue;
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }


  openEvent(id: string) {
    window.location.href = '/events/' + id;
  }

}
