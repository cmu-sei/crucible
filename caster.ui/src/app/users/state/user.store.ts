/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Injectable } from '@angular/core';
import { Theme } from '@crucible/common';
import {
  EntityState,
  EntityStore,
  EntityUIStore,
  Store,
  StoreConfig,
} from '@datorama/akita';
import { User } from '../../generated/caster-api';
import { UserUi } from './user.model';

export interface UsersState extends EntityState<User> {}
export interface UserUIState extends EntityState<UserUi> {}

export const initialUserUiState: UserUi = {
  isSelected: false,
  isEditing: false,
  isSaved: false,
};

@Injectable({
  providedIn: 'root',
})
@StoreConfig({ name: 'users' })
export class UserStore extends EntityStore<UsersState> {
  ui: EntityUIStore<UserUIState>;
  constructor() {
    super();
    this.createUIStore().setInitialEntityState((entity) => ({
      ...initialUserUiState,
    }));
  }
}

export interface CurrentUserState {
  name: string;
  isSuperUser: boolean;
  id: string;
  theme?: Theme;
}

export function createInitialCurrentUserState(): CurrentUserState {
  return {
    name: '',
    isSuperUser: false,
    id: '',
    theme: Theme.LIGHT,
  };
}

@Injectable({
  providedIn: 'root',
})
@StoreConfig({ name: 'currentUser' })
export class CurrentUserStore extends Store<CurrentUserState> {
  constructor() {
    super(createInitialCurrentUserState());
  }
}
