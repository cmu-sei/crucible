# Alloy Overview

Alloy joins the other independent Crucible apps together to provide a complete Crucible experience.

## Alloy Event Template

An Alloy _event template_ is used to associate one or more of the individual Crucible applications, including a Player view template, Caster directory, and Steamfitter scenario template. When an event template is launched, a new event is created.

Only a **system admin** or a **content developer** can create or modify event templates in the Alloy administrator user interface. **System admin** and **content developer** permissions are granted to users in the Player administrator user interface.

## Alloy Event

Once the event template has been created, it can be used to create an Alloy _event_. The event is the actual running of a simulation.

When the event template is launched, Alloy:

- clones the Player exercise specified in the event template
- creates a Caster workspace under the Caster directory specified in the event template
- creates and starts a Steamfitter session from the Steamfitter scenario specified in the event template

Active, ended, and failed events can be viewed by a system admin or content developer in the Alloy administrator user interface.

## Alloy Settings

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

## Alloy User Interface

The Alloy user interface as viewed by a user consists of two screens:

- **Labs:** contains a list of event templates and labs available to the user.
- **Launch:** contains a view of a specific event template or lab. Here, the user can **Launch** an event if no active event exists for this user and event template combination. If an active event already exists, then the user can open it in Player or end it.

> **Note:** Currently, there is **no** error reporting when launching an event template or lab. If an error occurs, the user is returned to the Launch screen.
