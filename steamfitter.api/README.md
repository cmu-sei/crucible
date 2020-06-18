# steamfitter.api

This project provides a restful api for steamfitter functionality in the Sketch/Scenario Player ecosystem.

By default, steamfitter.api is available at localhost:4400, with the swagger page at localhost:4400/swagger/index.html.

# Entity Description
<b>ScenarioTemplate:</b> A definition of a series of dispatch tasks that can be used to run a view

<b>Scenario:</b> An instantiation of a series of dispatch tasks that run a particular view.

<b>Task:</b> An individual task that is defined to run on a group of VM's (defined by a VM mask) or that runs against an external API.

<b>Result:</b> The result from the API or a single VM of running a Task.  There will be a Result for each VM on which the Task was run. If no VM is associated with the Task, there wil be one Result.

# Task Execution
1. Ad-hoc Tasks must have a VmList associated with it.
2. A Scenario can have a Task that uses a VmMask, <b>ONLY</b> if the Scenario is associated with a Player View.
