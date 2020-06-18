/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Observable, config } from 'rxjs';
import { MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material';
import { Injectable } from '@angular/core';
import { ConfirmDialogComponent } from '../../components/shared/confirm-dialog/confirm-dialog.component';
import { AddRemoveUsersDialogComponent } from '../../components/shared/add-remove-users-dialog/add-remove-users-dialog.component';
import { Team } from '../../generated/s3.player.api';
import { CreatePermissionDialogComponent } from '../../components/admin-app/admin-role-permission-search/create-permission-dialog/create-permission-dialog.component';
import { CreateRoleDialogComponent } from '../../components/admin-app/admin-role-permission-search/create-role-dialog/create-role-dialog.component';
import { SelectRolePermissionsDialogComponent } from '../../components/admin-app/admin-role-permission-search/select-role-permissions-dialog/select-role-permissions-dialog.component';


@Injectable()
export class DialogService {

    constructor(private dialog: MatDialog) { }

    public confirm(title: string, message: string, data?: any): Observable<boolean> {

        let dialogRef: MatDialogRef<ConfirmDialogComponent>;
        dialogRef = this.dialog.open(ConfirmDialogComponent, {data: data || {} });
        dialogRef.componentInstance.title = title;
        dialogRef.componentInstance.message = message;

        return dialogRef.afterClosed();
    }

    public createPermission(title: string, permission: any, configData?: any): Observable<boolean> {

        let dialogRef: MatDialogRef<CreatePermissionDialogComponent>;
        dialogRef = this.dialog.open(CreatePermissionDialogComponent, configData || {});
        dialogRef.componentInstance.title = title;
        dialogRef.componentInstance.permission = permission;

        return dialogRef.afterClosed();
    }

    public createRole(title: string, name: string, configData?: any): Observable<boolean> {

        let dialogRef: MatDialogRef<CreateRoleDialogComponent>;
        dialogRef = this.dialog.open(CreateRoleDialogComponent, configData || {});
        dialogRef.componentInstance.title = title;
        dialogRef.componentInstance.name = name;

        return dialogRef.afterClosed();
    }

    public selectRolePermissions(title: string, role: any, permissions: any[], configData?: any): Observable<boolean> {

        let dialogRef: MatDialogRef<SelectRolePermissionsDialogComponent>;
        dialogRef = this.dialog.open(SelectRolePermissionsDialogComponent, configData || {});
        dialogRef.componentInstance.title = title;
        dialogRef.componentInstance.role = role;
        dialogRef.componentInstance.permissions = permissions;

        return dialogRef.afterClosed();
    }


    public addRemoveUsersToTeam(title: string, team: Team, configData?: any): Observable<boolean> {

        let dialogRef: MatDialogRef<AddRemoveUsersDialogComponent>;
        dialogRef = this.dialog.open(AddRemoveUsersDialogComponent, configData || {} );
        dialogRef.componentInstance.title = title;
        dialogRef.componentInstance.loadTeam(team);
        return dialogRef.afterClosed();
    }

}
