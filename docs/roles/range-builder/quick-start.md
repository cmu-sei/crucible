# Range Builder Quick Start

This guide walks you through building your first cyber range in 60 minutes using the Crucible platform. You'll create a basic penetration testing lab with automated scenario injects.

## What You'll Build

- A 3-VM network topology (attacker, target server, monitoring station)
- An automated scenario with timed injects and manual checkpoints
- A team-based Player interface with documentation and VM access
- A complete exercise ready for participant deployment

## Prerequisites

- [Content Developer permissions](../roles/permissions.md) in all Crucible applications
- A basic Terraform module available in [Caster](../../caster/index.md)
- An understanding of your virtualization environment

---

## Step 1: Create Infrastructure (Caster) — 15 minutes

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

1. In the `pentest-topology` directory, click **Add New File**.  
2. Create `main.tf` with the following infrastructure definition:

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
