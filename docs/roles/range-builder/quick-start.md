# Range Builder Quick Start

Build your first cyber range in 60 minutes using Crucible. This guide creates a basic penetration testing lab with automated scenario progression.

## What You Will Build

- 3-VM network topology (attacker, target server, monitoring station)
- Automated scenario with timed injects and manual checkpoints
- Team-based Player interface with documentation and VM access
- Deployable exercise ready for participants

## Prerequisites

- [Content Developer permissions](../../roles/permissions.md) in all Crucible applications
- Basic Terraform module available in [Caster](../../caster/index.md)
- Understanding of your virtualization environment

## Step 1: Create Infrastructure (Caster)-15 minutes

### 1.1 Create Project and Directory

1. Navigate to Caster → **Projects**
2. Click **Add New Project**
3. Enter:
   - **Name**: `PenTest-Lab-Basic`
   - **Description**: `Basic penetration testing laboratory`
4. Click **Save**
5. In the new project, click **Add Directory**
6. Enter:
   - **Name**: `pentest-topology`
   - **Description**: `Core network topology`

### 1.2 Create Base Configuration

In the `pentest-topology` directory, create three files:

**main.tf** - Core infrastructure:

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

**variables.tf** - Configuration parameters:

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

**data.tf** - Environment references:

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

## Step 2: Create Player View-10 minutes

### 2.1 Create New View

1. Navigate to Player → **Administration** → **Views**
2. Click **Add New View**
3. Enter:
   - **Name**: `PenTest Lab - Basic`
   - **Description**: `Basic penetration testing laboratory`
   - **Status**: Active

### 2.2 Add Applications

VM List Application:

1. Click **Applications** tab
2. Click **Add New Application**
3. Select **Template** → **VM List**
4. Enter:
   - **Name**: `Virtual Machines`
   - **Icon**: `fas fa-desktop`

Documentation Application:

1. Click **Add New Application**
2. Select **Blank Application**
3. Enter:
   - **Name**: `Lab Guide`
   - **URL**: `https://your-lab-guide-url.com`
   - **Icon**: `fas fa-book`

### 2.3 Create Teams

1. Click **Teams** tab
2. Click **Add New Team**
3. Enter:
   - **Team Name**: `Red Team`
   - **Role**: `Participant`
4. Add both applications (`Virtual Machines` and `Lab Guide`)
5. Add users to team using search function

## Step 3: Create Scenario (Steamfitter)-20 minutes

### 3.1 Create Scenario Template

1. Navigate to Steamfitter → **Scenario Templates**
2. Click **Add New Scenario Template**
3. Enter:
   - **Name**: `PenTest-Basic-Scenario`
   - **Description**: `Basic penetration testing scenario with automated progression`
   - **Duration**: `3600` (1 hour)

### 3.2 Add Tasks

Create tasks in this sequence to build progression:

Task 1: Initial Setup (Manual)

- **Name**: `Environment Setup Complete`
- **Description**: `Confirm all systems are online and accessible`
- **Action**: `Manual Task`
- **Trigger Condition**: `Manual`
- **VM Mask**: `attacker`
- **Expected Output**: `ready`

Task 2: Discovery Phase (Timed)

- **Name**: `Start Network Discovery`
- **Description**: `Begin network reconnaissance phase`
- **Action**: `guest.run_command`
- **Trigger Condition**: `Success` (depends on Task 1)
- **Delay**: `300` (5 minutes after setup)
- **VM Mask**: `attacker`
- **Command**: `nmap -sn 192.168.1.0/24`
- **Expected Output**: `192.168.1`

Task 3: Vulnerability Scanning (Timed)

- **Name**: `Vulnerability Scan`
- **Description**: `Scan target for vulnerabilities`
- **Action**: `guest.run_command`
- **Trigger Condition**: `Success` (depends on Task 2)
- **Delay**: `600` (10 minutes after discovery)
- **VM Mask**: `attacker`
- **Command**: `nmap -sV -O target-ip`
- **Expected Output**: `open`

Task 4: Exploitation Checkpoint (Manual)

- **Name**: `Exploitation Attempt`
- **Description**: `Manual exploitation phase - report findings`
- **Action**: `Manual Task`
- **Trigger Condition**: `Success` (depends on Task 3)
- **VM Mask**: `attacker`
- **Expected Output**: `exploited`

Task 5: Monitoring Alert (Timed)

- **Name**: `Security Alert Generated`
- **Description**: `Generate security alert on monitoring system`
- **Action**: `guest.run_command`
- **Trigger Condition**: `Time`
- **Delay**: `1800` (30 minutes from scenario start)
- **VM Mask**: `monitor`
- **Command**: `logger "ALERT: Suspicious network activity detected"`
- **Expected Output**: `ALERT`

## Step 4: Create Alloy Definition-5 minutes

### 4.1 Create Definition

1. Navigate to Alloy → **Administration** → **Event Templates**
2. Click **Add New Definition**
3. Enter:
   - **Name**: `PenTest Lab - Basic Event`
   - **Description**: `Complete basic penetration testing exercise`
   - **Duration**: `3600`

### 4.2 Link Components

Connect your range components:

1. **Player Exercise ID**: Copy View ID from Player (found in view details or URL)
2. **Caster Directory ID**: Copy Directory ID from `pentest-topology` in Caster
3. **Steamfitter Scenario ID**: Copy ID from `PenTest-Basic-Scenario` in Steamfitter
4. Click **Save Definition**

## Step 5: Test Deployment-10 minutes

### 5.1 Launch Test Event

1. In Alloy user interface, find your definition
2. Click **Launch**
3. Monitor deployment:
   - Caster workspace creation
   - Terraform plan and apply
   - Steamfitter scenario start
   - Player exercise clone

### 5.2 Validate Components

**Infrastructure Check:**

- Navigate to Caster workspace
- Verify all VMs deployed successfully
- Check resource states show healthy

**Player Access:**

- Open Player as test user
- Confirm VM list displays all machines
- Test VM console access

**Scenario Execution:**

- Navigate to Steamfitter scenario
- Verify manual tasks appear for participants
- Confirm timed tasks show countdown

### 5.3 End Test Event

1. Return to Alloy
2. Click **End Event**
3. Verify cleanup:
   - VMs destroyed in Caster
   - Steamfitter scenario ended
   - Player exercise removed

## You're Done

Your basic penetration testing lab is complete with:

- Automated infrastructure deployment
- Progressive scenario with timed events
- Team-based user interface
- Full exercise lifecycle management

## Next Steps

**Enhance Infrastructure:**

- Add complex networking topologies
- Include additional security tools
- Implement realistic enterprise environment

**Expand Scenario:**

- Add conditional branching based on participant actions
- Create multiple difficulty paths
- Include assessment and scoring

**Improve User Experience:**

- Add custom maps and navigation
- Create comprehensive documentation
- Set up chat and collaboration tools

**Scale for Multiple Teams:**

- Create team-specific variations
- Implement competitive elements
- Add real-time monitoring and analytics
