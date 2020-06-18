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
import { SystemMessageService } from '../cwd-system-message/services/system-message.service';

@Injectable({
  providedIn: 'root',
})
export class ErrorService implements ErrorHandler {
  constructor(private injector: Injector) {}

  handleError(err: any) {
    const messageService = this.injector.get(SystemMessageService);
    let title = '';
    let message = '';
    if (err instanceof HttpErrorResponse) {
      // Backend returns unsuccessful response codes such as 404, 500 etc.
      const apiError = err as HttpErrorResponse;
      title = apiError.error.title;
      message =
        'Code:  ' + apiError.status + '\nMessage: ' + apiError.message + '\n';
      message += apiError.error.detail;
      console.log(title);
      console.log(apiError);
    } else {
      // A client-side or network error occurred.
      const error = err as Error;
      title = 'Client error occurred:';
      message = error.message + ' \n' + error.stack;
      console.log(title);
      console.log(err);
    }
    // Delay 1 second to allow page to load.  This prevents error message from popping up irregularly
    messageService.displayMessage(title, message, 1000);
  }
}
