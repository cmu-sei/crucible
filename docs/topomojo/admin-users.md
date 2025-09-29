---
title: TopoMojo — Users Tab
---

# Admin Users

The **Users** tab shows all current TopoMojo users. You can create new users here and assign them permissions. The **Search** feature allows TopoMojo admins to search on the name of a TopoMojo user. To search for a user across all of TopoMojo, enter the term into the **Search** field or filter by *role* or *audience*.

Recall from workspace Settings that "audience" is a list of clients who can launch the workspace as a gamespace. Selecting an audience filter results in users who are part of that audience.

**View:** Select **View** to see the properties for the user.

**Delete:** Deletes the user.

## Roles

All permissions are *additive*; meaning a Creator can do everything a Builder can do and an Observer can do everything a Builder and Creator can do.

- **Admin:** Highest level of permission in TopoMojo; can do everything the other roles can do.
- **Observer:** Allows a user to view and use the Gamespaces tab. However, the *scope* of the user (see below) limits access too. An observer can deploy gamespaces with a matching *audience* and these are the only gamespaces the user can observe.
- **Creator:** Can have as many workspaces and templates as wanted.
- **Builder:** Can connect to bridge-net.
- **User:** No extra permissions in TopoMojo. This is the TopoMojo default.
- **Disabled:** No permissions in TopoMojo.

## Create a New User

**Name:** Enter a new user name here.

**Scope:** A space-delimited list of administrator-defined groups the user belongs to. Administrators can define a *scope* with any name here. A user's scope determines which workspaces they have permission to deploy gamespaces from. Users can only deploy a gamespace from a workspace if the user has a *scope* that matches an *audience* defined in the workspace. See also: [Building a new workspace](building-a-workspace.md).

**Workspace Limit:** The maximum number of workspaces this user can manage.

**Gamespace Limit:** The maximum number of concurrent gamespaces allowed for this user.

**Gamespace Max Duration:** The maximum amount of minutes allowed for a gamespace.

**Gamespace Cleanup Grace time:** The number of "grace" minutes between the time the gamespace expires and when TopoMojo tears it down.

**Generate ApiKey:** Generate API keys here. This allows users to programmatically interact with the TopoMojo API without needing to log in.
