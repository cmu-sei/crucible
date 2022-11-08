# Caster Overview

Caster is the primary deployment component of the Crucible simulation framework. Caster is built around [Terraform](https://www.terraform.io/), an open source "Infrastructure as Code" tool. 

Caster is meant to provide a web interface that gives exercise developers a powerful and flexible way to create, share, and manage topology configurations. 

Initial versions of Caster focus on a web front-end for raw Terraform configurations and outputs. This gives advanced developers easier access to a powerful deployment tool. Targeted improvements to the experience for these users will be made in the future. Eventually, this system will be used to underpin a more user-friendly interface that will allow configurations to be pieced together with less or no writing of Terraform code directly. 

The goal is to create a tool that gives advanced users the power and flexibility that they desire, while also allowing novice users to take advantage of complex topologies created by advanced users or create their own simple ones easily.

### Terraform Documentation

For more information on native Terraform constructs used in Caster, please refer to the [Terraform documentation](https://www.terraform.io/docs/index.html).

## Caster and Player Integration

Caster is a standalone application and is not dependent upon Player or the other Crucible applications in the framework. However, there are some integrations that are desirable when using the components together. One natural integration is assigning Caster deployed virtual machines to teams within Player. This is done  by setting the `guestinfo.teamId` property of a `vsphere_virtual_machine` resource's `extra_config` setting to the `Id` of the Player Team. After each run, Caster will look for this property and register the virtual machine with the specified Team.
>**Note:** Terraform reboots a virtual machine if any of its `extra_config` properties are changed, including `guestinfo` variables.
