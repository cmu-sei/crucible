/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {throwError as observableThrowError,  Observable, BehaviorSubject, Subject } from 'rxjs';
import {catchError} from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { SettingsService } from '../settings/settings.service';
import { VmModel } from '../../models/vm/vm-model';
import { HttpClient } from '@angular/common/http';
import { Title } from '@angular/platform-browser';

declare var WMKS: any; // needed to check values

@Injectable()
export class VmService {

  public wmks;
  public model: VmModel;
  public showLoading = true;
  public showPower = false;
  public showPoweringOff = false;
  public showError = false;
  public showLock = false;
  public uploadConfig: any = {
    username: '',
    password: '',
    filepath: ''
  };

  public modelSubject: Subject<VmModel> = new Subject<VmModel>();
  public vmClipBoard = new BehaviorSubject<string>('');

  private ConsoleApiUrl;

  constructor (
    private http: HttpClient,
    private settings: SettingsService,
    private titleService: Title
  ) {
    this.ConsoleApiUrl = settings.ConsoleApiUrl;
    this.model = new VmModel();
  }

  public getVm(id: string) {

    return this.http.get(this.ConsoleApiUrl + id).pipe(
      catchError((error: any) => {
        if (error.status === 500) {
          return observableThrowError(new Error(error.status));
        } else if (error.status === 400) {
          return observableThrowError(new Error(error.status));
        } else if (error.status === 401) {
          return observableThrowError(new Error(error.status));
        } else if (error.status === 403) {
          return observableThrowError(new Error(error.status));
        } else if (error.status === 0) {
          return observableThrowError(new Error(error.status));
        } else {
          return observableThrowError(error);
        }
      })
    );
  }

  public sendPowerOn(id: string) {

    return this.http.get(this.ConsoleApiUrl + id + '/poweron').pipe(
      // .timeout(2000)
      catchError((error: any) => {
        if (error.status === 500) {
          return observableThrowError(new Error(error.status));
        } else if (error.status === 400) {
          return observableThrowError(new Error(error.status));
        } else {
          return observableThrowError(error);
        }
      }));
  }

  public sendPowerOff(id: string) {

    return this.http.get(this.ConsoleApiUrl + id + '/poweroff').pipe(
      // .timeout(2000)
      catchError((error: any) => {
        if (error.status === 500) {
          return observableThrowError(new Error(error.status));
        } else if (error.status === 400) {
          return observableThrowError(new Error(error.status));
        } else {
          return observableThrowError(error);
        }
      }));
  }

  public sendReboot(id: string) {

    return this.http.get(this.ConsoleApiUrl + id + '/reboot').pipe(
      // .timeout(2000)
      catchError((error: any) => {
        if (error.status === 500) {
          return observableThrowError(new Error(error.status));
        } else if (error.status === 400) {
          return observableThrowError(new Error(error.status));
        } else {
          return observableThrowError(error);
        }
      }));
  }

  public checkForVmTools(id: string) {
    return this.http.get(this.ConsoleApiUrl + id + '/checkvmtools');
  }

  public verifyCredentials(id: string) {
    const formData: FormData = new FormData();
    formData.append('username', this.uploadConfig.username);
    formData.append('password', this.uploadConfig.password);
    formData.append('filepath', this.uploadConfig.filepath);
    return this.http.post(this.ConsoleApiUrl + id + '/checkvmcredentials', formData);
  }

  public sendFileToVm(id: string, files: FileList) {

    const formData: FormData = new FormData();
    for (let i = 0; i < files.length; i++) {
      formData.append('username', this.uploadConfig.username);
      formData.append('password', this.uploadConfig.password);
      formData.append('filepath', this.uploadConfig.filepath);
      formData.append(files[i].name, files[i]);
    }
    console.log('sending ' + files.length.toString() + ' files to the api');
    return this.http.post(this.ConsoleApiUrl + id + '/uploadfile', formData);
  }

  public changeNic(id: string, adapter: string, nic: string){
    return this.http.get(this.ConsoleApiUrl + id + '/adapter/' + adapter + '/nic/' + nic).pipe(
    // .timeout(2000)
    catchError((error: any) => {
      if (error.status === 500) {
        return observableThrowError(new Error(error.status));
      } else if (error.status === 400) {
        return observableThrowError(new Error(error.status));
      } else {
        return observableThrowError(error);
      }
    }));
  }

