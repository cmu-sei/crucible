# Alloy Overview

Alloy joins the other independent Crucible apps together to provide a complete Crucible experience (i.e. labs, on-demand exercises, exercises, etc.).

## Alloy Definition

An Alloy *definition* is used to associate one or more of the individual Crucible applications, including a Player exercise, Caster directory, and Steamfitter scenario.

Only a **system admin** or a **content developer** can create or modify definitions in the Alloy administrator user interface.  **System admin** and **content developer** permissions are granted to users in the Player administrator user interface.

## Alloy Implementation
Once the definition has been created, it can be used to create an Alloy *implementation*. The implementation is the actual running of a labs, on-demand exercise, or exercise.

When the implementation is launched, Alloy:
 - clones the definition Player exercise
 - creates a Caster workspace under the definition Caster directory
 - creates and starts a Steamfitter session from the definition Steamfitter scenario

Active, ended, and failed implementations can be viewed by a system admin or content developer in the Alloy administrator user interface.

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

The default setting for the maximum number of active implementations per user is **two**.  However, this can be changed in the `MaxImplementationsForBasicUser` setting.

      "Resource": {
        "MaxImplementationsForBasicUser": 2
      }

## Alloy User Interface
The Alloy user interface as viewed by a user consists of two screens: 
 - **Labs:** contains a list of definitions and labs available to the user
 - **Launch:** contains a view of a specific definition or lab.  Here, the user can **Launch** an implementation if no active implementation exists for this user and definition combination.  If an active implementation already exists, then the user can open it in Player or end it.

> **Note:** Currently, there is **no** error reporting when launching a definition or lab.  If an error occurs, the user is returned to the Launch screen.
