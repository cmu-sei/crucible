/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { Resource, Run, Workspace } from '../../generated/caster-api';

export function createRun({
  id,
  workspaceId,
  createdAt,
  isDestroy,
  status,
  plan,
  apply,
}: Partial<Run>) {
  return {
    id: id || null,
    workspaceId,
    createdAt,
    isDestroy,
    status,
    planId: plan == null ? null : plan.id,
    applyId: apply == null ? null : apply.id,
    plan: plan || {},
    apply: apply || {},
  };
}

export interface StatusFilter {
  key: string;
  filter: boolean;
}

// Use declaration merging to extend the workspace model for the Entity state.
declare module 'src/app/generated/caster-api/model/workspace' {
  interface Workspace {
    runs: Run[];
    resources?: Resource[];
  }
}

export interface WorkspaceEntityUi {
  id?: string;
  isExpanded: boolean;
  expandedRuns: string[];
  expandedResources: string[];
  resourceActions: string[];
  selectedRuns: string[];
  statusFilter: StatusFilter[];
  workspaceView: string;
}

export function createWorkspace({
  id,
  name,
  directoryId,
  runs,
  dynamicHost,
  resources,
}: Partial<Workspace>) {
  return {
    id: id || null,
    name,
    directoryId,
    dynamicHost: dynamicHost || false,
    runs: runs || [],
    resources: resources || [],
  } as Workspace;
}
