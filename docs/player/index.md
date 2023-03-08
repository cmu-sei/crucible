<!-- 1. Introduction -->
<!-- 1a. Description of the application/plugin and its purpose -->

# Player

## Overview

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

<!-- 1b. List of features and capabilities -->

## Key Player Concepts hello world

Some key Player concepts are described below.

### **Apps** 

A website a participant in a view can open within Player or in a separate browser tab. A common example of an app used in Player is the Mattermost messaging platform.
### **App template**
 The settings associated with an app that is added to a team's *view*.  An app template can be created for common apps that are added to a view with default settings that an administrator can override if needed.
### **Authentication** 

Player uses `IdentityServer 4` for authenticating users. Authentication gets a user into Player, but setting permissions within the Player Administration view (under **Users**) determines what a user can open and/or edit within Player.
### **Focused Application Panel** 

The focused app panel displays the selected application in an iFrame. The iFrame points to the URL specified by the application template. The application within the focused app panel is responsible for authentication and content. Player displays content but has no control of the application running within the focused app panel.
### **Notification** 
A message sent to a specific user, team, or view from an Administrator or an app that has been given permission.
### **Permission** 
A key/value pair that can be created and used by any application given permission within the system. A permission can be assigned to a user or a team. Some permissions such as `SystemAdmin` are read-only. `ExerciseAdmin` and `SystemAdmin` are permanent permissions that cannot be edited or deleted.
### **Role** 
A set of permissions that can be grouped together and assigned to a user or team.
### **Team** 
A group of logged in users who are associated with a view. Each team can be configured to view a particular set of applications and be granted team-level roles/permissions.
### **User** 
A user who is identified in an Identity Server configured for the Player system is automatically added into Player upon the first login. In addition, users can be pre-loaded into Player using the Player API.
  > Note: IdentityServer4 is an OAuth 2.0 framework that is used by the Software Engineering Institute to authenticate users. Some Identity servers are configured to authenticate using CAC cards while others are user/password based - depending upon the location of the system.
### **View** 
These are the settings associated with building a *view*. A view can be edited, cloned, and deleted.
### **Views** 
The collection of content a participant can interact with during a cyber simulation. Depending upon your role (*end-user* view  versus *administrator* view) a view will look different.

<!-- 2.	Requirements -->
<!-- 2a.	List of hardware/software requirements. -->
<!-- 2b.	List of dependencies and prerequisites. -->
<!-- 3.	Installation -->
<!-- 3a.	Step-by-step instructions for installation. -->
<!-- 4.	Cloud Deployment (if applicable) -->
<!-- 4a.	Overview on how to deploy the application on a cloud environment. -->
<!-- 4b.	Information on cloud-specific configurations and requirements. -->
<!-- 4c.	Add location for helm charts or docker images. -->
<!-- 5.	Configuration -->
<!-- 5a.	Describe how to configure application/plugin. -->
<!-- 5b.	List available options and settings. -->
<!-- 6.	User Guide -->
<!-- 6a.	Overview on how to use application/plugin. -->

# Player Applications

Every Exercise in Player contains *applications*.  Applications are served elsewhere and viewed in a content panel in the Player user interface.  If the application requires authentication, we recommended you configure it to use the same identity server used by Player for the best user experience. The following are applications used in Player:

- Virtual Machines (VM-UI and VM-Console)
- Help desk using osTicket
- Messaging with Mattermost
- General Document Links (PDF, PNG, etc.)

## Working with Applications hello world

All system allows Applications to be opened within a tab on the same screen or popped out into a new browser tab using the arrow.

## Virtual Machines (VM-UI and VM-Console)

The **VM List** is set up to provide links to any virtual machines that the user has permission to view. The tabular system allows the VM to be opened similar to an Application, within a tab on the same screen or popped out into a new browser tab using the icon. Users can also run commands on one or more virtual machines by dragging a box and shift clicking.

### Player VM Console application

