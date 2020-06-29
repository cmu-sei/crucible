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
  Component,
  OnInit,
  Inject,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { PageEvent, MatPaginator } from '@angular/material/paginator';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import {
  User,
  UserService,
  Team,
  TeamService,
  TeamMembershipService,
  TeamMembership,
} from '../../../generated/s3.player.api';
import {
  Role,
  RoleService,
  TeamMembershipForm,
  Permission,
} from '../../../generated/s3.player.api';
import { forkJoin, Observable } from 'rxjs';

/** User node with related user and application information */
export class TeamUser {
  constructor(
    public name: string,
    public user: User,
    public teamMembership: TeamMembership
  ) {}
}

@Component({
  selector: 'app-add-remove-users-dialog',
  templateUrl: './add-remove-users-dialog.component.html',
  styleUrls: ['./add-remove-users-dialog.component.scss'],
})
export class AddRemoveUsersDialogComponent implements OnInit {
  public title: string;
  public team: Team;

  public displayedUserColumns: string[] = ['name', 'id'];
  public displayedTeamColumns: string[] = ['name', 'teamMembership', 'user'];
  public userDataSource = new MatTableDataSource<User>(new Array<User>());
  public teamUserDataSource = new MatTableDataSource<TeamUser>(
    new Array<TeamUser>()
  );
  public isLoading: Boolean;
  public isBusy: Boolean;

  public filterString: string;
  public defaultPageSize = 7;
  public pageEvent: PageEvent;

  public roles: Array<Role>;

  @ViewChild('usersInput') usersInput: ElementRef<
    HTMLInputElement
  >;
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor(
    @Inject(MAT_DIALOG_DATA) data,
    private dialogRef: MatDialogRef<AddRemoveUsersDialogComponent>,
    public userService: UserService,
    public teamService: TeamService,
    public teamMembershipService: TeamMembershipService,
    public roleService: RoleService
  ) {
    this.dialogRef.disableClose = true;
    this.isLoading = false;
    this.isBusy = false;
    this.filterString = '';
  }

  /**
   * Initializes the components
   */
  ngOnInit() {
    this.sort.sort(<MatSortable>{ id: 'name', start: 'asc' });
    this.userDataSource.sort = this.sort;

    this.pageEvent = new PageEvent();
    this.pageEvent.pageIndex = 0;
    this.pageEvent.pageSize = this.defaultPageSize;

    this.roleService.getRoles().subscribe((roles) => {
      const nullRole = <Role>{
        id: '',
        name: 'None',
        permissions: new Array<Permission>(),
      };

      roles.unshift(nullRole);
      this.roles = roles;
    });
  }

  /**
   * Called by UI to add a filter to the viewDataSource
   * @param filterValue
   */
  applyFilter(filterValue: string) {
    this.filterString = filterValue;
    this.pageEvent.pageIndex = 0;
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.userDataSource.filter = filterValue;
  }

  /**
   * Clears the search string
   */
  clearFilter() {
    this.applyFilter('');
  }

  /**
   * Called to load the team before the dialog is opened
   * @param team The team to load in this dialog
   */
  loadTeam(team: Team) {
    this.team = team;

    this.isLoading = true;
    this.userService.getUsers().subscribe((allUsers) => {
      this.userService.getTeamUsers(team.id).subscribe((teamUsers) => {
        const tUsers = teamUsers.slice(0);
        tUsers.sort((a, b) => {
          return this.compare(a.name, b.name, true);
        });

        if (tUsers.length > 0) {
          const newTeamUsers = new Array<TeamUser>();
          // The following gets kind of crazy.  Because observables are non-blocking, an array
          // of all the observables is created.
          const membershipObservable = new Array<
            Observable<Array<TeamMembership>>
          >();
          tUsers.forEach((tu) => {
            membershipObservable.push(
              this.teamMembershipService.getTeamMemberships(
                this.team.viewId,
                tu.id
              )
            );
          });
          // The rxjs forJoin allows for multiple observables to be called in parallel and then processing
          // will resume after all of the observables in the array are returned.
          forkJoin(membershipObservable).subscribe(
            (tmbss: Array<Array<TeamMembership>>) => {
              // A 2 dimensional array is returned in this case because the inner observable already returns an array
              tmbss.forEach((tmbs) => {
                const tu = tmbs.find((tmb) => tmb.teamId === this.team.id); // Match by team
                const us = allUsers.find((u) => u.id === tu.userId); // Get user object from getUsers() array
                if (tu.roleId === null) {
                  tu.roleId = '';
                  tu.roleName = '';
                }
                const newTeamUser = new TeamUser(us.name, us, tu); // Create the hybrid object
                newTeamUsers.push(newTeamUser);
              });
              // Now that all of the observables are returned, process accordingly.
              this.teamUserDataSource.data = newTeamUsers;
              const newAllUsers = allUsers.slice(0);
              this.teamUserDataSource.data.forEach((tu) => {
                const index = newAllUsers.findIndex((u) => u.id === tu.user.id);
                newAllUsers.splice(index, 1);
              });
              this.userDataSource = new MatTableDataSource(newAllUsers);
              this.userDataSource.sort = this.sort;
              this.userDataSource.paginator = this.paginator;
              this.isLoading = false;
            }
          ); // forkJoin
        } else {
          // In this case, No users have been added to the team.  Therefore proceed accordingly.
          const newTeamUsers = new Array<TeamUser>();
          this.teamUserDataSource.data = newTeamUsers;
          const newAllUsers = allUsers.slice(0);
          this.teamUserDataSource.data.forEach((tu) => {
            const index = newAllUsers.findIndex((u) => u.id === tu.user.id);
            newAllUsers.splice(index, 1);
          });
          this.userDataSource = new MatTableDataSource(newAllUsers);
          this.userDataSource.sort = this.sort;
          this.userDataSource.paginator = this.paginator;
          this.isLoading = false;
        }
      }); // getTeamUsers
    }); // getUsers
  }

