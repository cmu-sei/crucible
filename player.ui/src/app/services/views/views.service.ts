/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ComnSettingsService } from '@crucible/common';
import {
  BehaviorSubject,
  Observable,
  throwError as observableThrowError,
} from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TeamData } from '../../models/team-data';
import { ViewData } from '../../models/view-data';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
  }),
};

@Injectable()
export class ViewsService {
  public readonly currentViewGuid: BehaviorSubject<string>;
  public viewList: BehaviorSubject<Array<ViewData>>;

  constructor(private http: HttpClient, private settings: ComnSettingsService) {
    this.currentViewGuid = new BehaviorSubject<string>('');
    this.viewList = new BehaviorSubject<Array<ViewData>>(new Array<ViewData>());
  }

  /**
   * Makes a call to the API and updates the viewList
   * @param userGuid
   */
  public getViewList(userGuid: string): void {
    this.http
      .get<Array<ViewData>>(
        `${this.settings.settings.ApiUrl}/users/${userGuid}/views`
      )
      .subscribe((views) => {
        const viewArray = new Array<ViewData>();
        views.forEach((view) => {
          this.http
            .get<Array<TeamData>>(
              `${this.settings.settings.ApiUrl}/users/${userGuid}/views/${view.id}/teams`
            )
            .pipe(map((teams) => teams.filter((t) => t.isMember)))
            .subscribe((teams) => {
              teams.forEach((team) => {
                if (team.isPrimary) {
                  const ex = <ViewData>{
                    id: view.id,
                    name: view.name,
                    description: view.description,
                    status: view.status,
                    teamId: team.id,
                    teamName: team.name,
                  };
                  viewArray.push(ex);
                }
              });
              this.viewList.next(viewArray);
            });
        });
      }),
      (err) => {
        console.log(err);
        return observableThrowError(err || 'Server error');
      };
  }

  public setPrimaryTeamId(userGuid: string, teamGuid: string): Observable<any> {
    return this.http
      .post<any>(
        `${this.settings.settings.ApiUrl}/users/${userGuid}/teams/${teamGuid}/primary`,
        null,
        httpOptions
      )
      .pipe(
        catchError((err) => {
          return observableThrowError(err || 'Server error');
        })
      );
  }

  /**
   * Returns a single instance of the specified view
   * @param viewGuid
   */
  public getViewById(viewGuid: string): Observable<ViewData> {
    return this.http
      .get<ViewData>(`${this.settings.settings.ApiUrl}/views/${viewGuid}`)
      .pipe(
        catchError((err) => {
          return observableThrowError(err || 'Server error');
        })
      );
  }
}
