/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { HttpClient, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ComnSettingsService } from '@crucible/common';

@Injectable()
export class FileService {
  private viewId: string;

  constructor(
    private http: HttpClient,
    private settings: ComnSettingsService,
    private router: Router
  ) {
    this.viewId = this.router.routerState.snapshot.root.firstChild.params[
      'viewId'
    ];
  }

  public uploadIso(isForAll: boolean, file: File) {
    const scope = isForAll ? 'view' : 'team';
    const payload: FormData = new FormData();
    payload.append('size', file.size.toString());
    payload.append('scope', scope);
    payload.append(file.name, file);
    return this.http.request(
      new HttpRequest(
        'POST',
        `${this.settings.settings.ApiUrl}/views/${this.viewId}/isos`,
        payload,
        { reportProgress: true }
      )
    );
  }
}
