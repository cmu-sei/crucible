## Steamfitter Tasks

A *task* is an action or command that can be executed against one or more topology resources (that is, a VM). Each task has a *result*. A result is a single output that is generated when a task is executed against a single topology resource - like a single VM. A result indicates success or failure and typically includes some text describing the outcome. 

A task has no results until it is executed. 

A task can have multiple results:

- a task defined to run against one VM will have one result for each time the task is executed
- a task defined to run against multiple VMs will have on result per VM - each time the task is executed. So, a task that runs against four VMs and is executed three times yields 12 results.

### Adding a Task

**Name:** What this task is supposed to do.

**Description:** Additional details about what the task does.

#### Action

- **Select an Action:** Power on a VM, power off a VM, read a file, etc. An *action* is the name StackStorm gives to a single Task/Command.

- **Trigger Condition:** 
  - **Time:** A *timed* trigger is executed automatically after a set *delay* in seconds. A timed trigger can have multiple *iterations* executed on a specific interval.  
  - **Manual:** A *manual* trigger condition is executed by manual intervention - clicking a button, for example, to fire off a task. It's up to a user to intervene to execute the task. 
  - **Completion:** When the parent task completes the dependent task runs - regardless of success or failure.
  - **Success:** If the expected output is contained in the actual output then the dependent task runs.
  - **Failure:** Only runs if the expected output is not contained within the actual output. 

**Expected Output:** Whatever you type here, if the actual output contains that text, then it is considered a success. If the output does not contain what is typed here, then it is considered a failure.

#### Delay / Iteration / Expiration

- **Delay:** Set in seconds before executing the task executes.
- **Number of Iterations:** An *iteration* is an execution of a task when the task is configured to iterate for *x* number of times. Enter the number of times you want the task to execute here.
- **Interval Between Iterations:** The time in seconds in between iterations. 
- **Iteration Termination:** 
  - **IterationCountTask:** The task will execute until exactly the number of iteration times specified above; regardless of whether the task succeeds or fails.
  - **UntilSuccess:** The task will iterate until the command has a successful completion.
  - **UntilFailure:** The task will iterate until the command fails.
- **Expiration Timeout:** The time, in seconds, where if no response has been received the task expires (times out).  

#### VM Selection

- **VM Mask:** Tasks will run against Player VMs that include the text typed here. 
- **Choose Actual VMs:** Enable Choose Actual VMs to select specific VMs; these are the VMs found in the selected Player view. 

#### Task Menu and Dependent Tasks

Clicking the Task Menu on the newly created task will give you the context menu for the task: **Edit**, **Copy**, **Cut**, **New**, **Delete**, and **Execute**. Selecting **New** here, however, creates a new *dependent* task. A dependent task does not execute until the condition on the parent task is met.

A task can be copied and pasted from any other scenario template, scenario, and task.