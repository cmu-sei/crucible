/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { ApplicationTemplate } from '../../../swagger-codegen/s3.player.api';
import { MatTableDataSource, PageEvent, MatPaginator, MatSort, MatSortable } from '@angular/material';
import { ApplicationService } from '../../../swagger-codegen/s3.player.api/api/application.service';

export interface Action {
  Value: string;
  Text: string;
}

@Component({
  selector: 'app-admin-app-template-search',
  templateUrl: './admin-app-template-search.component.html',
  styleUrls: ['./admin-app-template-search.component.css']
})
export class AdminAppTemplateSearchComponent implements OnInit, AfterViewInit {

  public appTemplateDataSource: MatTableDataSource<ApplicationTemplate>;
  public appTemplateColumns: string[] = ['name', 'url'];
  public filterString: string;
  public currentAppTemplate: ApplicationTemplate;

  // MatPaginator Output
  public defaultPageSize = 10;
  public pageEvent: PageEvent;
  public uploading = false;
  public uploadProgress = 0;

  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor(private applicationService: ApplicationService) { }

  /**
   * Initialization
   */
  ngOnInit() {

    this.pageEvent = new PageEvent();
    this.pageEvent.pageIndex = 0;
    this.pageEvent.pageSize = this.defaultPageSize;

    // Initial datasource
    this.appTemplateDataSource = new MatTableDataSource<ApplicationTemplate>(new Array<ApplicationTemplate>());
    this.sort.sort(<MatSortable>({id: 'name', start: 'asc'}));
    this.appTemplateDataSource.sort = this.sort;
    this.filterString = '';
    this.refresh(false);
  }

  /**
   * Called after the components initialized
   */
  ngAfterViewInit() {
    this.appTemplateDataSource.paginator = this.paginator;
  }


  /**
   * Updates the current list of application templates
   */
  refresh(wasDeleted: boolean) {
    this.applicationService.getApplicationTemplates().subscribe(appTemplates => {
      if (wasDeleted) {
        this.currentAppTemplate = undefined;
      }
      this.appTemplateDataSource.data = appTemplates;
    });
  }


  /**
   * Add a new application template
   */
  addAppTemplate() {
    const newAppTemplate: ApplicationTemplate = {
      name: 'New Template',
      url: 'http://localhost',
      embeddable: true,
      icon: '/assets/img/exercise-player.png',
      loadInBackground: false
    };
    this.applicationService.createApplicationTemplate(newAppTemplate).subscribe(newApp => {
      this.paginator.lastPage();
      this.applicationService.getApplicationTemplates().subscribe(appTemplates => {
        this.appTemplateDataSource.data = appTemplates;
        this.currentAppTemplate = newApp;
      });
    });
  }


  /**
   * Called by UI to add a filter to the appTemplateDataSource
   * @param filterValue
   */
  applyFilter(filterValue: string) {
    this.currentAppTemplate = undefined;
    this.filterString = filterValue;
    this.pageEvent.pageIndex = 0;
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.appTemplateDataSource.filter = filterValue;
  }


  /**
   * Clears the application template filter string
   */
  clearFilter() {
    this.applyFilter('');
  }

}

