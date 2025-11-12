# Tutorial: Creating a Challenge in TopoMojo

This tutorial walks through creating a cybersecurity challenge in [TopoMojo](../../topomojo/index.md) - a virtual lab builder and player.


## Prerequisites

- Access to a TopoMojo instance
- Required role permissions for creating workspaces
- Basic understanding of virtualization and networking concepts
- Familiarity with supported hypervisors (VMware or Proxmox)


## Overview

TopoMojo is a lab builder and player application for developing cybersecurity challenges. Each challenge lives in its own **workspace** that contains all virtual machines, artifacts, and configuration. When a user starts a challenge, the system deploys a read-only copy called a **gamespace** for participants to interact with.

For more detailed documentation on TopoMojo, visit [TopoMojo's documentation page](../../topomojo/index.md).

## Table of Contents

- [Tutorial: Creating a Challenge in TopoMojo](#tutorial-creating-a-challenge-in-topomojo)
  - [Prerequisites](#prerequisites)
  - [Overview](#overview)
  - [Table of Contents](#table-of-contents)
  - [Create and Configure a Workspace](#create-and-configure-a-workspace)
  - [Add Virtual Machine Templates](#add-virtual-machine-templates)
    - [Saving Template Changes](#saving-template-changes)
    - [Guest Settings](#guest-settings)
    - [Template Replicas](#template-replicas)
    - [Template Best Practices](#template-best-practices)
  - [Configure Transforms](#configure-transforms)
  - [Create Challenge Questions](#create-challenge-questions)
  - [Common Issues and Troubleshooting](#common-issues-and-troubleshooting)
    - [Issue: Changes to VM Are Not Saving](#issue-changes-to-vm-are-not-saving)
    - [Issue: Transform Values Not Appearing in VMs](#issue-transform-values-not-appearing-in-vms)
  - [Additional Resources](#additional-resources)


## Create and Configure a Workspace

1. Log into TopoMojo
2. Click the **Create New Workspace** button to create an empty workspace for building your challenge
3. Configure Workspace Metadata. Descriptions of a few key settings are below; see the [TopoMojo documentation on building a workspace](../../topomojo/index.md#building-a-new-workspace) for full details and additional settings.
   1. **Workspace Title**: Set a title that identifies your challenge (e.g., "Network Traffic Analysis")
   2. **Description**: Provide a short (1-3 sentence) description of the challenge.
   3. **Tags**: Add a few tags that make this challenge easily searchable. This field is a space-delimited list (e.g., "cyber-defense-analysis incident-response")
   4. **Audience**: Set the audience to control which users can view/play this challenge. See the [TopoMojo documentation](../../topomojo/index.md#template-field-definitions) for additional details.


## Add Virtual Machine Templates

TopoMojo Templates are starting point virtual machines that you can customize. When a user deploys a Gamespace, they will receive read-only copies of all templates in the workspace from their last saved state. See the [TopoMojo documentation on configuring templates](../../topomojo/index.md#templates-tab) for full details and settings.

Navigate to the **Templates** section of the workspace to add VMs to your challenge. Deploy VMs from templates and access the console to make changes to workspace VMs.

### Saving Template Changes

A template must be "unlinked" to allow saving changes. **You must unlink before making changes to a VM - you will lose any changes made to a deployed VM from a linked template.**

Unlinking a template creates a new clone of the parent VMDK when using VMware and makes a clone of the parent Proxmox VM Template when using Proxmox.

After unlinking the UI will show a **Save** button for the template. Clicking **Save** will create a VM Snapshot on the hypervisor. TopoMojo only supports one snapshot; saving will overwrite the previous saved state.

### Guest Settings

**Guest Settings** use VMware guestinfo variables or the QEMU Firmware Configuration Device for Proxmox to pass information/variables to deployed VMs. Expand a template's options to configure guest settings using a `key=value` format. You can use [transforms](#configure-transforms) to insert randomized values.

Example:

```text
token1=##token1##
ipaddr=1.2.3.4
```

VMs can use scripts to access guest settings using hypervisor-specific commands:

Querying VMware guestinfo variables

```bash
vmware-toolsd --cmd "info-get guestinfo.<variable-name>"
```

Querying the QEMU Firmware Configuration Device for Proxmox

```bash
sudo cat /sys/firmware/qemu_fw_cfg/by_name/opt/guestinfo.<variable-name>/raw
```

### Template Replicas

You can configure **Replicas** for team-based challenges where multiple team members need to work simultaneously. All replicas will have the same starting state, configurations, and guest settings. TopoMojo will deploy the specified number of replicas of the VM template in gamespaces. Running VMs using replicas have a `_#` suffix applied to their names (e.g., `kali_1`, `kali_2`). It is a best practice to set the replica count to `-1` to deploy a replica of the VM for each member of the team that owns the gamespace. This avoids deploying more replicas that there are team members to use the VMs.

### Template Best Practices

1. **Unlinking Templates**: Use linked templates as much as possible. Using linked templates reduces storage on the backend and facilitates a shared/common VM/environment that the user can become comfortable with across challenges.
2. **Use a "Challenge Server"**: A "Challenge Server" is a VM that serves as the brains of a challenge. It can facilitate startup scripts that reconfigure VMs at boot, host a grading script/website, collect logs, and more. This allows developers to maximize linked VMs by placing all configuration/automation/grading on a single unlinked VM. CMU SEI publishes our [Challenge Server on GitHub](https://github.com/cmu-sei/Challenge-Server).
3. **Template Visibility**: Templates can be "visible" or "hidden". Visible templates will have a UI console accessible to the user. Hidden VMs will not show a UI console to the user, but may still be accessible to the user over the network. Hide VMs that you do not want users to have direct access to. For example, in a challenge where the user must identify the source of a live cyber attack, you should hide any VMs related to the attack/attacker.
4. **Template Naming**: Templates should have clean, descriptive names. Visible templates should not have names with tags, IDs, or other details that are not meaningful to the end user. It can be helpful to name hidden templates with unique identifiers that allow administrators to quickly isolate/identify resources in the backend while troubleshooting. It is best when templates for a challenge have unique names to help with each of search, but uniqueness across workspaces is not enforced.
5. **Template Count**: Add VM templates only when required. Keeping the VM template count low reduces infrastructure resource requirements and minimizes challenge complexity.
6. **Clean Before Saving**: Developers should clean up all development artifacts before saving templates. This includes: clearing command/browser history, deleting logs, and removing development artifacts/files. If left behind, users can find undesired hints and shortcuts to solving the challenge.


## Configure Transforms

The **Challenge** tab of a workspace allows configuration of transforms and challenge questions.

Transforms are dynamically generated values (like tokens) that make each deployed challenge unique. TopoMojo creates randomized values for configured transforms at gamespace deploy time. Creating unique challenge deployments using transforms increases reusable value of challenge by avoiding cases where users already know the answers. Challenges with unique/dynamic answers prevents answer sharing between users.

Most text fields on the **Challenge** tab support transform substitution via a "double-pound" variable notation (e.g., `##transform-name##`). The **Markdown** section and **Variant Markdown** sections add additional markdown to the challenge document that can contain transforms for dynamic challenge documents. Challenge questions and answers can contain dynamic text/answers using transforms. See the [Guest Settings section](#guest-settings) for applying transforms in template guest settings.

See the [TopoMojo documentation on transforms](../../topomojo/index.md#transforms) for additional details.


## Create Challenge Questions

In the Challenge section, add questions that users will answer. Challenge questions should always have an example answer configured to help users understand the answer's expected format.

Challenge questions can optionally specify a weight to change the percentage of points allocated to the question. By default, all questions will have a weight of `0`, meaning all questions have an equal share of the challenge's total points. Use values `0-100` to define the percentage of points to allocate (e.g., a question with a weight of `60` will hold 60% of the points for the challenge). **Challenges with question weights that do not add up to 100 may experience undesired behavior.**

All questions one of the "Grader Types" which defines to how determine correct answers. See the [TopoMojo Documentation](../../topomojo/index.md#question-set) for more details on the supported grader types.

You can reference transform variables in question text and answers as described in the [transforms section](#configure-transforms). Using transforms allows for unique questions/answers per challenge deployments.

You can optionally use **Variants** to randomize challenges in a more prescribed way than using transforms. Variants can have different documentation, ISOs attached to VMs, and questions/answers. TopoMojo will randomly choose a variant when deploying a gamespace. See the [TopoMojo documentation on variants](../../topomojo/index.md#variants) for more details.

Developers should prefer using transforms instead of variants. Using variants can increase challenge development and testing because of the need to develop an test all variants. Typical use cases for variants include:

1. Challenge artifacts that cannot be dynamically modified in a predictable way by scripting/transforms
2. VMs must have different configurations where scripting the changes at boot is not feasible


## Common Issues and Troubleshooting

### Issue: Changes to VM Are Not Saving

1. Confirm you have unlinked the template.
2. Confirm clicking the "Save" button does not show an error in the TopoMojo UI.
3. Check TopoMojo API and hypervisor (VMware or Proxmox) logs to find errors saving VMs.

### Issue: Transform Values Not Appearing in VMs

1. Because TopoMojo populates transform values at gamespace deploy time, transform values will not appear in workspaces.
2. Confirm you have configured guest settings as desired. Variable names are case sensitive.
3. Confirm you are using the [correct command for your hypervisor](#guest-settings).


## Additional Resources

- [TopoMojo GitHub Repository](https://github.com/cmu-sei/TopoMojo)
- [TopoMojo Documentation](../../topomojo/index.md)
- [Crucible Documentation](/)
- [CMU SEI Challenge Development Guidelines](https://resources.sei.cmu.edu/library/asset-view.cfm?assetID=889267) - Technical report with best practices
