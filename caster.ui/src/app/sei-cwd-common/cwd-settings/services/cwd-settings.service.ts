/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {Injectable, NgModule, Optional} from '@angular/core';
import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {catchError, map, take, tap} from 'rxjs/operators';
import {Observable, of, zip} from 'rxjs';

@NgModule()
export class CwdSettingsConfig {
  url: string;
  envUrl: string;
}

@Injectable({
  providedIn: 'root'
})
export class CwdSettingsService {
  private _url: string;
  private _envUrl: string;
  private _settings: any;
  
  get url() {
    return this._url;
  }
  
  set url(value) {
    this._url = value;
  }
  
  get envUrl() {
    return this._envUrl;
  }
  
  set envUrl(value) {
    this._envUrl = value;
  }
  
  set settings(value) {
    this._settings = value;
  }
  
  get settings() {
    return this._settings;
  }
  
  
  constructor(@Optional() config: CwdSettingsConfig = {url: `assets/config/settings.json`, envUrl: `assets/config/settings.env.json`},
              private http: HttpClient) {
    this.url = config.url;
    this.envUrl = config.envUrl;
  }
  
  load(): Promise<any> {
    return new Promise<any>((resolve) => {
      zip(
        this.http.get<any>(this.url).pipe(catchError(err => this.notify(err))),
        this.http.get<any>(this.envUrl).pipe(catchError(err => this.notify(err)))
      ).pipe(
        // Remove error objects, typically these are files or endpoints that don't exists.
        map(result => result.filter(f => !(f instanceof HttpErrorResponse)))
      )
        .subscribe((result) => {
          this.settings = result.reduce((p, v) => {
            return {
              ...p,
              ...v
            };
          }, this._settings);
          this.notify(this.settings).pipe(take(1)).subscribe();
          resolve(true);
      });
    });
  }
  
  notify(error: any): Observable<any> {
    return of(error).pipe(
      tap((e) => {
        switch (e.constructor) {
          case HttpErrorResponse: {
            console.log(error.message);
            break;
          }
          default: {
            console.log(JSON.stringify(error, null, 2));
          }
        }
      })
    );
  }
}

