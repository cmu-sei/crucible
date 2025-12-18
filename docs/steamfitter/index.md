# ![Steamfitter Logo](../assets/steamfitter-logo.svg){: style="height:75px;width:75px"} **Steamfitter:** Crafting a Scenario

## Overview

**Steamfitter** gives content developers the ability to create scenarios consisting of a series of scheduled tasks, manual tasks, and injects which run against virtual machines during an event. These scenarios enable the content developer to automate assessments and configurations.

In the Steamfitter UI, four major functional sections help you manage content:

- **Scenario Templates:** Organize tasks into groups that you can reuse on demand.
- **Scenarios:** Display all planned, running, and completed scenarios.
- **Tasks:** Let users create and execute ad-hoc tasks.
- **History:** Shows the results of all tasks executed by all users.

![Four functional areas of Steamfitter](img/4-functional-sections.png)

## StackStorm Integration

Behind the scenes, Steamfitter uses [StackStorm](https://stackstorm.com/) to execute Steamfitter tasks. StackStorm is an open-source application that connects applications, services, and workflows. Steamfitter uses StackStorm to send commands to guest VMs via the vSphere Action Pack. This ensures that no communication for the tasks run by StackStorm occurs over the network.

## Permissions and Roles

### Permissions

Sets of *permissions* control access to features in Steamfitter. Permissions apply globally or per Scenario Template or Scenario.

Examples of global permissions include:

- `CreateScenarioTemplates`: Can create new Scenario Templates
- `ViewScenarios`: Can view all Scenarios and their Users and Groups
- `ManageUsers`: Can change User accounts

Users who have View or Manage permissions for an administration function (for example, `ViewScenarioTemplates` or `ManageGroups`) can open the **Administration** area. Select the **gear** icon in the top-left corner of the Steamfitter screen to access it. They see only the sections their permissions allow in the sidebar menu.

View all available permissions in the **Roles** section of the **Administration** area.

The partial screen capture below shows the Steamfitter Administration screen with the Roles section and the system roles highlighted.

![Steamfitter administration area with roles selected](img/admin-roles-area.png)

### Roles

Roles group permissions and apply them to *users*. Steamfitter includes **System Roles** that define global access across the application.

#### System Roles

Each user can have one *system role* that provides global permissions across Steamfitter. The default system roles are:

- **Administrator:** Has all permissions in the system.
- **Content Developer:** Has `CreateScenarioTemplates`, `CreateScenarios`, `ExecuteScenarios`, `ManageTasks`, `ViewUsers`, and `ViewGroups` permissions. Users with this role create and manage their own Scenario Templates and Scenarios but can't change global settings or other users' content.
- **Observer:** Has `ViewScenarioTemplates` and `ViewScenarios` permissions. Users with this role can view all Scenario Templates and Scenarios but can't make changes.

Users who have the `ManageRoles` permission can create custom system roles in the Roles section of the Administration area.

## Administrator Guide

Steamfitter is primarily an administrative tool. Content developers and "Range tech" types of admins manage templates and scenarios, while read-only users can observe assigned projects without making changes.

### Scenario Templates

![scenario template](img/scenario-templates.png)

Scenario templates group predefined tasks so you can create scenarios and run them on demand. To create a new scenario template, provide a **Name**, **Description**, and **Duration**.

After adding the new scenario template, you can **Create a Scenario**, **Copy** a scenario template to create a duplicate, **Edit** it, and **Delete** it.

Add tasks in the Scenario Templates screen by clicking the **+** icon. Complete the same task information as you did to create a new task.

### Scenarios

![scenarios](img/scenarios.png)

Scenarios group predefined tasks that you can execute against a required view's VMs.

A user with the appropriate permissions defines tasks in the scenario template and then creates the *scenario*. Think about it like this--the scenario template is the *plan*; the scenario is the actual *instance*. One template can yield multiple scenarios. You associate these scenarios with one or more Player views.

In the **Scenarios** screen, Steamfitter names the created scenario `scenario template name` - `your username` by default.

Select the newly added scenario from the scenario list. You can edit the **Name** and **Description** and use the **View** dropdown. Associate each scenario with one specific Player view.

You can change the **Start** and **End** dates and times here.

!!! caution

    The scenario inherits the tasks you attached to the scenario template. If you edit them in the scenario, the changes apply only to that scenario. To update every scenario based on the template, edit the tasks in the scenario template.

#### Stand-alone Tasks on the Scenario

Steamfitter provides a stand-alone Tasks page that allows users without full Steamfitter permissions to run scenario tasks and view their results. You can access this page using either `scenario/{scenarioId}` or `/view/{viewId}`.

The scenario-based URL shows the manually executable tasks for a specific scenario and works reliably in all cases—use it whenever possible.

The view-based URL supports cloned Player views by substituting the active view ID and displaying the tasks for the scenario mapped to that view. However, it fails if more than one scenario points to the same view. You can add either URL as an application within a Player view.

A user with the appropriate permissions defines tasks under a scenario. In Steamfitter, on the **Scenarios** tab with a scenario open:

1. Click the **Task Menu**, then **Edit**.
   ![Scenario Task Edit icon](img/task-menu-icon.png)
2. On the **Edit Task** screen:
   - **User Executable:** Check if you want Player to display this task on the Player view. When a user clicks it in the Player view, the system runs the action configured for that task.
   - **Repeatable:** Check if you want to allow Steamfitter to execute this task more than once even when successful. When unchecked, Steamfitter only permits you to execute it once, and then the task is unavailable.

!!! info

    When a task is not user executable, whether it runs depends upon its configuration. For example, you may have a task called “Wait for VM to become available” – Trigger Condition of Time. In this case, the task starts automatically some number of seconds after the scenario starts, using a delay to give the VM time to initialize before other tasks are permitted to run.

##### Subtasks

A task can have *subtasks*. A subtask is a smaller dependent step that belongs to the task. Only the main task appears on the Player view because the subtasks are not user executable. Subtasks run automatically in sequence: when the main task completes, the first subtask runs, then the next subtask, and so on. The Player view only needs to show the main task; the subtasks stay behind the scenes and execute automatically.

In Steamfitter, on the **Scenarios** tab with a scenario open, click the **Task Menu**, then **New**.

##### Points

Tasks can optionally have *points* assigned to them. For example, one task may be worth 1 point, another worth 3 points, and a third 0 points. In the Player view, only tasks with points contribute to the score panel. Task points and the Player view scoring panel exist to support grading tasks and show their contribution to an overall score.

If no tasks in the scenario have points assigned, the score panel does not appear. In this case, the Player view displays only a list of tasks that users can run directly.

1. In Steamfitter, on the **Scenarios** tab with a scenario open, click the **Task Menu**, then **Edit**.
2. Under **Scoring**, assign the number of points you wish to award the user for completing the task successfully.

!!! tip

    In the Player view, in the left navigation pane, you can right-click the Task icon and open it in a new tab to view the full URL. This URL includes the Player view ID, which Steamfitter uses to look up and display the tasks associated with that view.

##### What Happens When More Than One Scenario Uses the Same View?

When more than one scenario shares the same Player view, Steamfitter can't determine which scenario's tasks to display. When this happens, the Tasks page appears blank because the system has no way to resolve the ambiguity.

To prevent confusion, the system now displays an error message explaining that multiple scenarios share the same view and that it cannot display tasks for that view.

![Error message stating to use scenario id](img/error-message-same-viewid.png)

To work around this issue, open the Tasks page using a scenario-based URL instead of a view-based URL.

Instead of `/view/{viewId}` use `/scenario/{scenarioId}`. This URL tells Player exactly which scenario's tasks to display and avoids the conflict.

!!! tip

	Steamfitter lists tasks alphabetically by name. To control their display order, prefix task names with labels such as "Task 01," "Task 02," and so on.

#### Starting a Scenario

Start the scenario to execute its tasks.

!!! info

    If the **Start Scenario** button stays disabled, you have not associated the scenario with a Player view.

When you start the scenario, Steamfitter marks it **Active** and adds a new **Execute** option to the Task context menu.

After you execute tasks, the results display in the task details. Each task is expandable. You will see a result listed for every execution of the task.

#### Ending a Scenario

You can also end scenarios. After you start a scenario, the **Start Scenario** button changes to **End Scenario Now**.

### Tasks

A *task* is an action or command that executes against one or more topology resources (that is, a VM). Each task has a *result*. The result captures the output generated when the task runs against a single topology resource—such as one VM—and indicates success or failure. The output typically includes text describing the outcome.

A task has no results until you execute it.

A task can produce multiple results:

- a task defined to run against one VM generates one result for each execution
- a task defined to run against multiple VMs generates one result per VM each time the task runs. For example, a task that targets four VMs and runs three times yields 12 results.

#### Adding a Task

- **Name:** Describe what this task does.
- **Description:** Provide additional details about the task.

##### Action

- **Select an Action:** Power on a VM, power off a VM, read a file, etc. StackStorm refers to a single task/command as an *action*.
- **Trigger Condition:**

       - *Time:* Steamfitter executes a timed trigger automatically after the specified *delay* in seconds. A timed trigger can run multiple *iterations* on a specific interval.
       - *Manual:* A manual trigger runs only when a user intervenes—for example, by clicking a button to fire off a task.
       - *Completion:* When the parent task completes, the dependent task runs—regardless of success or failure.
       - *Success:* If the actual output contains the expected output, the dependent task runs.
       - *Failure:* Runs only when the actual output does not include the expected output.

- **Expected Output:** Enter the text that indicates success. If the actual output contains that text, the task counts as a success; otherwise it counts as a failure.

##### Delay / Iteration / Expiration

- **Delay:** Specify the number of seconds before the task executes.
- **Number of Iterations:** Enter how many times you want the task to execute when you configure it to iterate.
- **Interval Between Iterations:** Specify the time in seconds between iterations.
- **Iteration Termination:**

      - *IterationCountTask:* The task executes exactly the number of iteration times specified above, regardless of success or failure.
      - *UntilSuccess:* The task iterates until the command completes successfully.
      - *UntilFailure:* The task iterates until the command fails.

- **Expiration Timeout:** The time in seconds where, in the absence of a response, the task expires (times out).

##### VM Selection

- **VM Mask:** Steamfitter runs tasks against Player VMs that include the text you enter here.
- **Choose Actual VMs:** Enable Choose Actual VMs to select specific VMs from the Player view.

#### Task Menu and Dependent Tasks

Clicking the Task Menu on the newly created task opens the context menu: **Edit**, **Copy**, **Cut**, **New**, **Delete**, and **Execute**. Selecting **New** here creates a new *dependent* task, which waits to run until the parent task meets its condition.

You can copy and paste a task between scenario templates, scenarios, or other tasks.

### History

The default History view shows task results for the current user sorted in reverse chronological order. However, you can also view history by **User**, **View**, and **VM**. Sorting and filtering are also available in the history results.

## Glossary

This glossary defines key terms and concepts used in the Steamfitter application.

**Delay:** set in seconds before the task executes.

**Execute:** to carry out the task.

**Expiration:** when a task times out.

**Iteration:** an execution of a task given the task specification to iterate x number of times.

**Scenario:** groupings of predefined tasks that execute against a required view's VMs.

**Scenario Template:** location for grouping predefined tasks; scenario templates create scenarios that can run on-demand.

**StackStorm:** an open source application that can connect applications, services, and workflows.

**Task:** an action or command that executes against one or more topology resources.
