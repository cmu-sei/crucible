# Player Overview

**Player** is the centralized interface where participants, teams, and administrators go to engage in a cyber event. In Player, participants view teams, applications, virtual environments, and third-party applications. The event experience is highly customizable by content developers.

Think of Player as the front door to an event running on Crucible. Player is meant to: 

- Provide a platform for individual participants on a team to view content in an exercise.
- Allow for quick navigation between various applications in the Crucible framework.
- Allow administrators to set what teams can see what applications.

Applications can use Player roles and permissions as needed. Notifications can be sent to individual users, teams, or the entire exercise by an administrator or an application with proper permissions.

Player is ***not*** meant to:

- Control any authentication or content within any application that it displays in a content panel.
- Provide any mechanisms for individual applications to communicate with each other.
- Provide any default applications.  However, some common applications can be consumed within an exercise.

Descriptions of the common applications used in Player can be found in the [Player Applications](https://cmu-sei.github.io/crucible/player-applications) document.

## Key Player Concepts 

Some key Player concepts are described below.

- **Apps:** A website a participant in a view can open within Player or in a separate browser tab. A common example of an app used in Player is the Mattermost messaging platform.
- **App template:** The settings associated with an app that is added to a team's *view*.  An app template can be created for common apps that are added to a view with default settings that an administrator can override if needed.
- **Authentication:** Player uses `IdentityServer 4` for authenticating users. Authentication gets a user into Player, but setting permissions within the Player Administration view (under **Users**) determines what a user can open and/or edit within Player.
- **Focused Application Panel:** The focused app panel displays the selected application in an iFrame. The iFrame points to the URL specified by the application template. The application within the focused app panel is responsible for authentication and content. Player displays content but has no control of the application running within the focused app panel.
- **Notification:** A message sent to a specific user, team, or view from an Administrator or an app that has been given permission.
- **Permission:** A key/value pair that can be created and used by any application given permission within the system. A permission can be assigned to a user or a team. Some permissions such as `SystemAdmin` are read-only. `ExerciseAdmin` and `SystemAdmin` are permanent permissions that cannot be edited or deleted.
- **Role:** A set of permissions that can be grouped together and assigned to a user or team.
- **Team:** A group of logged in users who are associated with a view. Each team can be configured to view a particular set of applications and be granted team-level roles/permissions.
- **User:** A user who is identified in an Identity Server configured for the Player system is automatically added into Player upon the first login. In addition, users can be pre-loaded into Player using the Player API.
  > Note: IdentityServer4 is an OAuth 2.0 framework that is used by the Software Engineering Institute to authenticate users. Some Identity servers are configured to authenticate using CAC cards while others are user/password based - depending upon the location of the system.
- **View:** These are the settings associated with building a *view*. A view can be edited, cloned, and deleted.
- **Views:** The collection of content a participant can interact with during a cyber simulation. Depending upon your role (*end-user* view  versus *administrator* view) a view will look different.
