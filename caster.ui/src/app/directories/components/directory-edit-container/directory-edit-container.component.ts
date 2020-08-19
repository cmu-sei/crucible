/**
 * Crucible
 * Copyright 2020 Carnegie Mellon University.
 * NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
 * Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
 * [DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
 * Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
 * DM20-0181
 */

import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  Input,
  Output,
  EventEmitter,
} from '@angular/core';
import { Observable } from 'rxjs';
import {
  Directory,
  TerraformVersionsResult,
  TerraformService,
} from 'src/app/generated/caster-api';
import { DirectoryService, DirectoryQuery } from '../../state';

@Component({
  selector: 'cas-directory-edit-container',
  templateUrl: './directory-edit-container.component.html',
  styleUrls: ['./directory-edit-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DirectoryEditContainerComponent implements OnInit {
  @Input() id: string;
  @Output() editDirectoryComplete = new EventEmitter<boolean>();

  public directory$: Observable<Directory>;
  public versionResult$: Observable<TerraformVersionsResult>;

  constructor(
    private directoryService: DirectoryService,
    private directoryQuery: DirectoryQuery,
    private terraformService: TerraformService
  ) {}

  ngOnInit(): void {
    this.directory$ = this.directoryQuery.selectEntity(this.id);
    this.versionResult$ = this.terraformService.getTerraformVersions();
  }

  onUpdateDirectory(directory: Partial<Directory>) {
    if (directory != null) {
      this.directoryService.partialUpdate(this.id, directory);
    }
    this.editDirectoryComplete.emit(true);
  }
}
