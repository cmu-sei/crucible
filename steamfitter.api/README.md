# steamfitter.api

This project provides a restful api for steamfitter functionality in the Sketch/Scenario Player ecosystem.

By default, steamfitter.api is available at localhost:4400, with the swagger page at localhost:4400/swagger/index.html.

# Entity Description
<b>Scenario:</b> A definition of a series of dispatch tasks that can be used to run an exercise

<b>Session:</b> An instantiation of a series of dispatch tasks that run a particular exercise session.

<b>DispatchTask:</b> An individual task that is defined to run on a group of VM's (defined by a VM mask) or that runs against an external API.

<b>DispatchTaskResult:</b> The result from the API or a single VM of running a DispatchTask.  There will be a DispatchTaskResult for each VM on which the DispatchTask was run. If no VM is associated with the DispatchTask, there wil be one DispatchTaskResult.

# DispatchTask Execution
1. Ad-hoc DispatchTasks must have a VmList associated with it.
2. A Session can have a DispatchTask that uses a VmMask, <b>ONLY</b> if the Session is associated with a Player Exercise.