- Load VMware consoles
- [Power On/Off VMs](https://cmu-sei.github.io/crucible/player-power-a-vm-on-or-off)
- [Copy/Paste](https://cmu-sei.github.io/crucible/player-copy-and-paste-text)
- [Send files/Mount ISOs](https://cmu-sei.github.io/crucible/player-how-to-upload-files)

## Help desk using osTicket 

**osTicket** (https://osticket.com/), a widely-used open source support ticket system, can be configured and deployed for an exercise. Using this "built-in" help desk, participants can submit help requests to event administrators.

## Messaging with Mattermost 

**Mattermost** (https://mattermost.com/), an open source messaging platform can be configured and deployed to provide an "off the shelf" chat system for users. 

## General document links

Any documents or files  - PDF, PNG, etc. - that can be served on the web can be easily set up as an application for viewing.  This is particularly helpful for displaying instructions, intel, maps, and other information.

<!-- 6b.	Explain user interface. -->
<!-- 6c.	List of functionalities and how to use them. -->

# Player Views

The collection of content a participant can interact with during a cyber simulation is called the _view_. Depending upon your role (*end-user* view  versus *administrator* view) a view will look different.

## View: As seen by the end-user
![player-user-view](../../assets/img/player-user-view.png)

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
![player-admin-view](../../assets/img/player-admin-view.png)

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

# Player Maps

The **Maps** application allows Player users who have View Administrator (*ViewAdmin*) permissions on the view to create, edit, and delete "clickable" maps of systems and environments in a simulation. A common example of a map is a network topology where selecting a system on the topology launches the associated VM.

Assuming that View Administrator permissions have been granted, in Player add the map application to the view.

## Adding the Map application to the view

1. In Player, in your present view, select your user name and then **Edit View**.
2. Under Applications, select **Add New Application** then **Templates**, then **Map**.
3. The Select Map dropdown and the New Map icon appear in the right pane.

## Creating a new map
![player-new-map](../../assets/img/player-new-map.png)

1. In the newly created Map application, in the right pane, select **New Map**.
2. Complete the following fields:
   - **Name:** the name of the map.   
   - **Select Image:** from the dropdown, select an image. The images you see here are images that have been previously uploaded to the view by the View Admin and assigned to a team.   
   - **External Image URL:** enter the URL of an external image if no image has been attached to the view or if you want to use a different image than what is available.   
   - **Teams:** only the teams selected here will see the new map.   
   > Note that you can select more than one team here.
3. Click **Submit**. The image of the new map appears in the right pane.

## Editing a map

1. In Player's left navigation pane, click the **Map application**. 
2. In the right pane, select a map from the **Select Map** dropdown.
3. Click the **Edit** icon. From here, you can:
   - **Edit Properties:** allows you to change the name, images, and teams of the map.
   - **Discard Changes:** allows you to discard changes you made to the map; for example, adding a click point.
   - **Save:** saves your map.
   - Click in the map to **add a click point**.

### Adding a click point

A _click point_ is a location on the map that when clicked by Player user launches a resource like a virtual machine in a new tab. To add a click point to the map:

1. In Player's left navigation pane, click the **Map application**. 
2. In the right pane, select a map from the **Select Map** dropdown.
3. Click the **Edit** icon.
4. Click anywhere in the map to launch the **Add Click Point** modal. If your map is a network topology diagram with network elements--routers, switches, firewalls, servers, etc.--then you may want to click on an element that represents the VM resource you want the user to launch. 
   - **Radius:** by default, the value is 3.
   - **Resource:** this is the virtual machine (or another map) that launches when clicked. The VMs that appear here are the VMs from the VM application in the current view.
   - **Enter Custom Resource URL:** enable this if you want to link to something other than the view's VMs and maps that are available above. For example, you could place a click point labeled "Linux Help" that links to relevant Linux documentation.
   - **Label:** this is how the click point is labeled on the map. If your click point is over top of an element that launches a Windows 10 Administrator Workstation, then it makes sense to label it "Win 10 Admin".
5. Click **Save** to save the new click point in the map.
6. Click **Save** again to save the map.

### Deleting a click point

1. In Player's left navigation pane, click the **Map application**. 
2. In the right pane, select a map from the **Select Map** dropdown.
3. Click the **Edit** icon.
4. Click an existing click point.
5. In the Edit Click Point modal, click **Delete**.

## Deleting a map
1. In Player's left navigation pane, click the **Map application**. 
2. In the right pane, select a map from the **Select Map** dropdown.
3. Click **Delete Map**.

<!-- 7.	Admin User Guide -->
<!-- 7a.	Overview of admin user interface. -->
<!-- 7b.	Explain the administration functionality. -->
<!-- 7c.	Step-by-step instructions on how to perform administrative tasks. -->
<!-- 8.	Troubleshooting -->
<!-- 8a.	Common issues and how to resolve them. -->
<!-- 9.	FAQ -->
<!-- 9a.	List of frequently asked questions and answers. -->
<!-- 10.	License -->
<!-- 10a.	Information about open-source license used. -->