  public async connect(id: string) {
    console.log('Attempting to connect to vm');

    this.getVm(id).subscribe(
      (model: VmModel) => {
        this.model = model;
        this.modelSubject.next(this.model);

        if (model.name) {
          this.titleService.setTitle(model.name);
        }

        // console.log('got vm model');
        if (model.state === 'error') {
          console.log('could not get power state');
          this.showError = true;
          this.showPower = false;
          this.showPoweringOff = false;
          this.showLoading = false;
          return;
        } else if (model.state === 'off') {
          this.showError = false;
          this.showPower = true;
          this.showPoweringOff = false;
          this.showLoading = false;
          // console.log('vm is off');
          return;
        } else if (model.state === '') {
          this.showError = false;
          this.showPower = false;
          this.showPoweringOff = false;
          this.showLoading = true;
          // console.log('vm is off');
          return;
        } else if (model.state === 'on') {
          this.showError = false;
          this.showPower = false;
          this.showPoweringOff = false;
          this.showLoading = true;
          // console.log('vm is on');
        }

        if (this.model.ticket) {
          // console.log('got ticket: ' + this.model.ticket);

          this.CreateWmks();
          const state = this.wmks.getConnectionState();
          if (state === WMKS.CONST.ConnectionState.DISCONNECTED) {
            // console.log('connecting to ' + this.model.ticket);
            this.wmks.connect(this.model.ticket);
          }
          // ticket is one time use so unset it
          this.model.ticket = null;
          this.showLoading = false;
        }
      },
      error => {
        this.model.name = 'Virtual Machine';
        this.model.id = id; // make sure that we dont lose the id
        this.model.state = error.message;
      }
    );
  }



  public CreateWmks() {

    this.wmks = WMKS.createWMKS('wmksContainer', {
      changeResolution: this.model.isOwner,
      rescale: true,
      position: WMKS.CONST.Position.CENTER,
      retryConnectionInterval: 5000 // Changed to 5 seconds.  This affects initial connection to console.
    });

    this.wmks.register(
      WMKS.CONST.Events.CONNECTION_STATE_CHANGE,
      (event: any, data: any) => {
        console.log('Connection State:  ' + data.state);
        if (data.state === WMKS.CONST.ConnectionState.CONNECTED) {
          // console.log('connection state change : connected');
          this.showLoading = false;
          this.showPower = false;
          this.showPoweringOff = false;
          this.showError = false;
          this.showLock = false;
        } else if (data.state === WMKS.CONST.ConnectionState.CONNECTING) {
          // console.log('connection state change : connecting');
          this.showLoading = true;
          this.showPower = false;
          this.showPoweringOff = false;
          this.showError = false;
          this.showLock = false;
        } else if (data.state === WMKS.CONST.ConnectionState.DISCONNECTED) {
          // console.log('connection state change : disconnected');
          // we should not connect if we sent poweroff
          if (this.showPower === false) {
            this.showLoading = true;
          }

          // When this.showPoweringOff is false, the disconnect came from the VM shutting itself down
          if (this.showPoweringOff === false) {
            // This is weird but there is no easy way to reload the wmks after it destroys itself when
            // a VM shuts itself down.  Wait a few seconds and then reload the page which will trigger
            // a reconnect to the already shutdown VM
            setTimeout(() => {
              console.log('attempt reload');
              window.location.reload();
            }, 3000);
          }
          this.showPoweringOff = false;
          this.showError = false;
          this.showLock = false;
          console.log('disconnect complete');
        }
      }
    );


    // Register the COPY event from the VM.
    this.wmks.register(
      WMKS.CONST.Events.COPY,
      (event: any, data: string) => {
        if (data) {
          this.vmClipBoard.next(data);
        }
      });

  }


  public powerOn(id: string) {
    this.sendPowerOn(id).subscribe(
      response => {
        // console.log(response);
        if (response === 'poweron submitted') {
          console.log('poweron submitted');
        } else if (response === 'already on') {
          console.log('already powered on');
        } else if (response === 'poweron error') {
          console.log('poweron error received');
        }
      },
      error => {
        console.log('error sending poweron to s3.vm.console');
      }
    );
  }

  public powerOff(id: string) {
    this.sendPowerOff(id).subscribe(
      response => {
        if (response === 'poweroff submitted') {
          this.showPoweringOff = true;
          // Wait before submitting the destroy to allow events from wmks to finish.
          setTimeout(() => {
            this.wmks.destroy();
          }, 1000);

        } else if (response === 'already off') {
        } else if (response === 'poweroff error') {
          console.log('poweroff error received');
        }
      },
      error => {
        console.log('error sending poweroff to s3.vm.console');
      }
    );
  }


  public reBoot(id: string) {
    // console.log('reboot requested');
    this.sendReboot(id).subscribe(
      response => {
        // console.log(response);
      },
      error => {
        console.log('error sending reboot to s3.vm.console');
        this.model.ticket = null;
      }
    );
  }

  public getUploadConfig(): Observable<any> {
    return this.uploadConfig.asObservable();
  }

  public getIsos() {
    return this.http.get(this.ConsoleApiUrl + this.model.id.toString() + '/isos');
  }

  public mountIso(id: string, iso: string) {
    const formData: FormData = new FormData();
    formData.append('iso', iso);
    return this.http.post(this.ConsoleApiUrl + id + '/mountiso', formData).pipe(
    catchError((error: any) => {
      if (error.status === 500) {
        return observableThrowError(new Error(error.status));
      } else if (error.status === 400) {
        return observableThrowError(new Error(error.status));
      } else {
        return observableThrowError(error);
      }
    }));
  }

}

