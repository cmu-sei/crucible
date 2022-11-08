# Caster Modules

[Modules](https://www.terraform.io/docs/glossary.html#module) are a Terraform construct:

> A module is a container for multiple resources that are used together. Modules can be used to create lightweight abstractions, so that you can describe your infrastructure in terms of its architecture, rather than directly in terms of physical objects.

Modules are very powerful and allow for complex configurations to be made simpler and more easily shared and used. A module takes any Terraform configuration consisting of any number of resources and turns it into a single block, exposing required and/or optional variables. Some examples include:

- A generic virtual machine module that abstracts away commonly used parameters into variables such as: 
  - **TeamId:** sets `guestinfo.teamId` in `extra_config`.
  - **Networks:** creates a NIC for each specified network and assigns it to the specified network vlan.
  - **ExerciseId:** appends the `exerciseId` to the name of the vm for use with ODX's where unique naming is required.
  - Other simplified variable names based on the target audience.
- A module to create a very specific type of virtual machine resource, such as a domain controller, that points to a known good VMware template/base disk and an Ansible playbook that requires variables such as:
  - Domain Name
  - IP Address
  - DomainAdminUser
  - DomainAdminPass
- A module to define an entire Cyber Flag enclave.
- A module to define a generic GreySpace that accepts variables to configure GreyBox, TopGen, etc.

Modules allow for endless flexibility for developers to wrap whatever configuration they can create into a small package and describe to consumers of the module exactly what it does and what values it requires to function. 

Caster makes it easier to search for and use modules when building a Terraform configuration. 

Caster supports modules created as GitLab projects that are visible to the GitLabToken defined in the API settings with at least one version defined.  All versions will be shown in Caster when the project is added/refreshed to Caster.  

>**Note:**  Caster requires that the inputs file and the outputs file be written in JSON (that is, `variables.tf.json` and `ouptuts.tf.json`). 

There are three ways that the a module can be added/refreshed to Caster:

- Every time that the modules list is requested, Caster API checks for updated modules in the **Terraform-Modules** group (the group ID is a Caster API setting) or any of its sub-groups.  If you add a module or version, you may have to refresh your Caster UI browser to see the change.

- Because the Caster UI uses its internal modified date to determine if there are any "new" changes, the Caster dates could get out of sync with the GitLab dates.  In this case, an administrator can force an update of all of the modules.

- An administrator can also individually add/refresh a module using its GitLab Project ID, whether or not it is underneath the Terraform-Modules group.

When editing a file in the Caster UI, a **Modules** sidebar can be opened to search through available modules.

Upon selecting a Module, a form opens that allows the user to select the Version of the Module, and then complete the version-specific variables that the Module expects.

Upon **Submit**, Caster generates the Terraform code that can be copied into a configuration file to use the selected module with the selected variable values.