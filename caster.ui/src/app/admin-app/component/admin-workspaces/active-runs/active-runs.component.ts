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
  Component,
  OnInit,
  ChangeDetectionStrategy,
  Input,
  Output,
  EventEmitter,
} from '@angular/core';
import { Run } from 'src/app/generated/caster-api';

@Component({
  selector: 'cas-active-runs',
  templateUrl: './active-runs.component.html',
  styleUrls: ['./active-runs.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActiveRunsComponent implements OnInit {
  @Input() runs: Run[];
  @Input() expandedRuns: string[];

  @Output() expandRun = new EventEmitter<{ expand: boolean; item: Run }>();
  @Output() planUpdated = new EventEmitter<{ output: string; item: Run }>();
  @Output() applyUpdated = new EventEmitter<{ output: string; item: Run }>();

  constructor() {}

  ngOnInit() {}

  expand(event) {
    const { expand, item } = event;
    this.expandRun.emit({ expand, item });
  }

  planOutput(output: string, item: Run) {
    this.planUpdated.emit({ output, item });
  }

  applyOutput(output: string, item: Run) {
    this.applyUpdated.emit({ output, item });
  }
}
