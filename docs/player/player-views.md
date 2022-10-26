# Player Views

The collection of content a participant can interact with during a cyber simulation is called the _view_. Depending upon your role (*end-user* view  versus *administrator* view) a view will look different.

## View: As seen by the end-user
![player-user-view](/docs/assets/img/player-user-view.png)

### Collapsible navigation panel

The left application navigation panel can be collapsed to provide additional display space. This side-bar is configurable per team by an administrator.

### Application navigation bar

In the view, the navigation bar on the left contains applications.

### Focused app panel

The focused app panel displays the selected app.

### Notifications

Receive and read notifications here.

> Tip! If your browser is set up to allow notifications you can receive Player notifications that way too.

### Top bar

The top bar displays the current *view name*, *team*, and the *menu select* dropdown (your username in the top right).

Player fully supports users who are on multiple teams.  Any such user, when logged in, can switch their team by using the *team* drop-down. 

#### Menu select

Log off here. If you are a view administrator, you have the option to edit the view from here. The option to enable Dark Theme is here too.

## View: As seen by the administrator
![player-admin-view](/assets/img/player-admin-view.png)

A Player view administrator will see the **Administration navigation bar** on the left. To switch to the administrator view in Player if you have the appropriate permissions:

In the top-right corner, click the dropdown next to your user name, then **Administration**.

> **Important!** Only users who have the SystemAdmin permission can view the Administration screen and the Administration nav bar (Views, Users, Application Templates, Roles / Permissions).

### Views

Views is where a Player view administrator adds a new view and browses existing views. For step-by-step instructions on how to create a new view, see [Player How to: Create a new View](./player-create-new-view.md).

### Users

Users are only available in Player after they have successfully authenticated via the identity server and opened Player in their browser. Users and/or teams can be assigned any set of **Permissions:** 

- **SystemAdmin:** can edit anything in Player; SystemAdmin permissions are given by existing SystemAdmin.
- **ViewAdmin:** can edit anything within a View that they have permissions.

A SystemAdmin creates the View and assigns ViewAdmin permissions to specific teams who can now edit that View.

Users and/or teams can be assigned to a **Role**, which is a group of permissions. More about roles as future Player development is completed. Only a SystemAdmin can create roles. 

### Application Templates

Think of *application templates* as "helpers" for adding new or common applications to Player. For example, the Virtual Machines application template contains several URLs. Including them in a template means that these values may be used over and over as part of a template--rather than manually entering the same information over and over again with each new view. 

In the Player system, creating a new application template is a relatively rare occurrence when compared to creating a view. Create the application templates first because you will use them on each view.

For step-by-step instructions on how to create a new application template, see [Player How to: Create a new App Template](./player-create-new-app-template.md).
