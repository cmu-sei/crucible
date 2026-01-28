# Range Builder Quick Start Guide

Build your first cyber range in 60 minutes using Crucible. This quick start walks through creating a basic penetration testing lab with automated scenario progression in five steps. These steps are:

1. [**Step 1:**](#step-1-create-the-infrastructure-caster-15-minutes) Build the infrastructure in Caster by creating a project, directory, and Terraform files for a 3‑VM topology.
2. [**Step 2:**](#step-2-create-player-view-10-minutes) Create a Player view with applications and teams to present the lab experience.
3. [**Step 3:**](#step-3-create-scenario-steamfitter-20-minutes) Define a Steamfitter scenario with a sequence of manual and timed tasks.
4. [**Step 4:**](#step-4-create-alloy-definition-5-minutes) Create an Alloy event definition that links the Player view, Caster directory, and Steamfitter scenario.
5. [**Step 5:**](#step-5-test-deployment-10-minutes) Launch and validate a test event end‑to‑end, then end it and confirm cleanup.

## What You Will Build

Upon completion of this quick start, you will have:

- 3-VM network topology (attacker, target server, monitoring station).
- Automated scenario with timed injects and manual checkpoints.
- Team-based Player interface with documentation and VM access.
- Deployable exercise ready for participants.

## Prerequisites

Before starting, confirm the following:

- Content Developer permissions in all required Crucible applications.
- Access to a basic Terraform module in Caster.
- Familiarity with the target virtualization environment.

## Step 1: Create the Infrastructure (Caster) ~15 minutes

### Create a Project and Directory

1. Navigate to Caster, **Projects**.
2. Click **Add New Project** and enter:

     - **Name:** `PenTest-Lab-Basic`
     - **Description:** `Basic penetration testing laboratory`

3. Save the project.
4. In the new project, click **Add Directory** and enter:

     - **Name:** `pentest-topology`
     - **Description:** `Core network topology`

### Create the Base Configuration

In the `pentest-topology` directory, create three files:

1. `main.tf`: core infrastructure
2. `variables.tf`: configuration parameters
3. `data.tf`: environment references

#### Main.tf

```hcl
# Basic 3-VM topology for penetration testing
resource "vsphere_virtual_machine" "attacker_vm" {
  name             = "${var.exercise_id}-attacker"
  resource_pool_id = data.vsphere_resource_pool.pool.id
  datastore_id     = data.vsphere_datastore.datastore.id

  num_cpus = 2
  memory   = 4096
  guest_id = "ubuntu64Guest"

  network_interface {
    network_id = data.vsphere_network.network.id
  }

  disk {
    label = "disk0"
    size  = 40
  }

  clone {
    template_uuid = data.vsphere_virtual_machine.template_kali.id
  }
}

resource "vsphere_virtual_machine" "target_server" {
  name             = "${var.exercise_id}-target"
  resource_pool_id = data.vsphere_resource_pool.pool.id
  datastore_id     = data.vsphere_datastore.datastore.id

  num_cpus = 1
  memory   = 2048
  guest_id = "ubuntu64Guest"

  network_interface {
    network_id = data.vsphere_network.network.id
  }

  disk {
    label = "disk0"
    size  = 20
  }

  clone {
    template_uuid = data.vsphere_virtual_machine.template_vulnerable.id
  }
}

resource "vsphere_virtual_machine" "monitor_vm" {
  name             = "${var.exercise_id}-monitor"
  resource_pool_id = data.vsphere_resource_pool.pool.id
  datastore_id     = data.vsphere_datastore.datastore.id

  num_cpus = 2
  memory   = 4096
  guest_id = "ubuntu64Guest"

  network_interface {
    network_id = data.vsphere_network.network.id
  }

  disk {
    label = "disk0"
    size  = 30
  }

  clone {
    template_uuid = data.vsphere_virtual_machine.template_monitor.id
  }
}
```

#### Variables.tf

```hcl
variable "exercise_id" {
  description = "Unique identifier for the exercise"
  type        = string
}

variable "team_name" {
  description = "Team name for resource naming"
  type        = string
}

variable "vsphere_host_name" {
  description = "Target vSphere host"
  type        = string
}

variable "vsphere_datastore" {
  description = "Target datastore"
  type        = string
}
```

#### Data.tf

```hcl
data "vsphere_datacenter" "dc" {
  name = "Crucible-DC"
}

data "vsphere_resource_pool" "pool" {
  name          = "Crucible-Pool"
  datacenter_id = data.vsphere_datacenter.dc.id
}

data "vsphere_datastore" "datastore" {
  name          = var.vsphere_datastore
  datacenter_id = data.vsphere_datacenter.dc.id
}

data "vsphere_network" "network" {
  name          = "VM Network"
  datacenter_id = data.vsphere_datacenter.dc.id
}

data "vsphere_virtual_machine" "template_kali" {
  name          = "kali-linux-template"
  datacenter_id = data.vsphere_datacenter.dc.id
}

data "vsphere_virtual_machine" "template_vulnerable" {
  name          = "vulnerable-server-template"
  datacenter_id = data.vsphere_datacenter.dc.id
}

data "vsphere_virtual_machine" "template_monitor" {
  name          = "monitoring-template"
  datacenter_id = data.vsphere_datacenter.dc.id
}
```

## Step 2: Create Player View ~10 minutes

### Create a New View

1. Navigate to **Player**, **Administration**, **Views**.
2. Click **Add New View**.
3. Enter:

     - **Name:** `PenTest Lab - Basic`
     - **Description:** `Basic penetration testing laboratory`
     - **Status:** Active

### Add Applications

#### VM List Application

1. Open the **Applications** tab.
2. Click **Add New Application**.
3. Select **Template**, **VM List**.
4. Enter:

     - **Name:** `Virtual Machines`
     - **Icon:** `fas fa-desktop`

#### Documentation Application

1. Click **Add New Application**.
2. Select **Blank Application**.
3. Enter:

     - **Name:** `Lab Guide`
     - **URL:** `https://your-lab-guide-url.com`
     - **Icon:** `fas fa-book`

### Create a Team

1. Open the **Teams** tab.
2. Click **Add New Team**.
3. Enter:

     - **Team Name:** `Red Team`
     - **Role:** `Participant`

4. Assign both applications, `Virtual Machines` and `Lab Guide`.
5. Add users to the team using the search function.

## Step 3: Create Scenario (Steamfitter) ~20 minutes

### Create a Scenario Template

1. Navigate to **Steamfitter**, **Scenario Templates**.
2. Click **Add New Scenario Template**.
3. Enter:

     - **Name:** `PenTest-Basic-Scenario`
     - **Description:** `Basic penetration testing scenario with automated progression`
     - **Duration:** `3600` (1 hour)

### Add Scenario Tasks

Create the following tasks in order to build scenario progression.

Task 1: Initial Setup (Manual)

- **Name:** `Environment Setup Complete`
- **Description:** `Confirm all systems are online and accessible`
- **Action:** `Manual Task`
- **Trigger Condition:** `Manual`
- **VM Mask:** `attacker`
- **Expected Output:** `ready`

Task 2: Discovery Phase (Timed)

- **Name:** `Start Network Discovery`
- **Description:** `Begin network reconnaissance phase`
- **Action:** `guest.run_command`
- **Trigger Condition:** `Success` (depends on previous task)
- **Delay:** `300` (5 minutes after setup)
- **VM Mask:** `attacker`
- **Command:** `nmap -sn 192.168.1.0/24`
- **Expected Output:** `192.168.1`

Task 3: Vulnerability Scanning (Timed)

- **Name:** `Vulnerability Scan`
- **Description:** `Scan target for vulnerabilities`
- **Action:** `guest.run_command`
- **Trigger Condition:** `Success` (depends on previous task)
- **Delay:** `600` (10 minutes after discovery)
- **VM Mask:** `attacker`
- **Command:** `nmap -sV -O target-ip`
- **Expected Output:** `open`

Task 4: Exploitation Checkpoint (Manual)

- **Name:** `Exploitation Attempt`
- **Description:** `Manual exploitation phase - report findings`
- **Action:** `Manual Task`
- **Trigger Condition:** `Success` (depends on previous task)
- **VM Mask:** `attacker`
- **Expected Output:** `exploited`

Task 5: Monitoring Alert (Timed)

- **Name:** `Security Alert Generated`
- **Description:** `Generate security alert on monitoring system`
- **Action:** `guest.run_command`
- **Trigger Condition:** `Time`
- **Delay:** `1800` (30 minutes from scenario start)
- **VM Mask:** `monitor`
- **Command:** `logger "ALERT: Suspicious network activity detected"`
- **Expected Output:** `ALERT`

## Step 4: Create Alloy Definition ~5 minutes

### Create an Event Definition

1. Navigate to **Alloy**, **Administration**, **Event Templates**.
2. Click **Add New Definition**.
3. Enter:

     - **Name:** `PenTest Lab - Basic Event`
     - **Description:** `Complete basic penetration testing exercise`
     - **Duration:** `3600`

### Link Range Components

Connect the range components to the event definition:

1. **Player Exercise ID:** Copy the View ID from Player (available in the view details or URL).
2. **Caster Directory ID:** Copy the Directory ID from `pentest-topology` in Caster.
3. **Steamfitter Scenario ID:** Copy the Scenario ID from `PenTest-Basic-Scenario` in Steamfitter.
4. Click **Save Definition**.

## Step 5: Test Deployment ~10 minutes

### Launch a Test Event

1. In Alloy, locate your event definition.
2. Click **Launch**.
3. Monitor deployment progress:

     - Caster workspace creation
     - Terraform plan and apply
     - Steamfitter scenario start
     - Player exercise clone

### Validate Components

#### Infrastructure

- Navigate to the Caster workspace.
- Verify all virtual machines deploy successfully.
- Confirm resource states report healthy.

#### Player Access

- Open Player as a test user.
- Confirm the VM list displays all machines.
- Verify VM console access.

#### Scenario Execution

- Navigate to the Steamfitter scenario.
- Confirm manual tasks appear for participants.
- Verify timed tasks display countdowns.

### End the Test Event

1. Return to Alloy.
2. Click End Event.
3. Verify cleanup:

     - Caster destroys all virtual machines.
     - Steamfitter ends the scenario.
     - Player removes the exercise.

## Lab Complete

This quick start results in a basic penetration testing lab that includes:

- Automated infrastructure deployment.
- Scenario progression with timed events.
- Team-based user interface.
- Full exercise lifecycle management.

## Next Steps

### Enhance Infrastructure

- Add more complex networking topologies.
- Include additional security tools.
- Implement a realistic enterprise environment.

### Expand the Scenario

- Add conditional branching based on participant actions.
- Create multiple difficulty paths.
- Include assessment and scoring.

### Improve User Experience

- Add custom maps and navigation.
- Create comprehensive documentation.
- Set up chat and collaboration tools.

### Scale for Multiple Teams

- Create team-specific variations.
- Implement competitive elements.
- Add real-time monitoring and analytics.
