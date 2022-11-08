## Steamfitter Scenarios

Scenarios are groupings of predefined tasks that can be executed against a required view's VMs.

After the tasks are defined in the scenario template the next step is to create the *scenario*.  Keep in mind that the scenario template is the *plan*; the scenario is the actual *instance*. Multiple scenarios can be created from one template; those scenarios can then be associated with different Player views.  

In the **Scenarios** screen, the created scenario is named `scenario template name` - `your username` by default. 

Select the newly added scenario from the scenario list. Now, you can edit the **Name** and **Description** and you also have the **View** dropdown. A scenario must be associated with one specific Player view. 

**Start** and **End** dates and times can be changed here.

> As you would expect, the same tasks you attached to the scenario template appear in the scenario. Editing them in the scenario only changes them in this scenario. If you want the tasks to be changed for all scenarios based upon the template, then you will have to edit the tasks in the scenario template.

### Starting a Scenario

In order for tasks to execute, a task has to be started. 

> If the Start Scenario button is not enabled that means that you have not associated it to a Player view.

Once started, the scenario status is now **Active** and a new **Execute** option is available in the Task context menu.

After tasks have been executed results are displayed in the task details. Each task is expandable. You will see a result listed for every single time that task gets executed. 

### Ending a Scenario

Scenarios can also be ended. When a scenario is started, the Start Scenario button changes to **End Scenario Now**.

### A note about StackStorm

Behind the scenes Steamfitter uses StackStorm ([stackstorm.com](https://stackstorm.com/)) to execute these tasks.  StackStorm is an open source application that can connect applications, services, and workflows. Steamfitter uses StackStorm to send commands to the guest VMs using the StackStorm vSphere Action Pack, so that none of that communication occurs over the network.