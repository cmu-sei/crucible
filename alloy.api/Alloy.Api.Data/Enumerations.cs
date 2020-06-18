/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

namespace Alloy.Api.Data
{
    public enum EventStatus
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

    public enum InternalEventStatus
    {
        LaunchQueued = 1,
        CreatingView = 2,
        CreatingScenario = 3,
        CreatingWorkspace = 4,
        WritingWorkspaceFile = 5,
        PlanningLaunch = 6,
        PlannedLaunch = 7,
        ApplyingLaunch = 8,
        FailedLaunch = 9,
        AppliedLaunch = 10,
        StartingScenario = 11,
        Launched = 12,
        EndQueued = 21,
        DeletingView = 22,
        DeletedView = 23,
        DeletingScenario = 24,
        DeletedScenario = 25,
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
