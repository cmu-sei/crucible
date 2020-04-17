/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Injectable, Injector, ErrorHandler } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { SystemMessageService } from '../system-message/system-message.service';
import { ApiError } from '../../swagger-codegen/s3.player.api';

@Injectable({
  providedIn: 'root'
})
export class ErrorService implements ErrorHandler {

  constructor(private injector: Injector) { }

  handleError(err: any) {
    console.log(err);
    const messageService = this.injector.get(SystemMessageService);
// Http failure response for (unknown url): 0 Unknown Error
    if (err instanceof HttpErrorResponse) {
      const apiError = (<ApiError>err.error);
      if (apiError.title !== undefined) {
        messageService.displayMessage(apiError.title, apiError.detail);
      } else if (err.message === 'Http failure response for (unknown url): 0 Unknown Error') {
        messageService.displayMessage('Player API Error', 'The Player API could not be reached.');
      } else {
        messageService.displayMessage(err.statusText, err.message);
      }
    } else if (err.message.startsWith('Uncaught (in promise)')) {
      if (err.rejection.message === 'Network Error') {
        messageService.displayMessage('Identity Server Error', 'The Identity Server could not be reached for user authentication.');
      } else {
        messageService.displayMessage('Error', err.rejection.message);
      }
    } else {
      messageService.displayMessage(err.name, err.message);
    }
  }
}

