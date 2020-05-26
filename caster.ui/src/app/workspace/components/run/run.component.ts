/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Component, OnInit, Input, ChangeDetectionStrategy, OnChanges, OnDestroy, ViewChild, ElementRef, Output, EventEmitter } from '@angular/core';
import { Run, RunStatus } from 'src/app/generated/caster-api';
import { ISubscription } from '@microsoft/signalr';
import { SignalRService } from 'src/app/shared/signalr/signalr.service';
import { Terminal, ITerminalOptions } from 'xterm';
import { FitAddon } from 'xterm-addon-fit';

@Component({
  selector: 'cas-run',
  templateUrl: './run.component.html',
  styleUrls: ['./run.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RunComponent implements OnInit, OnChanges, OnDestroy {

  @Input() run: Run;
  @Input() loading: boolean;
  @Output() planOutput = new EventEmitter<string>();
  @Output() applyOutput = new EventEmitter<string>();

  output = '';
  streamSub: ISubscription<any>;
  status: RunStatus;
  isPlan = false;
  isApply = false;

  @ViewChild('xterm', {static: true, read: ElementRef}) eleXtern: ElementRef;

  // there is no infinite scrolling for xterm.  Set number of lines to very large number!
  xtermOptions: ITerminalOptions = { convertEol: true, scrollback: 9999999};
  xterm: Terminal = new Terminal(this.xtermOptions);
  fitAddon: FitAddon = new FitAddon();

  constructor(private signalRService: SignalRService) { }

  ngOnInit() {
    this.xterm.open(this.eleXtern.nativeElement);
    this.xterm.loadAddon(this.fitAddon);
    this.fitAddon.fit();
  }

  ngOnChanges() {
    if (!this.shouldStartStream()) {
      return;
    }

    this.disposeStream();
    this.resetOutput();

    let streamResult;

    if (this.run.applyId !== null) {
      this.isApply = true;

      if (this.run.apply != null) {
        this.addOutput(this.run.apply.output);
        return;
      } else {
        streamResult = this.signalRService.streamApplyOutput(this.run.applyId);
      }
    } else if (this.run.planId !== null) {
      this.isPlan = true;

      if (this.run.plan != null) {
        this.addOutput(this.run.plan.output);
        return;
      } else {
        streamResult = this.signalRService.streamPlanOutput(this.run.planId);
      }
    }

    if (streamResult) {
      this.streamSub = streamResult.subscribe({
        next: (item: string) => {
          this.addOutput(item);
        },
        complete: () => {
          if (this.isApply) {
            this.applyOutput.emit(this.output);
          } else if (this.isPlan) {
            this.planOutput.emit(this.output);
          }
        },
        error: (err) => {
          console.log(err);
        },
      });
    } else {
      this.addOutput('Loading...');
    }
  }

  private addOutput(output: string) {
    this.output = this.output + output;
    this.xterm.write(output);
    this.fitAddon.fit();
  }

  private resetOutput() {
    this.xterm.clear();
  }

  private setStatus() {
    this.status = this.run.status == null ? null : this.run.status;
  }

  private shouldStartStream(): boolean {
    if (this.run == null) {
      return false;
    } else if (this.status == null && this.run.status !== RunStatus.Queued) {
      // if we haven't started streaming yet and there is something to stream, start stream
      this.setStatus();
      return true;
    } else if (this.status === this.run.status) {
      // if status has not changed, don't restart stream
      this.setStatus();
      return false;
    }

    switch (this.run.status) {
      case RunStatus.Planning: {
        this.setStatus();
        return true;
      }
      case RunStatus.Planned: {
        if (this.status === RunStatus.Planning) {
          // if we were already streaming planning, don't restart stream when planning finished
          this.setStatus();
          return false;
        }
        break;
      }
      case RunStatus.Applying: {
        this.setStatus();
        return true;
      }
      case RunStatus.Applied: {
        if (this.status === RunStatus.Applying) {
          // if we were already streaming applying, don't restart stream when applying finished
          this.setStatus();
          return false;
        }
        break;
      }
      case RunStatus.Failed:
      case RunStatus.Rejected:
      case RunStatus.Queued: {
        this.setStatus();
        return false;
      }
    }

    return true;
  }

  ngOnDestroy() {
    this.disposeStream();
  }

  private disposeStream() {
    if (this.streamSub != null) {
      this.streamSub.dispose();
    }
  }
}
