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
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { ITerminalAddon, Terminal } from 'xterm';
import { FitAddon } from 'xterm-addon-fit';

@Component({
  selector: 'cas-output',
  templateUrl: './output.component.html',
  styleUrls: ['./output.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OutputComponent implements OnInit, OnChanges {
  @Input() loading: boolean;
  @Input() output: string;
  @ViewChild('xterm', { static: true, read: ElementRef }) eleXtern: ElementRef;
  xterm: Terminal = new Terminal();
  fitAddon: FitAddon = new FitAddon();
  constructor() {}

  ngOnInit() {
    this.xterm.setOption('convertEol', true);
    this.xterm.open(this.eleXtern.nativeElement);
    this.xterm.loadAddon(this.fitAddon);
    this.xterm.setOption('scrollback', 9999999); // there is no infinite scrolling for xterm.  Set number of lines to very large number!
    this.fitAddon.fit();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.output && this.xterm) {
      this.xterm.clear();
      this.xterm.write(changes.output.currentValue);
      this.fitAddon.fit();
    }
  }
}
