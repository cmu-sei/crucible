# Caster Workspace

A *workspace* represents a specific instance of a deployed Terraform configuration. The same configuration can be used to deploy virtual machines to multiple workspaces that differ only by the values set to certain variables. For example: a configuration for an enclave in a Cyber Flag exercise may be defined once, and then deployed to `flag00` through `flag30` workspaces - each creating a copy of the enclave. 

Workspaces can contain files, which extend the configuration of the directory for that specific workspace. This might include files specifying values for variables defined in the directory, or additional resources to be deployed only for that workspace.

A workspace is where users:

- Create an instance of a Terraform configuration
- Run their plans (_Runs_ are a specific instance of a Terraform plan; explained [here](./caster-run-plan-apply)
- Manage the differences and the variables in their environments

Users can access workspaces from a project's navigation pane in Caster. Users can add additional files, but _not_ additional directories, to a workspace. The workspace view allows users to see all the runs that have been planned and applied. Runs shaded in red are destroyed operations, while runs in white signify various other status classifications.

Users can `Plan`, `Destroy`, `Apply`, `Taint`, and `Reject` operations in real time in the workspace view.
