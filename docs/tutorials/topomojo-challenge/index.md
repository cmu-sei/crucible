# Creating a TopoMojo Challenge

This tutorial shows you how to create and configure a cybersecurity challenge in [TopoMojo](../../topomojo/index.md), from defining the workspace and resources to preparing the challenge for participant use.

## Assumptions

This tutorial assumes the following:

- You have access to a TopoMojo instance
- You have the `Creator` role or greater in TopoMojo
- You have a basic understanding of virtualization and networking concepts
- You are familiar with supported hypervisors like VMware and Proxmox

:blue_book: As you work through this tutorial, you may want to learn more about the Crucible applications or features. If so, refer to the [Related Resources](#related-resources) section below for additional detail and reference information.

## Step 1: Creating and Configuring Your Workspace

1. Log in to your TopoMojo instance.
2. Click **+New Workspace** to create an empty workspace for the challenge.
3. Enter the workspace metadata. The list below describes some key settings. For additional detail, see the TopoMojo documentation, [Building a New Workspace](../../topomojo/index.md#building-a-new-workspace).

   - **Title**: Enter a title that identifies the challenge (for example, "Network Traffic Analysis").
   - **Description**: Enter a short description of the challenge (1–3 sentences).
   - **Tags**: Add tags to make the challenge easier to find. Use a space-delimited list (for example, `cyber-defense-analysis incident-response`).
   - **Audience**: Select the audience to control which users can view or play the challenge. For a complete list of the settings, see the TopoMojo documentation, [Settings](../../topomojo/index.md#settings).

## Step 2: Adding Virtual Machine Templates

TopoMojo templates are starting-point virtual machines that you can customize. When a user deploys a gamespace, they receive read-only copies of all templates in the workspace from the template's last saved state. For full field descriptions and functions, see [Templates](../../topomojo/index.md#templates) in the TopoMojo Guide.

1. In your TopoMojo workspace, click the **Templates** tab.
2. Click **+ Add Templates** to add VMs to your challenge.
3. Deploy VMs from templates and access the console to make changes to workspace VMs.

### Saving Template Changes

*Unlink* the template to save changes. Unlinking a template creates a new clone of the parent VMDK on VMware and a new clone of the parent VM template on Proxmox.

After unlinking, initializing, and deploying, the TopoMojo UI shows a **Save** icon for the template. Clicking **Save** creates a VM snapshot on the hypervisor. TopoMojo supports only one snapshot, and saving overwrites the previously saved state.

### Guest Settings

Guest Settings pass information to deployed VMs using VMware `guestinfo` variables or the QEMU firmware configuration device on Proxmox. Expand a template's options to define guest settings using a `key=value` format. You can use transforms to insert randomized values.

!!! example

    ```text
    token1=##token1##
    ipaddr=1.2.3.4
    ```

VMs can use scripts to access guest settings using hypervisor-specific commands.

!!! example "Querying VMware guestinfo variables"

    ```bash
    vmware-toolsd --cmd "info-get guestinfo.<variable-name>"
    ```

!!! example "Querying the QEMU firmware configuration device on Proxmox"

    ```bash
    sudo cat /sys/firmware/qemu_fw_cfg/by_name/opt/guestinfo.<variable-name>/raw
    ```

### Template Replicas

You can configure **Replicas** for team-based challenges where multiple team members work simultaneously. Each replica starts with the same state, configuration, and guest settings. TopoMojo deploys the specified number of VM replicas in each gamespace and appends a `_#` suffix to running VM names (for example, `kali_1`, `kali_2`). As a best practice, set the replica count to `-1` so TopoMojo deploys one replica per team member. This prevents deploying more replicas than the team can use.

??? tip "Template Best Practices"

    - **Unlinking Templates:** Use linked templates whenever possible. Linked templates reduce backend storage use and provide a shared, consistent VM environment that users can become familiar with across challenges.
    - **Use a Challenge Server:** Use a dedicated *challenge server* VM as the control point for the challenge. A challenge server can run startup scripts, host grading logic or websites, collect logs, and perform automation. This approach lets you maximize the use of linked templates by placing configuration, automation, and grading on a single unlinked VM. Carnegie Mellon University's Software Engineering Institute provides a reference challenge server on GitHub: [github.com/cmu-sei/Challenge-Server](https://github.com/cmu-sei/Challenge-Server).
    - **Template Visibility:** Set templates as *visible* or *hidden*. Visible templates expose a UI console to users. Hidden templates do not expose a UI console but may remain accessible over the network. Hide templates that users should not access directly, such as attacker or backend systems.
    - **Template Naming:** Use clean, descriptive names for templates. Avoid tags, IDs, or internal details in visible template names. Use unique or structured names for hidden templates to help administrators identify resources during troubleshooting.
    - **Template Count:** Add VM templates only when required. Fewer templates reduce infrastructure usage and keep challenges easier to understand and maintain.
    - **Clean Before Saving:** Clean templates before saving them. Remove command and browser history, delete logs, and remove development artifacts or files to prevent unintended hints or shortcuts.

## Step 3: Configuring Transforms

The **Challenge** tab lets you configure transforms and challenge questions.

Transforms generate dynamic values, like tokens, that make each deployed challenge unique. TopoMojo creates randomized values for configured transforms when it deploys a gamespace. Using transforms increases challenge reusability by preventing users from relying on known answers and sharing solutions.

Most text fields on the Challenge tab support transform substitution using a double-pound notation (for example, `##transform-name##`). The **Markdown** and **Variant** sections let you add challenge content that also supports transforms, enabling dynamic challenge documents. Challenge questions and answers can include transforms to generate dynamic text and answers. For applying transforms in VM configuration, see the [Guest Settings](#guest-settings) section.

## Step 4: Adding Challenge Questions

Also on the Challenge tab, add the questions that users must answer. Always configure an example answer for each question to show the expected answer format.

You can assign an optional **Weight** to each question to control how TopoMojo distributes points. By default, questions have a weight of `0`, which gives all questions an equal share of the total points. Use values from `0–100` to allocate a percentage of the total score (for example, a weight of `60` assigns 60% of the challenge points to that question).

!!! warning "Question Weights"

    If question weights do not add up to 100, the challenge may behave unexpectedly.

Each question uses a **Grader** type, which defines how the system evaluates answers. For additional details on supported grader types, see [Question Set](../../topomojo/index.md#question-set) in the *TopoMojo Guide*.

You can reference transform variables in question text and answers, as described in the [transforms section](#step-3-configuring-transforms). Transforms allow each challenge deployment to generate unique questions and answers.

You can also use variants to randomize challenges in a more controlled way than transforms. Variants can define different documentation, VM attachments (such as ISOs), and question sets. When deploying a gamespace, TopoMojo randomly selects a variant. For additional details, see [Variants](../../topomojo/index.md#variants) in the *TopoMojo Guide*.

Prefer transforms over variants. Variants increase development and testing effort because you must build and validate each variant. Use variants when:

1. Challenge artifacts can't change dynamically through scripting or transforms.
2. VM configurations must differ in ways that scripting at boot can't support.

## Troubleshooting Common Issues

### VM Changes Don't Persist

1. Confirm that you unlinked the template.
2. Confirm that clicking **Save** does not produce an error in the TopoMojo UI.
3. Review TopoMojo API logs and hypervisor logs (VMware or Proxmox) for errors related to saving the VM.

### Transform Values Don't Appear in VMs

1. Remember that TopoMojo applies transform values at gamespace deploy time; they do not appear in workspaces.
2. Confirm that you configured guest settings correctly and that variable names match exactly (case sensitive).
3. Confirm that you use the [correct command for your hypervisor](#guest-settings).

## Related Resources

- [Challenge Development Guidelines for Cybersecurity Competitions](https://www.sei.cmu.edu/library/challenge-development-guidelines-for-cybersecurity-competitions): This paper draws on the SEI's experience to provide general-purpose guidelines and best practices for developing effective cybersecurity challenges.
- [Crucible Documentation](../../index.md)
- [TopoMojo Guide](../../topomojo/index.md)
- [TopoMojo GitHub Repository](https://github.com/cmu-sei/TopoMojo)
