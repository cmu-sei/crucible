# Run, Plan, and Apply

A *run* is a specific instance of the Terraform *plan* and *apply* process for a workspace. The run is how the configuration in a directory is instantiated into deployed resources within a workspace. Upon opening a workspace, a list of runs is displayed. This is where plan or destroy operations are started. 

*Plan* and *apply* are specific Terraform terminologies. Every run is made up of a plan and an apply step. 

## Plan

Clicking Plan will create a new Run and execute the "terraform plan" command on the given configuration. This raw Terraform output is shown to the user, and describes:

- What actions Terraform will take
- What resources will be created
- What resources will be changed
- What resources will be destroyed

A plan shows the user what is going to be deployed.

This output always ends with a summary of the form `Plan: x to add, y to change, z to destroy`. The user reviews this and chooses to apply or reject the plan. 

- Choosing **apply** creates an apply for the run and executes `terraform apply` on the previously generated plan. This causes Terraform to actually make the changes described. 
- Choosing **reject** invalidates the plan. No changes are made to the infrastructure. 

## Apply

_Apply_ takes a run, executes `Terraform apply` on the previously generated plan and deploys the resources for a workspace. The `Apply` command:

- Deploys a workspace run
- Releases plan tools such as network resources and virtual machines into VCenter

Within the workspace view users can see all the runs that have been planned and applied.

## Destroy and Taint

### Destroy

Selecting destroy instead of plan is largely the same, except the plan generated is one that will destroy all of the previously deployed resources in the workspace, rather than making the infrastructure match the current configuration. That is, _Destroy_ creates a plan that will destroy all of the previously deployed resources in a workspace.

>**Note:** If a resource is defined in the configuration and created in a run and then deleted from the configuration, it is destroyed upon the next plan or destroy run. This is because a Terraform run always tries to match the infrastructure to the current configuration. 

>**Note:** Only one run can be in progress at a time per workspace. Terraform locks the state of the workspace and only a single operation can be performed at a time.
>Developers may wish to break up large deployments into multiple directories and workspaces to operate on different parts of the deployments simultaneously. 
>For example: User enclaves may be broken out so developers can perform actions on other parts of a network without (potentially) waiting a long time to redeploy user machines.

The workspace view allows users to see a table with all the runs that have been planned and applied within that directory. Runs highlighted in red are destroyed operations.

Within the workspace view users can click `Destroy` to destroy live Terraform applications.

This infrastructure as code approach is different than many developers may be used to. The general approach here is to define a configuration and apply it in its entirety, rather than selecting individual pieces to be deployed. There are some ways to target individual pieces of a configuration, but they are recommended by Terraform to be the exception rather than the rule and are not yet fully implemented in Caster.

### Taint

_Taint_ is a flag on a resource that tells Terraform to destroy and recreate a new instance on the next plan-and-apply cycle.

Taint allows users to redeploy resources. For example, if a user needs to redeploy a series of virtual machines, the user can:

1. Taint these resources
2. Run another plan-and-apply cycle that will redeploy the instance as if it was new from a template

Some resources can't be tainted, however. 

Users can taint resources within the workspace view. Once a resource is tainted it will display in red shading. Users can also easily access the `Untaint` command while in workspace view before running another plan-and-apply cycle if they change their mind and decide to keep the resource.