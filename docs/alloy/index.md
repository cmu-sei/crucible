# **Alloy**
*Conducting a Simulation*

## Overview

An Alloy Definition brings together a Player View, a Caster Directory, and a Steamfitter Scenario Template to create an event that is user-launchable.  Events can be scheduled to run for a set amount of time. Upon completion of the event, Alloy will clear up all related assets.

## Administrator Guide

When deploying the Alloy API, `ResourceOwnerAuthorization` settings must be configured for a system admin account so the Alloy API can make the necessary calls to the other API's.

      "ResourceOwnerAuthorization": {
        "Authority": "http://localhost:5000",
        "ClientId": "alloy.api",
        "ClientSecret": "",
        "UserName": "",
        "Password": "",
        "Scope": "s3 s3-vm alloy steamfitter caster-api",
        "TokenExpirationBufferSeconds": 900
      },

The default setting for the maximum number of active events per user is **two**. However, this can be changed in the `MaxEventsForBasicUser` setting.

      "Resource": {
        "MaxEventsForBasicUser": 2
      }

### Manage Event Templates

An Alloy _event template_ is used to associate one or more of the individual Crucible applications, including a Player view, Caster directory, and Steamfitter scenario template. When an event template is launched, a new event is created.

Only a **system admin** or a **content developer** can create or modify event templates in the Alloy administrator user interface. **System admin** and **content developer** permissions are granted to users in the Player administrator user interface.

Once the event template has been created, it can be used to create an Alloy _event_. The event is the actual running of a simulation.

When the event template is launched, Alloy:

- Clones the Player exercise specified in the event template
- Creates a Caster workspace under the Caster directory specified in the event template
- Creates and starts a Steamfitter session from the Steamfitter scenario specified in the event template

Active, ended, and failed events can be viewed by a system admin or content developer in the Alloy administrator user interface.

## User Guide 

The Alloy user interface as viewed by a user consists of two screens:

- **Labs:** contains a list of event templates and labs available to the user.
- **Launch:** contains a view of a specific event template or lab. Here, the user can **Launch** an event if no active event exists for this user and event template combination. If an active event already exists, then the user can open it in Player or end it.

> **Note:** Currently, there is **no** error reporting when launching an event template or lab. If an error occurs, the user is returned to the Launch screen.

### Deploy an Event/Exercise

**How to create an Alloy definition**

Before creating an Alloy definition, the *Player exercise*, *Caster directory*, and *Steamfitter scenario* should be created. All of these components are actually optional for a definition, so a definition can have any combination of the three components.

 1. Add a new Definition.
 2. Complete the Name, Description, and Duration fields.
 3. Copy and paste the ID's of the Player exercise, Caster directory, and Steamfitter scenario.
 4. Save the Definition.

To give the user the ability to end the implementation or lab from inside Player, add a link to the definition as a Player application in the associated Player exercise.

**Understanding the launch process**

When launching an implementation from a definition, the Alloy API goes through the following process:

1.  Clones the definition Player exercise into a new Player exercise and adds the Player exercise ID to the Alloy implementation.
2.  Creates a Steamfitter session from the definition Steamfitter scenario and adds the Steamfitter scenario ID to the Alloy implementation.
3.  Creates a Caster workspace in the definition Caster directory and adds the Caster Workspace ID to the Alloy implementation.
4.  Creates a Terraform `auto.tfvars` file in the Caster workspace that contains the following:
    - Exercise ID
    - Team Name and ID of every team in the exercise
    - User Name and ID
5.  Plans and applies the Caster workspace to deploy the infrastructure.
6.  Starts the Steamfitter scenario.

**Understanding the end process**

There are two ways the end process can be triggered:
- user initiated, and
- `AlloyQueryService` of the Alloy API initiated because expiration time has been reached.

When the end process is initiated, the Alloy API goes through the following process:

 1. Deletes the Player exercise.
 2. Deletes the Steamfitter session.
 3. Plans and applies the *destroy* of the Caster workspace.
 4. Deletes the Caster workspace.

### Invite Others to Event/Exercise

## Alloy Tips