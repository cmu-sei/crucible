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
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { View, ViewService } from '../../../generated/s3.player.api';
import { LoggedInUserService } from '../../../services/logged-in-user/logged-in-user.service';
import { DialogService } from '../../../services/dialog/dialog.service';
import { AdminViewEditComponent } from './admin-view-edit/admin-view-edit.component';

export interface Action {
  Value: string;
  Text: string;
}

@Component({
  selector: 'app-admin-view-search',
  templateUrl: './admin-view-search.component.html',
  styleUrls: ['./admin-view-search.component.scss'],
})
export class AdminViewSearchComponent implements OnInit {
  @ViewChild(AdminViewEditComponent, { static: true })
  adminViewEditComponent: AdminViewEditComponent;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  public viewActions: Action[] = [
    { Value: 'edit', Text: 'Edit View' },
    { Value: 'activate', Text: 'Activate/Deactivate View' },
  ];

  public viewDataSource: MatTableDataSource<View>;
  public displayedColumns: string[] = ['name', 'description', 'status'];
  public filterString: string;
  public showEditScreen: Boolean;
  public isLoading: Boolean;

  constructor(
    private viewService: ViewService,
    public loggedInUserService: LoggedInUserService,
    public dialogService: DialogService
  ) {}

  /**
   * Initialization
   */
  ngOnInit() {
    // Initial datasource
    this.viewDataSource = new MatTableDataSource<View>(new Array<View>());
    this.sort.sort(<MatSortable>{ id: 'name', start: 'asc' });
    this.viewDataSource.sort = this.sort;
    this.showEditScreen = false;
    this.filterString = '';

    // Initial datasource
    this.filterString = '';

    this.loggedInUserService.loggedInUser.subscribe((user) => {
      this.refreshViews();
    });
  }

  /**
   * Executes an action menu item
   * @param action: action string to case from
   * @param viewGuid: The guid for view
   */
  executeViewAction(action: string, viewGuid: string) {
    switch (action) {
      case 'edit': {
        // Edit view
        this.viewService.getView(viewGuid).subscribe((view) => {
          this.adminViewEditComponent.resetStepper();
          this.adminViewEditComponent.updateView();
          this.adminViewEditComponent.updateApplicationTemplates();
          this.adminViewEditComponent.view = view;
          this.showEditScreen = true;
        });
        break;
      }
      case 'activate': {
        // Activate or Deactivate
        this.viewService.getView(viewGuid).subscribe((view) => {
          let msg = '';
          let title = '';
          let activation = View.StatusEnum.Inactive;
          if (
            view.status === undefined ||
            view.status === View.StatusEnum.Inactive
          ) {
            msg = 'Do you wish to Activate view ' + view.name + '?';
            title = 'Activate View?';
            activation = View.StatusEnum.Active;
          } else {
            msg = 'Do you wish to deactivate view ' + view.name + '?';
            title = 'Deactivate View?';
            activation = View.StatusEnum.Inactive;
          }
          this.dialogService.confirm(title, msg).subscribe((result) => {
            if (result['confirm']) {
              view.status = activation;
              this.viewService
                .updateView(viewGuid, view)
                .subscribe((updateview) => {
                  console.log('successfully updated view ' + updateview.name);
                  this.refreshViews();
                });
            }
          });
        });
        break;
      }
      default: {
        alert('Unknown Action');
        break;
      }
    }
  }

  /**
   * Adds a new view
   */
  addNewView() {
    const view = <View>{
      name: 'New View',
      description: 'Add description',
      status: View.StatusEnum.Active,
    };
    this.viewService.createView(view).subscribe((ex) => {
      this.refreshViews();
      this.executeViewAction('edit', ex.id);
    });
  }

  /**
   * Called by UI to add a filter to the viewDataSource
   * @param filterValue
   */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.viewDataSource.filter = filterValue;
  }

  /**
   * Updated the datasource for the view search table
   */
  refreshViews() {
    this.showEditScreen = false;
    this.isLoading = true;
    this.viewService.getViews().subscribe((views) => {
      this.viewDataSource.data = views;
      this.isLoading = false;
    });
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }
}
