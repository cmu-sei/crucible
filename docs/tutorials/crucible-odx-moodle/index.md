# Crucible Range Build Guide: ODX consumed via Moodle

This tutorial walks you from having a clean, default Crucible stack to a working on‑demand exercise (ODX) delivered through Moodle. Assumes an already-deployed platform (minimum of [Caster](../caster/), [Player](../player/), [Alloy](../alloy/)) with defaults and [the Crucible plugin](https://github.com/cmu-sei/moodle-mod_crucible) is already installed on [Moodle](../integrations/#moodle).

## Prerequisites

- Appropriate roles across apps:
  - Alloy: `Administrator` or `Content Developer`
  - Player: `Administrator` or `Content Developer`
  - Caster: `Administrator` or `Content Developer`
  - Steamfitter: `Administrator` or `Content Developer`
- vCenter capacity sized for On-Demand (ODX) behavior (Caster can dynamically select hosts per exercise)
- Moodle's Crucible activity plugin available in your Learning Management System (LMS)

## Step 1: Create the Player Exercise or "View"

1. In Player, switch to **Administration**
1. **Views** -> **Add New View**; enter Name/Description and set Status
1. **Add Applications** (either from app templates or as blank apps)
1. **Add Teams**, assign roles/permissions, and add apps to each team
1. (Optional) **Configure Subscriptions** (webhooks) so apps (e.g., VM API, Maps) react when Alloy creates/deletes views for on-demand events

**Tip**: This is the exercise UI learners will open; Alloy will clone this view per event.

## Step 2: Prepare Caster Content (Directory)

1. In Caster, create a new **Project** and add a **Directory** that contains your Terraform configuration (modules, variables).
1. Ensure that your virtual machine names are unique, as required by vCenter. Typically, we recommend appending the `view_id` to the virtual machine name, as Alloy will fill in the variable with the cloned View's Id.
1. You do not need to create a workspace here for ODX; Alloy will create a workspace under the directory when an event launches.
1. Ensure your configuration is ready to plan/apply in vCenter.

**ODX Specific**: For vCenter *with* cluster licensing,	do not enable Dynamic Host. Point Terraform at the vCenter cluster and let vCenter handle host placement. For a vCenter *without* cluster licensing, enable Dynamic Host in Alloy so Caster selects a host at launch. When Alloy creates a workspace, Caster sets `DynamicHost=true`, chooses the least-loaded host among those assigned to the exercise, and writes a `generated_host_values.auto.tfvars` with `vsphere_host_name` and `vsphere_datastore`. In either licensing model, when the event ends, the system deletes the workspace and releases the host usage.

## Step 3: (Optional) Author a Steamfitter Scenario

Create a **Scenario Template** and/or **Scenario** to run tasks/injects during the event. [Steamfitter](../../steamfitter/) integrates with StackStorm for guest-VM actions and supports scheduled/manual tasks and injects.

## Step 4: Create the Alloy Definition (Template)

1. In Alloy (Admin), add a new **Definition**
1. Set **Name**, **Description**, **Duration**
1. Select from the dropdown or paste the IDs for:
    1. Player exercise (the view you built)
    1. Caster directory (from Step 2)
    1. Steamfitter scenario (Step 3, optional)
1. Save

What happens at launch in Alloy:

- Player exercise cloned into a new exercise
- Steamfitter session created (if provided)
- Caster workspace created in the chosen directory and it writes `auto.tfvars` with Exercise/Team/User context
- The workspace planned and applied
- The Steamfitter scenario starts

What happens at end:

- Alloy deletes the Player exercise and Steamfitter session, runs destroy on the Caster workspace, and deletes the workspace.

**Note**: Alloy's user launch screen does not surface detailed error reporting; failures return the user to Launch.

## Step 5: Expose the Exercise in Moodle

Use the [Crucible Plugin for Moodle](https://github.com/cmu-sei/moodle-mod_crucible) activity to start, access, and stop Crucible labs/exercises from a Moodle course. The activity can embed the Crucible VM app in an iframe or link out to the full Player experience. Follow [the plugin's documentation](https://cmu-sei.github.io/crucible/integrations/#moodle) for installation and activity configuration.

## Step 6: Launch and Validate

As a user (or instructor), open Alloy -> **Labs/Launch**, select the event template (Definition), and **Launch**. If an active event exists for that user/template, open or end it instead.

Observe the lifecycle:

- Player view cloned
- Caster workspace planned/applied to vCenter
- Steamfitter scenario started
- Player apps visible to teams

From Player, interact with the apps you assigned (VMs, chat, tickets, etc.).

## Troubleshooting / Ops Notes

If you must stop `Caster.Api`, first disable Workspace Operations in Administration -> **Workspaces** and wait for active runs to complete (avoids state corruption).

For ODX capacity issues, check the available hosts, since excessive VM counts against constrained resources will fail the run.
