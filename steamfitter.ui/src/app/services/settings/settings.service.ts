/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/


import {throwError as observableThrowError,  Observable } from 'rxjs';

import {catchError} from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserManagerSettings } from 'oidc-client';

@Injectable()
export class SettingsService {

  public ApiUrl = '';
  public OIDCSettings: UserManagerSettings = null;
  public AppTitle = "Player";
  public AppTopBarText = "Scenario Player";
  public UseLocalAuthStorage = false;

  constructor(private http: HttpClient) { }

  public load() {
    return new Promise((resolve, reject) => {
      this.http.get('assets/config/settings.json').pipe(
        catchError((error: any): any => {
          console.log('Configuration file "settings.json" could not be read');
          resolve(true);
          return observableThrowError(error.message);
        })).subscribe(settingsResult => {
          const settingsDefault = settingsResult;
          let settingsEnv = new Object();

          this.http.get('assets/config/settings.env.json').pipe(
            catchError((error: any) => {
              console.log('Configuration file "settings.env.json" could not be read. Default settings will be used.');
              resolve(true);
              this.parseSettings(settingsDefault, settingsEnv);
              return [];
            }))
            .subscribe(settingsEnvResult => {
              if (settingsEnvResult != null) {
                settingsEnv = settingsEnvResult;
              }

              this.parseSettings(settingsDefault, settingsEnv);
              resolve(true);
            });
        });
    });
  }

  private parseSettings(settingsDefault, settingsEnv) {
    Object.getOwnPropertyNames(this).forEach(prop => {
      this[prop] = settingsEnv[prop] || settingsDefault[prop];
      console.log('prop:' + prop);
    });
  }
}

