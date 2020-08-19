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
  Input,
  ChangeDetectionStrategy,
  OnChanges,
  OnDestroy,
  ViewChild,
  ElementRef,
  Output,
  EventEmitter,
  AfterViewInit,
  NgZone,
  HostListener,
} from '@angular/core';
import { Run, RunStatus } from 'src/app/generated/caster-api';
import { ISubscription } from '@microsoft/signalr';
import { SignalRService } from 'src/app/shared/signalr/signalr.service';
import { Terminal, ITerminalOptions } from 'xterm';
import { FitAddon } from 'xterm-addon-fit';

@Component({
  selector: 'cas-run',
  templateUrl: './run.component.html',
  styleUrls: ['./run.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RunComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input() run: Run;
  @Input() loading: boolean;
  @Output() planOutput = new EventEmitter<string>();
  @Output() applyOutput = new EventEmitter<string>();

  output = '';
  streamSub: ISubscription<any>;
  status: RunStatus;
  isPlan = false;
  isApply = false;
  height: number;
  width: number;

  private preventScroll = false;

  @ViewChild('xterm') eleXterm: ElementRef;
  @ViewChild('dragHandleBottom') dragHandleBottom: ElementRef;

  // there is no infinite scrolling for xterm.  Set number of lines to very large number!
  xtermOptions: ITerminalOptions = {
    convertEol: true,
    scrollback: 9999999,
  };
  xterm: Terminal = new Terminal(this.xtermOptions);
  fitAddon: FitAddon = new FitAddon();

  constructor(private signalRService: SignalRService, private ngZone: NgZone) {
    window.addEventListener('wheel', this.scroll, { passive: false });
  }

  ngAfterViewInit() {
    this.openTerminal();
    this.setAllHandleTransform();
  }

  ngOnDestroy() {
    this.disposeStream();
    window.removeEventListener('wheel', this.scroll);
  }

  private disposeStream() {
    if (this.streamSub != null) {
      this.streamSub.dispose();
    }
  }

  openTerminal() {
    this.xterm.loadAddon(this.fitAddon);
    this.xterm.open(this.eleXterm.nativeElement);
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
        if (
          this.status === RunStatus.Applying ||
          this.status === RunStatus.AppliedStateError
        ) {
          // if we were already streaming applying, don't restart stream when applying finished
          this.setStatus();
          return false;
        }
        break;
      }
      case RunStatus.Failed: {
        if (
          this.status === RunStatus.Applying ||
          this.status === RunStatus.FailedStateError
        ) {
          // if we were already streaming applying, don't restart stream when applying finished
          this.setStatus();
          return false;
        }
        break;
      }
      default: {
        this.setStatus();
        return false;
      }
    }

    return true;
  }

  fullscreen() {
    const elem = this.eleXterm.nativeElement;

    // save current height to restore after fullscreen
    this.height = elem.offsetHeight;

    if (elem.requestFullscreen) {
      elem.requestFullscreen();
    } else if (elem.mozRequestFullScreen) {
      /* Firefox */
      elem.mozRequestFullScreen();
    } else if (elem.webkitRequestFullscreen) {
      /* Chrome, Safari and Opera */
      elem.webkitRequestFullscreen();
    } else if (elem.msRequestFullscreen) {
      /* IE/Edge */
      elem.msRequestFullscreen();
    }

    elem.style.height = window.outerHeight + 'px';
    this.fitAddon.fit();
  }

  @HostListener('document:fullscreenchange', ['$event'])
  @HostListener('document:webkitfullscreenchange', ['$event'])
  @HostListener('document:mozfullscreenchange', ['$event'])
  @HostListener('document:MSFullscreenChange', ['$event'])
  fullscreenmode() {
    // return to previous height when exiting fullscreen
    if (document.fullscreenElement == null) {
      this.eleXterm.nativeElement.style.height = this.height + 'px';
      this.setAllHandleTransform();
      this.fitAddon.fit();
    }
  }

  setAllHandleTransform() {
    const rect = this.eleXterm.nativeElement.getBoundingClientRect();
    this.setHandleTransform(this.dragHandleBottom.nativeElement, rect, 'y');
  }

  setHandleTransform(
    dragHandle: HTMLElement,
    targetRect: ClientRect | DOMRect,
    position: 'x' | 'y' | 'both'
  ) {
    const dragRect = dragHandle.getBoundingClientRect();
    const translateX = targetRect.width - dragRect.width;
    const translateY = targetRect.height - dragRect.height;

    if (position === 'x') {
      dragHandle.style.transform = `translate3d(${translateX}px, 0, 0)`;
    }

    if (position === 'y') {
      dragHandle.style.transform = `translate3d(0, ${translateY}px, 0)`;
    }

    if (position === 'both') {
      dragHandle.style.transform = `translate3d(${translateX}px, ${translateY}px, 0)`;
    }
  }

  dragMove(dragHandle: HTMLElement) {
    this.ngZone.runOutsideAngular(() => {
      this.resize(dragHandle, this.eleXterm.nativeElement);
    });

    this.fitAddon.fit();
  }

  resize(dragHandle: HTMLElement, target: HTMLElement) {
    const dragRect = dragHandle.getBoundingClientRect();
    const targetRect = target.getBoundingClientRect();

    const width = dragRect.left - targetRect.left + dragRect.width;
    const height = dragRect.top - targetRect.top + dragRect.height;

    target.style.width = width + 'px';
    target.style.height = height + 'px';

    this.setAllHandleTransform();
  }

  @HostListener('mouseenter')
  onMouseEnter() {
    this.preventScroll = true;
  }

  @HostListener('mouseout')
  onMouseOut() {
    this.preventScroll = false;
  }

  @HostListener('mouseover')
  onMouseOver() {
    this.preventScroll = true;
  }

  // prevent page from scrolling when terminal is scrolled
  scroll = (event: Event): void => {
    if (this.preventScroll) {
      event.stopPropagation();
      event.preventDefault();
    }
  };
}
