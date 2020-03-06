/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/


import {throwError as observableThrowError,  Observable,  BehaviorSubject } from 'rxjs';
import {catchError,  map } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse, HttpHeaders } from '@angular/common/http';
import { SettingsService } from '../settings/settings.service';
import { ExerciseData } from '../../models/exercise-data'
import { MockExerciseData } from '../../models/mocks/mock-exercise-data';
import { ApplicationsService } from '../applications/applications.service';
import { TeamData } from '../../models/team-data';


const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type':  'application/json'
  })
};

@Injectable()
export class ExercisesService {

  public readonly currentExerciseGuid: BehaviorSubject<string>;
  public exerciseList: BehaviorSubject<Array<ExerciseData>>;

  constructor(
    private http: HttpClient,
    private settings: SettingsService,
    private applicationsService: ApplicationsService) {

    this.currentExerciseGuid = new BehaviorSubject<string>('');
    this.exerciseList = new BehaviorSubject<Array<ExerciseData>>(new Array<ExerciseData>());
  }

  /**
   * Makes a call to the API and updates the exerciseList
   * @param userGuid
   */
  public getExerciseList(userGuid: string): void {
    this.http.get<Array<ExerciseData>>(`${this.settings.ApiUrl}/users/${userGuid}/exercises`)
      .subscribe(exercises => {
        const exerciseArray = new Array<ExerciseData>();
        exercises.forEach(exercise => {
          this.http.get<Array<TeamData>>(`${this.settings.ApiUrl}/users/${userGuid}/exercises/${exercise.id}/teams`).pipe(
            map(teams => teams.filter(t => t.isMember))
          ).subscribe(teams => {
            teams.forEach(team => {
              if (team.isPrimary) {
                const ex = <ExerciseData>({
                  id: exercise.id,
                  name: exercise.name,
                  description: exercise.description,
                  status: exercise.status,
                  teamId: team.id,
                  teamName: team.name
                });
                exerciseArray.push(ex);
              }
            });
            this.exerciseList.next(exerciseArray);
          });
        });
      }),
      (err => {
        console.log(err);
        return observableThrowError(err || 'Server error');
      });
  }


  public setPrimaryTeamId(userGuid: string, teamGuid: string): Observable<any> {
    return this.http.post<any>(`${this.settings.ApiUrl}/users/${userGuid}/teams/${teamGuid}/primary`, null, httpOptions).pipe(
      catchError(err => {
        return observableThrowError(err || 'Server error');
      }));
  }
  
  /**
   * Returns a single instance of the specified exercise
   * @param exerciseGuid
   */
  public getExerciseById(exerciseGuid: string): Observable<ExerciseData> {
    return this.http.get<ExerciseData>(`${this.settings.ApiUrl}/exercises/${exerciseGuid}`).pipe(
      catchError(err => {
        return observableThrowError(err || 'Server error');
      }));
  }

}

