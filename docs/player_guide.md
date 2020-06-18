# Player Guide

Welcome to the *Player Guide*. Player is the platform where individual participants on a team go to view content during a cyber simulation. In Player, participants navigate between various Crucible framework applications, view administrators set which teams can see what applications, and notifications are sent and received.

## Understanding key Player concepts 

Some key Player concepts are described below.

- **Authentication:** Player uses `IdentityServer 4` for authenticating users. Authentication gets a user into Player, but setting permissions within the Player Administration view (under **Users**) determines what a user can open and/or edit within Player.
- **View template:** These are the settings associated with building a *view*. A view template can be edited, cloned, and deleted.
- **Views:** The collection of content a participant can interact with during a cyber simulation. Depending upon your role (*end-user* view  versus *administrator* view) a view will look different. 
- **App template:** The settings associated with an app that is added to a team's *view*.  An app template can be created for common apps that are added to a view with default settings that an administrator can override if needed.
- **Apps:** A website a participant in a view can open within Player or in a separate browser tab. A common example of an app used in Player is the Mattermost messaging platform.

## View: As seen by the end-user

### Collapsible navigation panel

The left application navigation panel can be collapsed to provide additional display space.

### Application navigation bar

In the view, the navigation bar on the left contains applications.

### Focused app panel

The focused app panel displays the selected app.

### Notifications

Receive and read notifications here.

> Tip! If your browser is set up to allow notifications you can receive Player notifications that way too.

### Top bar

The top bar displays the current *view name*, *team*, and the *menu select* dropdown.

Player fully supports users who are on multiple teams.  Any such user, when logged in, can switch their view by using the *team* drop-down. 

#### Menu select

Log off here. If you are a view administrator, you have the option to edit the view template from here.

## View: As seen by the administrator

A Player view administrator will see the **Administration navigation bar** on the left. To switch to the administrator view in Player if you have the appropriate permissions:

In the top-left corner, click the dropdown next to your user name, then **View Administration**.

> **Important!** Only users who have the SystemAdmin permission can view the Administration screen and  the Administration nav bar (View Templates, Users, Application Templates, Roles / Permissions).

### View Templates

View Templates is where a Player view administrator adds a new view template and browses existing view templates. For step-by-step instructions on how to create a new view template, see [Player How to: Create a new View Template](https://github.com/cmu-sei/crucible/wiki/Player-How-to:-Create-a-new-View-Template).

### Users

Users are only available in Player after they have successfully authenticated via the identity server and opened Player in their browser. Users and/or teams can be assigned any set of **Permissions:** 

- **SystemAdmin:** can edit anything in Player; SystemAdmin permissions are given by existing SystemAdmin.
- **ViewAdmin:** can edit anything within a View template that they have permissions.

A SystemAdmin creates the View template and assigns ViewAdmin permissions to specific teams who can now edit that View template.

Users and/or teams can be assigned to a **Role**, which is a group of permissions. More about roles as future Player development is completed. Only a SystemAdmin can create roles. 

### Application Templates

Think of *application templates* as "helpers" for adding new or common applications to Player. For example, the Virtual Machines application template contains several URLs. Including them in a template means that these values may be used over and over as part of a template--rather than manually entering the same information over and over again with each new view template. 

In the Player system, creating a new application template is a relatively rare occurrence when compared to creating a view template. Create the application templates first because you will use them on each view template.

For step-by-step instructions on how to create a new application template, see Player How to: Create a new App Template.
