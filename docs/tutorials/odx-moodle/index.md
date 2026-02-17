# Building an On-Demand Exercise (ODX)

This tutorial shows you how to create an on-demand exercise using a group of Crucible applications. You'll start from a clean, default Crucible stack and build a working on-demand exercise delivered through Moodle. You'll use Caster, Player, Steamfitter, and Alloy to define infrastructure, scenarios, and participant interaction, and then connect the exercise to Moodle for delivery using the Crucible Plugin for Moodle.

## Assumptions

This tutorial assumes the following:

- You have a deployed Crucible platform
- The Crucible platform contains Caster, Player, and Alloy
- You have installed the Crucible plugin in Moodle
- vCenter or Proxmox capacity sized for ODX behavior
- You have the `Administrator` or `Content Developer` role in the Crucible apps

:blue_book: As you work through this tutorial, you may want to learn more about the Crucible applications or features. If so, refer to the [Related Resources](#related-resources) section below for additional detail and reference information.

## Step 1: Creating the Player View Template

Create the Player View that participants will use during the exercise. Alloy clones this view for each event. For additional detail, see the Player documentation, [Manage Views](../../player/index.md#manage-views).

1. Log into Player.
2. In the upper-right corner, select your username, then **Administration**.
3. From the left navigation, select **Views**.
4. Click the **+** icon to add a new view.
5. Enter a **Name** and **Description**, set the **Status** to **Active** or **Inactive**, and check **Template**.
6. Select **Applications** to add applications to the view, using either existing templates or a blank starting point.
7. Select **Teams** to add teams and assign roles and permissions.
8. (Optional) Select **Files** to upload files to the view.

## Step 2: Creating the Caster Project and Directory

Create a Caster project and directory to store the Terraform infrastructure-as-code for the exercise topology. For additional detail, see the [Caster User Guide](../../caster/index.md#user-guide).

1. Log into Caster.
2. Click the **+** icon to create a new **Project**, then **Save**.
3. From the left navigation, select **Add Directory** to add a directory containing your Terraform configuration (modules and variables).
4. Make sure that the Terraform configuration is ready to run `plan` and `apply`.

!!! note "vCenter with cluster licensing"

    Do *not* enable Dynamic Host. Point Terraform at the vCenter cluster and allow vCenter to manage host placement.

!!! note "vCenter without cluster licensing"

    Enable Dynamic Host in Alloy so Caster selects a host at launch. When Alloy creates a workspace, Caster sets `DynamicHost=true`, chooses the least-loaded host among those assigned to the exercise, and writes a `generated_host_values.auto.tfvars` with `vsphere_host_name` and `vsphere_datastore`. Regardless of licensing model, when the event ends, the system deletes the workspace and releases the host usage.

## Step 3: Creating a Steamfitter Scenario Template

(Optional) Create a **Scenario Template** to run tasks and injects during the event. For additional detail, see the Steamfitter documentation, [Scenario Templates](../../steamfitter/index.md#scenario-templates).

1. Log into Steamfitter.
2. Select **Scenario Templates**, then click **Add Scenario Template**.
3. Enter a **Name**, **Description**, and set the **Duration Hours**.
4. Click **Save**.
5. Select the new **Scenario Template** and, next to **Tasks:**, click **+** to add a task to help define your scenario. Complete the same task information as you would if you were creating a new task.

!!! info

    For additional details on tasks, see [Tasks](../../steamfitter/index.md#tasks) in the *Steamfitter Guide*.

## Step 4: Creating the Alloy Event Template

Alloy ties the exercise components together and provides participants with a centralized location to engage in the exercise. For additional detail, see the Alloy documentation, [Deploy an Event/Exercise](../../alloy/index.md#deploy-an-eventexercise).

1. Log into Alloy.
2. In the upper-right corner, select your username, then **Administration**.
3. In the left navigation, select **Event Templates**, then click **Add New Event Template**.
4. Next to the New Event Template, click the **Edit** icon to set a **Name**, **Event Template Description**, and **Duration Hours**.
5. From the dropdown menus (or by pasting IDs), select the following:
    - Player View Template
    - Caster Directory
    - Steamfitter Scenario Template
6. Click **Save** to apply your changes.

## Step 5: Launching and Validating

From the Alloy home page (you may have to exit the Alloy Administration screen):

1. Click **Open** next to the Event Template.
2. In the Event screen, click  **Launch**.

Observe the lifecycle. During launch, Alloy does the following:

1. Clones the Player View.
2. Applies the Caster workspace to vCenter.
3. Starts the Steamfitter scenario.
4. Makes the Player apps visible to teams and participants.

In Player, you can now interact with the apps you assigned to the View.

## Step 6: Exposing the Exercise in Moodle

To make Crucible labs or exercises available in Moodle, you have to use the **Crucible Moodle Plugin**. The plugin adds Crucible as an *activity type* in Moodle and provides the connection between Moodle and a deployed Crucible environment.

You can get the Crucible Moodle plugin from its official GitHub repository:
[Crucible Plugin for Moodle](https://github.com/cmu-sei/moodle-mod_crucible)

The Crucible Moodle plugin allows instructors and learners to launch, access, and stop Crucible exercises from within a Moodle course, without Moodle running the lab itself. Moodle manages access and courses, while Crucible runs the exercise.

Assuming that you:

- Run a working Moodle site.
- Run a deployed and operational Crucible stack.
- Configured authentication for Moodle and Crucible.
- Created a Crucible exercise.

The next steps are to:

1. Download the Crucible Moodle plugin from its GitHub repository.
2. Install the Crucible Moodle plugin on your Moodle site.
3. Configure the plugin so Moodle knows how to reach the Crucible environment.
4. Add a Crucible activity to a Moodle course.
5. Point that activity at an existing Crucible exercise.
6. Launch the exercise from the Moodle course (in an iframe) or in a new tab/window.

## Related Resources

- [Alloy Guide](../../alloy/index.md)
- [Caster Guide](../../caster/index.md)
- [Crucible Plugin for Moodle](https://github.com/cmu-sei/moodle-mod_crucible)
- [Moodle Documentation](https://docs.moodle.org/501/en/Main_page) and [installation instructions](https://docs.moodle.org/501/en/Installation_quick_guide)
- [Player Guide](../../player/index.md)
