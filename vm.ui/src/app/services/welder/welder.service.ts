/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { SettingsService } from '../settings/settings.service';

@Injectable()
export class WelderService {
  private readonly deployUrl: string;

  constructor(
    private http: HttpClient,
    private settings: SettingsService,
  ) {
    this.deployUrl = `${settings.WelderUrl}`;
  }

  getDeploymentForExercise(exerciseName: string) {
    const requestUrl = `${this.deployUrl}/api/${exerciseName}`;
    return this.http.get<any>(requestUrl);
  }

  deployToExercise(exerciseName: string) {
    const requestUrl = `${this.deployUrl}/api/${exerciseName}`;
    return this.http.post<any>(requestUrl, null);
  }

  getQueueSize() {
    const requestUrl = `${this.deployUrl}/queue`;
    return this.http.get<any>(requestUrl);
  }
}

