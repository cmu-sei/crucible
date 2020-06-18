/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import {
  EntityUIQuery,
  HashMap,
  Order,
  Query,
  QueryConfig,
  QueryEntity,
} from '@datorama/akita';
import {
  UsersState,
  UserStore,
  UserUIState,
  CurrentUserState,
  CurrentUserStore,
} from './user.store';
import { User } from '../../generated/caster-api';
import { Injectable, InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

@QueryConfig({
  sortBy: 'name',
  sortByOrder: Order.ASC,
})
@Injectable({
  providedIn: 'root',
})
export class UserQuery extends QueryEntity<UsersState> {
  ui: EntityUIQuery<UserUIState>;
  isLoading$ = this.select((state) => state.loading);

  constructor(protected store: UserStore) {
    super(store);
    this.createUIQuery();
  }

  selectByUserId(id: string): Observable<User> {
    return this.selectEntity(id);
  }
}

@Injectable({
  providedIn: 'root',
})
export class CurrentUserQuery extends Query<CurrentUserState> {
  userTheme$ = this.select((state) => state.theme);
  constructor(protected store: CurrentUserStore) {
    super(store);
  }
}
