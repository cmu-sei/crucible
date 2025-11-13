# Creating an On Demand Exercise Using Caster, Player, Alloy, Steamfitter, and Moodle

This tutorial walks you from having a clean, default Crucible stack to a working on‑demand exercise (ODX) delivered through Moodle. Assumes an already-deployed platform (minimum of [Caster](../../caster/index.md), [Player](../../player/index.md), [Alloy](../../alloy/index.md)) with defaults and [the Crucible plugin](https://github.com/cmu-sei/moodle-mod_crucible) is already installed on [Moodle](../../integrations/index.md#moodle).


## Prerequisites

- Appropriate roles across apps:
  - Alloy: `Administrator` or `Content Developer`
  - Player: `Administrator` or `Content Developer`
  - Caster: `Administrator` or `Content Developer`
  - Steamfitter: `Administrator` or `Content Developer`
- vCenter or Proxmox capacity sized for On-Demand (ODX) behavior
- [Crucible's Moodle activity plugin](https://github.com/cmu-sei/moodle-mod_crucible) installed on Moodle


## Overview

**Caster** provides a streamlined user interface for creating and deploying cyber range topologies using [Terraform](https://www.terraform.io/). For more detailed documentation on Caster, visit [Caster's documentation page](../../caster/index.md).

**Player** provides a centralized interface where participants, teams, and administrators go to engage in a cyber event. For more detailed documentation on Player, visit [Player's documentation page](../../player/index.md).

**Steamfitter** gives content developers the ability to create scenarios consisting of a series of scheduled tasks, manual tasks, and injects which run against virtual machines during a cyber event. For more detailed documentation on Steamfitter, visit [Steamfitter's documentation page](../../steamfitter/index.md).

**Alloy** brings together a Player View, a Caster Directory, and a Steamfitter Scenario Template to create a playable event. For more detailed documentation on Alloy, visit [Alloy's documentation page](../../alloy/index.md).

**Moodle** is an open-source Learning Management System (LMS). For more detailed documentation on Crucible's integration with Moodle, visit [Crucible's 3rd-party integration documentation](../../integrations/index.md).


## Table of Contents

- [Creating an On Demand Exercise Using Caster, Player, Alloy, Steamfitter, and Moodle](#creating-an-on-demand-exercise-using-caster-player-alloy-steamfitter-and-moodle)
  - [Prerequisites](#prerequisites)
  - [Overview](#overview)
  - [Table of Contents](#table-of-contents)
  - [Create the Player View Template](#create-the-player-view-template)
  - [Create the Caster Project and Directory](#create-the-caster-project-and-directory)
  - [Create a Steamfitter Scenario Template](#create-a-steamfitter-scenario-template)
  - [Create the Alloy Event Template](#create-the-alloy-event-template)
  - [Launch and Validate](#launch-and-validate)
  - [Expose the Exercise in Moodle](#expose-the-exercise-in-moodle)


## Create the Player View Template

Create the exercise (Player View) that participants will open. Alloy will clone this view per event. See the [Player documentation on managing Views](../../player/index.md#manage-views) for more detailed information.


1. Login to Player.
2. Select your Username from the top right corner, then click **Administration**.
3. From the left menu, select **Views**.
4. Click the `+` icon to **Add a New View**.
5. Enter a **Name**, **Description** and set the **Status** to "Active" or "Inactive". Select the box indicating this is a **Template**.
6. Select **Applications** to add applications to this View from existing templates or a blank starting point.
7. Click **Teams** to add teams and assign roles/permissions.
8. Optionally, click **Files** to upload files to this View.


## Create the Caster Project and Directory

Create a Caster Project and Directory to house your Terraform infrastructure-as-code for the exercise topology. See the [Caster documentation on creating a Project](../../caster/index.md#user-guide) for more details.

1. Login to Caster.
2. Click the `+` icon to create a new **Project** and add a **Directory** that contains your Terraform configuration (modules, variables).
3. Ensure your configuration is ready to plan/apply.

> **Note**: For vCenter *with* cluster licensing,	do not enable Dynamic Host. Point Terraform at the vCenter cluster and let vCenter handle host placement. For a vCenter *without* cluster licensing, enable Dynamic Host in Alloy so Caster selects a host at launch. When Alloy creates a workspace, Caster sets `DynamicHost=true`, chooses the least-loaded host among those assigned to the exercise, and writes a `generated_host_values.auto.tfvars` with `vsphere_host_name` and `vsphere_datastore`. In either licensing model, when the event ends, the system deletes the workspace and releases the host usage.

## Create a Steamfitter Scenario Template

Optionally, create a **Scenario Template** to run tasks/injects during the event. See the [Steamfitter documentation on Scenarios](../../steamfitter/index.md#scenario-templates) for more details.

1. Login to Steamfitter.
2. Select **Scenario Templates** and click **Add Scenario Template**.
3. Provide a **Name** and **Description**. Associate the Scenario Template with your Player View using the dropdown menu.
4. Add Tasks to define your scenario.

## Create the Alloy Event Template

Alloy ties all of the exercise components together and provide the user a centralized location to participate. See the [Alloy documentation on Event Templates](../../alloy/index.md#deploy-an-eventexercise)

1. Login to Alloy
2. Select your Username from the top right corner, then click **Administration**.
3. Select **Event Templates**, then click **Add New Event Template**.
4. Click the **Edit** icon to set a **Name**, **Description**, **Duration**.
5. Select from the dropdown or paste the IDs for the:
    1. Player View Template
    2. Caster Directory
    3. Steamfitter Scenario Template
6. **Save** your changes.

## Launch and Validate

From the Alloy home page, select the Event Template, and **Launch**.

Observe the lifecycle:

- Player view cloned
- Caster workspace planned/applied to vCenter
- Steamfitter scenario started
- Player apps visible to teams

From Player, interact with the apps you assigned to the View.

## Expose the Exercise in Moodle

Use the [Crucible Plugin for Moodle](https://github.com/cmu-sei/moodle-mod_crucible) activity type to start, access, and stop Crucible exercises from a Moodle course. The activity can embed the Crucible VM app in an iframe or link out to the full Player experience.
