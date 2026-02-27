# ![Alloy Logo](../assets/alloy-logo.svg){: style="height:75px;width:75px"} **Alloy:** Conducting a Simulation

## Overview

An **Alloy** Definition brings together a Player View, a Caster Directory, and a Steamfitter Scenario Template to create an event that is user-launchable. You can schedule an event to run for a set amount of time. Upon completion of the event, Alloy will clear up all related assets.

## Permissions and Roles

### Permissions

Sets of *permissions* govern access to features of Alloy. Permissions can apply globally or per Event Template/Event. Examples of global permissions include:

- `CreateEventTemplates`: Create new Event Templates
- `ViewEvents`: View all Events and event Users and Groups
- `ManageUsers`: Make changes to Users

Users with View or Manage permissions for an administration function (for example, `ViewRoles` or `ManageGroups`) can open the **Administration** area. However, they see only the sections they have permission to access in the sidebar menu.

There are many more permissions available. View them in the **Roles** section of the **Administration** area.

### Roles

You apply permissions to *users* by grouping them into *roles*. Each user can have a *system role* applied giving them global permissions across all of Alloy. The three default system roles are:

- **Administrator:** All permissions within the system

- **Content Developer:** Has `CreateEventTemplates`, `CreateEvents`, `ExecuteEvents` permissions. Users with the Content Developer role can create and manage their own Event Templates and Events, but can't affect any global settings or other user's Event Templates and Events.

- **Observer:** Has `ViewEventTemplates`, `ViewEvents`, `ViewUsers`, `ViewRoles`, and `ViewGroups` permissions; users with the Observer role can view all these areas, but can't make changes.

Users who have the `ManageRoles` permission can create custom system roles. Do this in the **Roles** section of the **Administration** area.

Admins apply roles to users in the **Users** section of the **Administration** area.

## Administrator Guide

## Administration View

Across the Crucible exercise applications, the **Administration View** is where privileged users configure the platform and control access. It includes user and team management, role and permission assignment, and setup and maintenance of app-specific templates and content. The Administration View is where admins prepare and manage the environment so events run smoothly for participants.

Accessing the Administration View is the same in all Crucible exercise applications: expand the dropdown next to your username in the top-right corner and select **Administration**.

![The Administration dropdown in the top right-corner](img/crucible-administration.png)


When deploying the Alloy API, configure the `ResourceOwnerAuthorization` settings for a superadmin account so the Alloy API can make the necessary calls to the other APIs.

```json
      "ResourceOwnerAuthorization": {
        "Authority": "http://localhost:5000",
        "ClientId": "alloy.api",
        "ClientSecret": "",
        "UserName": "",
        "Password": "",
        "Scope": "s3 s3-vm alloy steamfitter caster-api",
        "TokenExpirationBufferSeconds": 900
      },
```

The default setting for the maximum number of active events per user is **two**. However, you can change this in the `MaxEventsForBasicUser` setting.

```json
      "Resource": {
        "MaxEventsForBasicUser": 2
      }
```

### Manage Event Templates

In Alloy, you use an *event template* to associate one or more of the individual Crucible applications, including a Player view, Caster directory, and Steamfitter scenario template. A user can launch a new event from the defined event template.

Only an Alloy **superadmin** or **event admin** can create or modify event templates in the Alloy administrator user interface. A Player superadmin grants the underlying Player permissions that allow event admins to preview the Player view tied to an event template.

Once the event admin creates the event template, a user can launch the Alloy *event* from the template. The event is the actual running of a simulation.

When a user launches an event from an event template, Alloy:

- Clones the Player exercise specified in the event template
- Creates a Caster workspace under the Caster directory specified in the event template
- Creates and starts a Steamfitter session from the Steamfitter scenario specified in the event template

Superadmins and event admins can view active, ended, and failed events in the Alloy administrator user interface.

<!-- ### Events -->

## User Guide

The Alloy user interface as viewed by a user consists of two screens:

- **Labs:** contains a list of event templates and labs available to the user.
- **Launch:** contains a view of a specific event template or lab. Here, the user can **Launch** an event if no active event exists for this user and event template combination. If an active event already exists, then the user can open it in Player or end it.

!!! warning

    There is no error reporting when launching an event template or lab. If an error occurs, the user is returned to the Launch screen.

### Deploy an Event/Exercise

#### How to Create an Alloy Definition

Before creating an Alloy definition, the supporting roles complete the following in their respective Crucible apps:

- Player view admin (or content view user) creates the *Player exercise*.
- Caster content developer creates the *Caster directory*.
- Steamfitter content developer creates the *Steamfitter scenario*.

All of these components are optional for a definition, so a definition can have any combination of the three components.

 1. Add a new Definition.
 2. Complete the **Name**, **Description**, and **Duration** fields.
 3. Copy and paste the IDs of the Player exercise, Caster directory, and Steamfitter scenario.
 4. Save the Definition.

To give the user the ability to end the implementation or lab from inside Player, add a link to the definition as a Player application in the associated Player exercise.

#### Understanding the Launch Process

When launching an implementation from a definition, the Alloy API goes through the following process:

1. Clones the definition Player exercise into a new Player exercise and adds the Player exercise ID to the Alloy implementation.
2. Creates a Steamfitter session from the definition Steamfitter scenario and adds the Steamfitter scenario ID to the Alloy implementation.
3. Creates a Caster workspace in the definition Caster directory and adds the Caster Workspace ID to the Alloy implementation.
4. Creates a Terraform `auto.tfvars` file in the Caster workspace that contains the following:
    - Exercise ID
    - Team Name and ID of every team in the exercise
    - User Name and ID
5. Plans and applies the Caster workspace to deploy the infrastructure.
6. Starts the Steamfitter scenario.

#### Understanding the End Process

A user can initiate the end process or, because time has expired, the `AlloyQueryService` can initiate the end process. Regardless of which method initiates the process, the Alloy API does the following:

 1. Deletes the Player exercise.
 2. Deletes the Steamfitter session.
 3. Plans and applies the *destroy* of the Caster workspace.
 4. Deletes the Caster workspace.

<!-- ### Invite Others to Event/Exercise -->

<!-- ## Alloy Tips -->

## Glossary

This glossary defines key terms and concepts used in the Alloy application.

**Definition:** The structure and build of the Alloy Exercise, usually contains some combination of a Player exercise, Caster directory, and Steamfitter scenario.

**Event:** The actual running of a simulation. An Event Template is the planned exercise to occur.
