/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon� and CERT� are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Alloy.Api.Data
{
    public enum ImplementationStatus
    {
        Creating = 1,
        Active = 2,
        Paused = 3,
        Ended = 4,
        Expired = 5,
        Planning = 6,
        Applying = 8,
        Failed = 10,
        Ending = 11
    }

    public enum InternalImplementationStatus
    {
        LaunchQueued = 1,
        CreatingExercise = 2,
        CreatingSession = 3,
        CreatingWorkspace = 4,
        WritingWorkspaceFile = 5,
        PlanningLaunch = 6,
        PlannedLaunch = 7,
        ApplyingLaunch = 8,
        FailedLaunch = 9,
        AppliedLaunch = 10,
        StartingSession = 11,
        Launched = 12,
        EndQueued = 21,
        DeletingExercise = 22,
        DeletedExercise = 23,
        DeletingSession = 24,
        DeletedSession = 25,
        DeletingWorkspace = 26,
        VerifyingWorkspace = 27,
        DeletedWorkspace = 28,
        PlanningDestroy = 29,
        PlannedDestroy = 30,
        ApplyingDestroy = 31,
        FailedDestroy = 32,
        AppliedDestroy = 33,
        Ended = 34,
        PlanningRedeploy = 35,
        PlannedRedeploy = 36,
        ApplyingRedeploy = 37,
        AppliedRedeploy = 38
    }

    public enum AlloyClaimTypes
    {
        AlloyBasic,
        SystemAdmin,
        ContentDeveloper
    }
}