  /**
   * Called to close the dialog
   */
  done() {
    this.dialogRef.close({
      teamUsers: this.teamUserDataSource.data,
    });
  }

  /**
   * Call to api to add a user to team and update local array
   * @param user The user to be added
   */
  addUserToTeam(user: User): void {
    if (this.isBusy) {
      return;
    }
    const index = this.teamUserDataSource.data.findIndex(
      (u) => u.user.id === user.id
    );
    if (index === -1) {
      this.isBusy = true;
      this.userService
        .addUserToTeam(this.team.id, user.id)
        .subscribe((result) => {
          const tUsers = this.teamUserDataSource.data.slice(0);

          this.teamMembershipService
            .getTeamMemberships(this.team.viewId, user.id)
            .subscribe((tmbs) => {
              const teamMembership = tmbs.find(
                (tmb) => tmb.teamId === this.team.id
              );
              const tUser = new TeamUser(user.name, user, teamMembership);
              tUsers.push(tUser);
              tUsers.sort((a, b) => {
                return this.compare(a.user.name, b.user.name, true);
              });
              this.teamUserDataSource.data = tUsers;
              const allUsers = this.userDataSource.data.slice(0);
              const i = allUsers.findIndex((u) => u.id === user.id);
              allUsers.splice(i, 1);
              this.userDataSource = new MatTableDataSource(allUsers);
              this.userDataSource.sort = this.sort;
              this.userDataSource.paginator = this.paginator;
              this.applyFilter('');
              this.isBusy = false;
            });
        });
    }
  }

  /**
   * Removes a user from the current team
   * @param user The user to remove from team
   */
  removeUserFromTeam(tuser: TeamUser): void {
    if (this.isBusy) {
      return;
    }
    const index = this.teamUserDataSource.data.findIndex(
      (u) => u.user.id === tuser.user.id
    );
    if (index >= 0) {
      this.isBusy = true;
      this.userService
        .removeUserFromTeam(this.team.id, tuser.user.id)
        .subscribe((result) => {
          const tUsers = this.teamUserDataSource.data.slice(0);
          tUsers.splice(index, 1);
          this.teamUserDataSource = new MatTableDataSource(tUsers);
          const allUsers = this.userDataSource.data.slice(0);
          allUsers.push(tuser.user);
          this.userDataSource = new MatTableDataSource(allUsers);
          this.userDataSource.sort = this.sort;
          this.userDataSource.paginator = this.paginator;
          this.applyFilter('');
          this.isBusy = false;
        });
    }
  }

  updateMembership(teamUser: TeamUser): void {
    console.log(
      'Update Team Membership: ' +
        teamUser.name +
        '   role: ' +
        teamUser.teamMembership.roleId
    );
    const form = <TeamMembershipForm>{
      roleId:
        teamUser.teamMembership.roleId === ''
          ? null
          : teamUser.teamMembership.roleId,
    };

    this.teamMembershipService
      .updateTeamMembership(teamUser.teamMembership.id, form)
      .subscribe((result) => {
        console.log('Update complete');
      });
  }

  compare(a: string, b: string, isAsc: boolean) {
    if (a === null || b === null) {
      return 0;
    } else {
      return (a.toLowerCase() < b.toLowerCase() ? -1 : 1) * (isAsc ? 1 : -1);
    }
  }
}
