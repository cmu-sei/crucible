/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Observable } from 'rxjs';
import { MatDialogRef, MatDialog } from '@angular/material/dialog';
import { Injectable } from '@angular/core';
import { MessageDialogComponent } from '../../components/shared/message-dialog/message-dialog.component';
import { SendTextDialogComponent } from '../../components/shared/send-text-dialog/send-text-dialog.component';
import { FileUploadInfoDialogComponent } from '../../components/shared/file-upload-info-dialog/file-upload-info-dialog.component';
import { MountIsoDialogComponent } from '../../components/shared/mount-iso-dialog/mount-iso-dialog.component';


@Injectable()
export class DialogService {

    constructor(private dialog: MatDialog) { }

    public message(title: string, message: string, data?: any): Observable<boolean> {

        let dialogRef: MatDialogRef<MessageDialogComponent>;
        dialogRef = this.dialog.open(MessageDialogComponent, {data: data || {} });
        dialogRef.componentInstance.title = title;
        dialogRef.componentInstance.message = message;

        return dialogRef.afterClosed();
    }

    public sendText(title: string, configData?: any): Observable<boolean> {

        let dialogRef: MatDialogRef<SendTextDialogComponent>;
        dialogRef = this.dialog.open(SendTextDialogComponent, configData || {});
        dialogRef.componentInstance.title = title;

        return dialogRef.afterClosed();
    }

    public getFileUploadInfo(title: string, configData?: any): Observable<boolean> {

        let dialogRef: MatDialogRef<FileUploadInfoDialogComponent>;
        dialogRef = this.dialog.open(FileUploadInfoDialogComponent, configData || {});
        dialogRef.componentInstance.title = title;

        return dialogRef.afterClosed();
    }

    public mountIso(publicIsos: string[], teamIsos: string[], configData?: any): Observable<boolean> {

        let dialogRef: MatDialogRef<MountIsoDialogComponent>;
        dialogRef = this.dialog.open(MountIsoDialogComponent, configData || {});
        dialogRef.componentInstance.publicIsos = publicIsos;
        dialogRef.componentInstance.teamIsos = teamIsos;

        return dialogRef.afterClosed();
    }


}


