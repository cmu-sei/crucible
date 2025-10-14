---
title: Gameboard - Permissions
---

# Permissions

Gameboard uses a role-based permissions system to define what functions each role can access.

There are five roles: **Admin**, **Director**, **Support**, **Tester**, and "participant" (i.e., no role). The four elevated roles have cumulative permissions: Testers have the least access; Support has more access than Testers; Directors have more than Support; and Admins have full access.

- **Admin**: Full permissions to access all parts of the application and change user permissions.
- **Director**: Tester and Support permissions, plus the ability to create/edit/delete games, manage game settings, deploy resources, manage team sessions and scores, and edit some site-wide settings (e.g., announcements, practice area).
- **Support**: Tester permissions, plus the ability to view additional information about teams/games, manage enrolled players, view reports, and manage support tickets.
- **Tester**: Can play games outside of the execution window and view hidden games for testing game functionality.

If you're Support, Director, or Admin, you can go to **Admin**, **Permissions** in the top navigation to check your role and see what each role can do.

Only Admins can change the permissions of other users and at least one Admin is required. To assign roles, go to **Admin**, **Permissions** in the top navigation.

The table below outlines the permissions associated with different roles in the Gameboard across various functions.

| Permission / Role                                                                                                                          | Admin   | Director | Support | Tester  |
| ------------------------------------------------------------------------------------------------------------------------------------------ | ------- | -------- | ------- | ------- |
| ADMIN                                                                                                                                      |         |          |         |         |
| Admin Area: Access the Admin area                                                                                                          | **Yes** | **Yes**  | **Yes** | No      |
| Create/edit sponsors: Create and edit sponsor organizations                                                                                | **Yes** | **Yes**  | No      | No      |
| Manage API Keys: Can generate API keys for any user and revoke their access                                                                | **Yes** | No       | No      | No      |
| Manage system notifications: Create, edit, and delete notifications which appear at the top of the app                                     | **Yes** | **Yes**  | No      | No      |
| GAMES                                                                                                                                      |         |          |         |         |
| Create/edit/delete games: Create, edit, and delete games. Add and remove challenges, set their scoring properties, and add manual bonuses. | **Yes** | **Yes**  | No      | No      |
| Set players to ready: Change player status to ready/not ready in sync-start games                                                          | **Yes** | **Yes**  | No      | No      |
| View hidden games and practice challenges: View games and practice challenges which have been hidden from players by their creator         | **Yes** | **Yes**  | **Yes** | **Yes** |
| PLAY                                                                                                                                       |         |          |         |         |
| Ignore registration/execution windows: Ignore registration and execution window settings when enrolling in and starting games              | **Yes** | **Yes**  | **Yes** | **Yes** |
| Ignore session reset settings: Reset their session, even in games where session reset is prohibited                                        | **Yes** | **Yes**  | **Yes** | **Yes** |
| Select challenge variants: Choose any variant of a challenge when deploying (rather than random assignment)                                | **Yes** | **Yes**  | **Yes** | **Yes** |
| PRACTICE                                                                                                                                   |         |          |         |         |
| Practice Area: Edit settings for the Practice Area                                                                                         | **Yes** | **Yes**  | No      | No      |
| REPORTS                                                                                                                                    |         |          |         |         |
| Reports: Run, view, and share reports                                                                                                      | **Yes** | **Yes**  | **Yes** | No      |
| SCORING                                                                                                                                    |         |          |         |         |
| Award manual bonuses: Award manual bonuses to individual players or teams                                                                  | **Yes** | **Yes**  | No      | No      |
| Revise scores: Manually initiate re-ranking of games and regrading of challenges                                                           | **Yes** | **Yes**  | No      | No      |
| View scores live: View scores for all players and teams (even before the game has ended)                                                   | **Yes** | **Yes**  | **Yes** | No      |
| SUPPORT                                                                                                                                    |         |          |         |         |
| Edit Support settings: Edit support settings (e.g. support page greeting)                                                                  | **Yes** | **Yes**  | **Yes** | No      |
| Manage tickets: Manage, edit, assign, and respond to tickets                                                                               | **Yes** | **Yes**  | **Yes** | No      |
| View tickets: View all tickets in the app                                                                                                  | **Yes** | **Yes**  | **Yes** | No      |
| TEAMS                                                                                                                                      |         |          |         |         |
| Administer sessions: Manually end and extend team play sessions                                                                            | **Yes** | **Yes**  | No      | No      |
| Approve name changes: Approve name change requests for users and players                                                                   | **Yes** | **Yes**  | **Yes** | No      |
| Create/delete challenge instances: Start and purge an instance of a challenge on behalf of any team                                        | **Yes** | No       | No      | No      |
| Deploy game resources: Deploy virtual on behalf of players through the Admin section                                                       | **Yes** | **Yes**  | No      | No      |
| Enroll Players: Enroll players in games on their behalf                                                                                    | **Yes** | **Yes**  | **Yes** | No      |
| Observe: See information about all active challenges and teams                                                                             | **Yes** | **Yes**  | **Yes** | No      |
| Send announcements: Send announcements to all players (or individual players and teams)                                                    | **Yes** | **Yes**  | No      | No      |
| USERS                                                                                                                                      |         |          |         |         |
| Assign roles: Assign roles to other users                                                                                                  | **Yes** | No       | No      | No      |
| Create users manually: Create and edit users manually (currently available only as an API call)                                            | **Yes** | No       | No      | No      |

After logging into Gameboard, but before playing a game or completing a lab users have to *enroll* in the game lobby. Users log into Gameboard, select a game on the **Home** page, and **Enroll** and **confirm** to start the session. The Enroll button appears when the user has set a display name and a sponsoring organization in their Profile and registration for that game is "open" (open and close dates and times are defined in the Admin Game Settings).

If a user has an elevated role, then the **Admin Enroll** button appears next to the **Enroll** button. See the screen print below.

**Admin Enroll** allows a user with an elevated role to bypass the restrictions of time for registration, but it does *not* allow the user to bypass display name and sponsoring organization requirements. Those still need to be set prior to game play. **Admin Enroll** is useful for testing, troubleshooting, and customer support purposes.

!!! note

     If you do have access to the **Admin Enroll** button, it behaves *identically* to the standard **Enroll** button for you. If you can see both **Enroll** and **Admin Enroll**, it does not matter which button you select.

*Enroll and Admin Enroll:*

![enroll vs. admin enroll](img/enroll-admin-enroll.png)
