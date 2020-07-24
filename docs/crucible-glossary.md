# Crucible Glossary

A short collection of specialized terms related to the Crucible simulation framework.

## Alloy Terms

- **Event**: Created by the event template, the implementation is the actual running of a simulation.
- **Event template**: Used to associate one or more of the individual Crucible applications, including a Player view template, Caster directory, and Steamfitter scenario template. When an event template is launched, a new event is created.
- **Launch:** The view of a specific definition or lab.  Here, the user can Launch an implementation if no active implementation exists for this user and definition combination.  If an active implementation already exists, then the user can open it in Player or end it.

## Caster Terms
- **Apply:** A summary of deployed Terraform resources. The Terraform definition is: 

  > One of the stages of a run, in which changes are made to real infrastructure resources in order to make them match their desired state. The counterpart of a plan.

- **Destroy:** 

- **Directory:** The top-level construct within project is called directory. A project can contain many directories. Directories contain files that make up a particular Terraform configuration, workspaces that represent a specific instance of that configuration, and sub-directories.
  Enclave

- **Module:** A well-structured set of Terraform resources with input variables and optional outputs.

- **Plan:** A summary of Terraform resources to be deployed or destroyed. The Terraform definition is: 

  > One of the stages of a run, in which Terraform compares the managed infrastructure's real state to the configuration and variables, determines which changes are necessary to make the real state match the desired state, and presents a human-readable summary to the user. The counterpart of an apply.

- **Project:** The top-level construct in Caster is called a project. The project contains the configuration and state of a set of environments within Caster. The main screen of Caster displays a list of the projects available and allows a user to create a new one.

- **Reject:** 

- **Run:** The Terraform definition is:

  > The process of using Terraform to make real infrastructure match the desired state (as specified by the contents of the config and variables at a specific moment in time).

- **Taint:** A flag on a resource that tells Terraform to destroy and recreate a new instance on the next plan and apply cycle.

- **Terraform:** An open source "Infrastructure as Code" tool; Caster is built around the Terraform application (https://www.terraform.io/).

- **Topology:** 

- **Topology Template:**

- **Workspace:**  A workspace represents a specific instance of a deployed Terraform configuration. The same configuration can be used to deploy virtual machines to multiple workspaces that differ only by the values set to certain variables.

## Player Terms

- **Application:**  A website that a user in a view can open within the view or a separate browser tab. See [Player Applications](https://github.com/cmu-sei/crucible/wiki/Player-Applications) for more information.
- **Application Template:** The settings associated with an application that is added to a team's view.  An application template can be created for common apps that are added to a view with default settings that an administrator can override if needed.
For applications that are used often but require slight modifications such as URL variations, _application templates_ are created so administrators can easily add new applications to an event without typing in all of the information required.
- **Focused Application Panel:** The focused app panel displays the selected application in an iFrame. The iFrame points to the URL specified by the _application template_.  The application within the focused app panel is responsible for authentication and content. Player displays content but has no control of the application running within the focused app panel.
- **Notification:** A message sent to a specific user, team, or view from an Administrator or an app that has been given permission.
- **Permission:** A key/value pair that can be created and used by any application given permission within the system. A permission can be assigned to a user or a team. Some permissions such as `SystemAdmin` are read-only. `ExerciseAdmin` and `SystemAdmin` are permanent permissions that cannot be edited or deleted.
- **Role:** A set of permissions that can be grouped together and assigned to a user or team.
- **Team:** A group of logged in users who are associated with a view. Each team can be configured to view a particular set of applications and be granted team-level roles/permissions.
- **User:** A _user_ who is identified in an Identity Server configured for the Player system is automatically added into Player upon the first login.  In addition, users can be pre-loaded into Player using the Player API. 
   
   >**Note:**  **IdentityServer4** is an **OAuth 2.0** framework that is used by the Software Engineering Institute to authenticate users.  Some Identity servers are configured to authenticate using CAC cards while others are user/password based - depending upon the location of the system.
- **View:** The collection of content that a user is allowed to interact with during a cyber simulation.
- **View Template:** The settings associated with building a view.  A view template can be edited, cloned, or deleted.

## Roles

Each application in the Crucible framework has its own roles. Roles are not always the same across the framework. For example, you could be an _administrator_ in Player, but not in Caster or Steamfitter.  

- **Authenticated user:** An individual who is authenticated by Crucible's Identity Server authentication system.
- **Content developer:** An authenticated user who has permissions to create content in a Crucible application.
- **System administrator:** An authenticated user who has full permissions to perform _all_ actions within a Crucible application.

## Steamfitter Terms

- **Result:** A single output that is generated when executing a task against a single topology resource such as a single VM.  The *result* indicates success or failure and typically includes some text describing the outcome.
- **Scenario:** Groupings of predefined tasks that can be executed against a view's VMs. 
- **Scenario template:**  Groupings of predefined tasks that can be used to create scenarios on demand.
- **Task:** An action or command that can be executed against one or more topology resources (for example, VMs).

## Other Terms