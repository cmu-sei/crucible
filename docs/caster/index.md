# ![Caster Logo](../assets/img/crucible-icon-caster.svg){: style="height:75px;width:75px"} **Caster:** Coding a Topology

## Overview

**Caster** is the primary deployment component of the Crucible simulation framework. [Terraform](https://www.terraform.io/), an open-source Infrastructure-as-Code tool, underlies Caster.

Caster provides a web interface that gives exercise developers a powerful and flexible way to create, share, and manage topology configurations.

Initial versions of Caster focused on a web front-end for raw Terraform configurations and outputs. This gave advanced developers easier access to a powerful deployment tool. Targeted improvements to the experience for these users are coming in the future. Eventually, this system will underpin a more user-friendly interface that will allow the piecing together of configurations with less or no writing of Terraform code directly.

Caster gives experts the control they need, while also making it easy for beginners to use expert setups or create simple ones on their own.

### Terraform Integration

For more information on native Terraform constructs used in Caster, please refer to the [Terraform documentation](https://www.terraform.io/docs/index.html).

## Administrator Guide

### Users

![Caster users](../assets/img/caster-users.PNG)

Users are only available in Player after they have successfully authenticated via the Identity server and opened Player in their browser. **Permissions** apply to users and/or teams.

#### Assign Roles

**Roles** are groups of permissions. Only a SystemAdmin can create roles and assign users and/or teams to them.

#### Assign Permissions

- **SystemAdmin:** can edit anything in Caster; SystemAdmin permissions are given by existing SystemAdmin.
- **ContentDeveloper:** can edit anything within a Directory for which they have permissions.

A SystemAdmin creates the Directory and assigns ContentDeveloper permissions to specific teams who can now edit that Directory.

!!! important

    Only users who have the SystemAdmin permission can view the Administration screen and the Administration nav bar (Users, Modules, Workspaces).

### Modules

![Caster modules](../assets/img/caster-modules.PNG)

[Modules](https://www.terraform.io/docs/glossary.html#module) are a Terraform construct:

!!! info

    A module is a container for multiple resources used together. Modules create lightweight abstractions, so you can describe your infrastructure in terms of its architecture, rather than directly in terms of physical objects.

Modules are very powerful and make complex configurations simpler and more easily shared and used. A module takes any Terraform configuration consisting of any number of resources and turns it into a single block, exposing required and/or optional variables. Some examples include:

1. A generic virtual machine module that abstracts away commonly used parameters into variables such as:

    - **TeamId:** sets `guestinfo.teamId` in `extra_config`.
    - **Networks:** creates a NIC for each specified network and assigns it to the specified network VLAN.
    - **ExerciseId:** appends the `exerciseId` to the name of the VM for use with ODXs requiring unique naming.
    - Other simplified variable names based on the target audience.

2. A module to create a very specific type of virtual machine resource, such as a domain controller, that points to a known good VMware template/base disk and an Ansible playbook that requires variables such as:

    - Domain Name
    - IP Address
    - DomainAdminUser
    - DomainAdminPass

3. A module to define an entire Cyber Flag enclave.
4. A module to define a generic GreySpace that accepts variables to configure GreyBox, TopGen, etc.

Modules allow for endless flexibility for developers to wrap whatever configuration they can create into a small package and describe to consumers of the module exactly what it does and what values it requires to function.

Caster makes it easier to search for and use modules when building a Terraform configuration.

Caster supports modules created as GitLab projects that are visible to the GitLabToken defined in the API settings with at least one version defined. When adding/refreshing the project to Caster, Caster will show all versions.

!!! note

    Caster requires that the inputs file and the outputs file are written in JSON (that is, `variables.tf.json` and `ouptuts.tf.json`).

There are three ways to add/refresh a module in Caster:

- For every modules list request, Caster API checks for updated modules in the **Terraform-Modules** group (the group ID is a Caster API setting) or any of its subgroups. If you add a module or version, you may have to refresh your Caster UI browser to see the change.

- Because the Caster UI uses its internal modified date to determine if there are any new changes, the Caster dates may get out of sync with the GitLab dates. In this case, an administrator can force an update of all of the modules.

- An administrator can also individually add/refresh a module using its GitLab Project ID, whether or not it is underneath the Terraform-Modules group.

When editing a file in the Caster UI, users can open a **Modules** sidebar to search through available modules.

Upon selecting a Module, a form opens that allows the user to select the Version of the Module, and then complete the version-specific variables that the Module expects.

Upon **Submit**, Caster generates the Terraform code to use the selected module with the selected variable values. The user can copy this code into a configuration file.

### VLANs

![Caster VLANs](../assets/img/caster-VLANs.PNG)

Adds the ability to manage VLAN ids. Creates pools of 4096 VLANs and subdivides them into Partitions. A user can request a VLAN from a Partition and will receive an unused VLAN id, now marked as used until they release it. A Partition is either assigned to a Project, or is a system-wide default. Users request VLAN ids either from their Project's Partition or from the default.

- VLANs can have tags for organizational purposes; users can request a VLAN by tag
- Uses can request a VLAN by specific VLAN id within a Partition
- VLANs marked as reserved (including 0, 1, and 4095, reserved by default) are never used
- fixed modified properties in entity updated events to restore signalR functionality

## User Guide

### Project

The top-level construct in Caster is a *project*. The *project* is a way to organize and categorize similar environments for multiple files and directories within Caster. The main screen of Caster displays a list of the projects available and allows a user to create a new one.

A project can:

- Categorize large events
- House directories, workspaces, and subdirectories

Users can add new projects, name projects, save projects, and also export projects. A project's landing page in Caster has a navigation panel for easy movement within the project's files, workspaces, and directories.

#### Export Project

Export Projects allows you to export the project as a zip file.

#### Add Directory

Add Directory lets you create a new directory at the same level as the above projects.

### Files

*Files* represent text files to eventually put onto a file system and use with the Terraform command line tool. You can name and edit files through Caster, but file extensions are important and have specific meaning to Terraform.

- `.tf` A configuration file that defines resources, variables, etc., in a Terraform configuration.
- `.auto.tfvars` Contains the values used for variables defined in `.tf` files.

!!! note

    When working with files in Caster, **CTRL+L** locks/unlocks a file to prevent others from editing that file simultaneously. When locked, the file icon appears as a dashed line. When unlocked, the file icon appears solid. Administrators can also lock files. A file is *administratively locked* to prevent anyone from changing that file. A lock icon in the top right corner of the file edit screen denotes that the file is administratively locked. **CTRL+S** saves a file.

See the official [Terraform Documentation](https://www.terraform.io/docs/index.html) for more details on supported file types and extensions. In the future, Caster may provide more guidance on allowed types and content of files.

### Workspaces

![Caster workspaces](../assets/img/caster-workspaces.PNG)

A *workspace* represents a specific instance of a deployed Terraform configuration. Use the same configuration to deploy virtual machines to multiple workspaces that differ only by the values set to certain variables. For example, define a configuration once for an enclave in a Cyber Flag exercise, and then deploy to `flag00` through `flag30` workspaces - each creating a copy of the enclave.

Workspaces can contain files, which extend the configuration of the directory for that specific workspace. This might include files specifying values for variables defined in the directory, or additional resources deployed only for that workspace.

A workspace is where users:

- Create an instance of a Terraform configuration
- Run their plans (*Runs* are a specific instance of a Terraform plan; explained [here](#run-plan-and-apply))
- Manage the differences and the variables in their environments

Users can access workspaces from a project's navigation pane in Caster. Users can add additional files, but *not* additional directories, to a workspace. The workspace view allows users to see all the runs that have been planned and applied. Runs shaded in red are destroyed operations, while runs in white signify various other status classifications.

Users can `Plan`, `Destroy`, `Apply`, `Taint`, and `Reject` operations in real time in the workspace view.

`Caster.Api` utilizes the Terraform binary in order to execute workspace operations. This binary is running inside of the `Caster.Api` service. *Restarting or stopping the `Caster.Api` Docker container while a Terraform operation is in progress can lead to a corrupted state.*

In order to avoid this, a System Administrator should follow these steps in the Caster UI before stopping the `Caster.Api` container:

- Navigate to Administration, Workspaces
- Disable Workspace Operations by clicking the toggle button
- Wait until all Active Runs are completed

### Directories

The top-level construct within a project is a *directory*. A project can contain many directories. Directories contain files that make up a particular Terraform configuration, workspaces that represent a specific instance of that configuration, and subdirectories. Directories are primarily for organization and shared resources.

#### Directory Hierarchies

Directories can contain subdirectories to create a *hierarchy* of directories and the configuration files contained therein. When creating a run, the files in the workspace, the workspace's directory, ***and all parent directories*** merged and pass to Terraform as a single configuration. This eliminates redundancy when developing many environments that are similar or share a set of common variables or data across many configurations. For example, a large deployment might have a top-level directory defining global variables `vlan ids` and `team ids`, and subdirectories defining resources using those variables.

Users can add, rename, delete, or export a directory from the navigation panel on a project's main Caster page.

Peer directories (directories that fall outside a parent directory) are not included in a run.

### Designs

Designer provides a graphical user interface for creating and editing terraform deployments through the use of modules.

When you open a project, you can create a design and add modules backed by Git to that design. You can also create variables for use in the modules settings.

## Caster Tips

### Crafting Terraform Code

This topic is for anyone who manages a Crucible instance who wants to configure their Terraform provider installation for Caster. You can configure Terraform to only download certain providers from the Internet and use them from a local File store.

Refer to **HashiCorp's Terraform** documentation: **CLI Configuration File** [Provider Installation](https://www.terraform.io/docs/cli/config/config-file.html#provider-installation).

For your reference, below is the `.terraformrc` file currently implemented in the SEI's CyberForce instance of Caster.

In the SEI's instance, we want to use any locally downloaded plugins in the `sei` or `mastercard` namespace. In addition, we can download any of the `hashicorp` namespace providers in the `direct` section directly from the Internet without any operator intervention.

These plugins are then cached in the `plugin_cache_dir` section, to save from downloading the providers during every Terraform `plan` and `apply`.

!!! example

    ```text
    plugin_cache_dir = "/terraform/plugin-cache"
    provider_installation {
	    filesystem_mirror {
		    path = "/terraform/plugins/linux_amd64"
            include = [
        	    "registry.terraform.local/sei/*",
        	    "registry.terraform.local/mastercard/*"
            ]
        }
         direct {
     	    include = [
            "hashicorp/vsphere",
            "hashicorp/aws",
            "hashicorp/azurerm",
            "hashicorp/azuread",
            "hashicorp/null",
            "hashicorp/random",
            "hashicorp/template",
            "hashicorp/cloudinit"
            ]
        }
        }
    ```

### Hosts

A *host* consists of a name, datastore, and maximum number of virtual machines that it can support. The API creates and manages hosts, then assigns them to exercises. An exercise can have many hosts.

Workspaces have an additional property, `DynamicHost`, which is usually set to `false`. When Alloy creates a workspace, this is set to `true`, and changes the behavior of a run. When `DynamicHost` is `true`, Caster examines all of the hosts assigned to the current exercise and chooses the one with the least usage (the number of machines to deploy/maximum machines) to assign to the workspace.

In addition to the normal run files, Caster creates a `generated_host_values.auto.tfvars` containing two variable values: `vsphere_host_name` and `vsphere_datastore`, set to the name and datastore of the selected host. Upon applying the run, Caster tracks how many VMs deployed to the host, and uses this for future calculations.

After an on-demand exercise (ODX) finishes, Caster deletes the workspace and releases the host's resources. If a run attempts to deploy more virtual machines than there is capacity for in the available hosts, the run will fail.

#### On-Demand Exercise Functionality

Alloy calls Caster in order to deploy resources for lab or ODX-style functionality. Caster itself does not differ in its main functionality of deploying workspaces and lets Alloy handle most of the ODX functionality.

However, in order to support this functionality Caster dynamically selects a host to deploy to.

Normally, the cluster or host to deploy to is embedded in the configuration - either directly or as a variable - and Caster doesn't concern itself with this. For ODX's, Caster *does* need to concern itself with:

- ensuring that resources are deployed evenly to the available hosts, and
- more ODXs than the hardware can deploy are not deployed.

To address these concerns the concept of a *host* was added to Caster.

### Run, Plan, and Apply

A *run* is a specific instance of the Terraform *plan* and *apply* process for a workspace. The run is how the configuration in a directory is instantiated into deployed resources within a workspace. Upon opening a workspace, a list of runs is displayed. This is where plan or destroy operations are started.

*Plan* and *apply* are specific Terraform terminologies. Every run is made up of a plan and an apply step.

#### Plan

Clicking *plan* creates a new Run and executes the `terraform plan` command on the given configuration. This raw Terraform output, visible to the user, describes:

- What actions Terraform will take
- What resources to create
- What resources to change
- What resources to destroy

A plan shows the user what will deploy.

This output always ends with a summary of the form `Plan: x to add, y to change, z to destroy`. The user reviews this and chooses to apply or reject the plan.

- Choosing **apply** creates an apply for the run and executes `terraform apply` on the previously generated plan. This causes Terraform to make the changes described.
- Choosing **reject** invalidates the plan. No changes are made to the infrastructure.

#### Apply

*Apply* takes a run, executes `terraform apply` on the previously generated plan and deploys the resources for a workspace. The `Apply` command:

- Deploys a workspace run
- Releases plan tools such as network resources and virtual machines into vCenter

Within the workspace view users can see all the runs that have been planned and applied.

#### Destroy

Selecting destroy instead of plan is similar, but the generated plan will destroy all previously deployed resources in the workspace, rather than matching the infrastructure to the current configuration. That is, *Destroy* creates a plan that will destroy all of the previously deployed resources in a workspace.

If a resource is defined in the configuration and created in a run and then deleted from the configuration, it is destroyed upon the next plan or destroy run. This is because a Terraform run always tries to match the infrastructure to the current configuration.

There is only one run in progress at a time per workspace. Terraform locks the state of the workspace and only performs a single operation at a time. Developers may wish to break up large deployments into multiple directories and workspaces to operate on different parts of the deployments simultaneously. For example, break out user enclaves so developers can perform actions on other parts of a network without (potentially) waiting a long time to redeploy user machines.

The workspace view allows users to see a table with all the runs that have been planned and applied within that directory. Runs highlighted in red are destroyed operations.

Within the workspace view users can click `Destroy` to destroy live Terraform applications.

This Infrastructure-as-Code approach is unfamiliar to some developers. It defines a configuration and applies it in its entirety, rather than selecting individual pieces for deployment. There are some ways to target individual pieces of a configuration, but Terraform recommends this as the exception rather than the rule, and Caster does not fully implement this.

#### Taint

*Taint* is a flag on a resource that tells Terraform to destroy and recreate a new instance on the next plan-and-apply cycle.

Taint allows users to redeploy resources. For example, if a user needs to redeploy a series of virtual machines, the user can:

1. Taint these resources
2. Run another plan-and-apply cycle that will redeploy the instance as if new from a template

Some resources are not in scope for taint, however.

Users can taint resources within the workspace view. A tainted resource will display in red shading. Users can easily access the `Untaint` command while in workspace view before running another plan-and-apply cycle if they decide to keep the resource.

## Glossary

This glossary defines key terms and concepts used in the Caster application.

**Designer:** a graphical user interface for creating and editing Terraform deployments through the use of modules.

**Directory:** the outline of a project and the modules it contains.

**File:** text files to eventually put onto a file system and use with the Terraform command line tool.

**Host:** consists of a name, datastore, and maximum number of virtual machines that it can support.

**Module:** a container for multiple resources used together. Modules create lightweight abstractions, so you can describe your infrastructure in terms of its architecture, rather than directly in terms of physical objects.

**Project:** a way to organize and categorize similar environments for multiple workspaces and directories within Caster.

**Terraform:** an open-source Infrastructure-as-Code tool.

**Topology:** the physical and logical arrangement of nodes and connections in a network.

**Workspace:** a specific instance of a deployed Terraform configuration.
