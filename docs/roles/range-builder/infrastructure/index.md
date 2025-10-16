# Infrastructure Development

[Caster](../../caster/index.md) provides the foundation for all cyber range infrastructure through Terraform/OpenTofu integration. As a Range Builder, you'll design and implement network topologies that support your training objectives while maintaining flexibility and reusability.

The [Crucible Terraform Provider](https://registry.terraform.io/providers/cmu-sei/crucible/latest/docs) allows you to manage Crucible resources directly through Terraform configurations.

## Best Practices

### Projects and Directories

    - **Projects**: Top-level organizational containers
    - **Directories**: Hierarchical configuration structure
    - **Workspaces**: Deployed instances of configurations
    - **Files**: Individual Terraform configuration components

### Module-Based Architecture

  Caster promotes modular design through reusable Terraform modules:

    - Virtual machine modules with standardized configurations
    - Network topology modules for common patterns
    - Security tool modules with preconfigured services
    - Assessment modules for automated scoring

### Hierarchical Organization

    ```
    project-name/
    ├── global/                    # Global variables and data sources
    │   ├── variables.tf
    │   ├── data.tf
    │   └── provider.tf
    ├── networks/                  # Network infrastructure
    │   ├── core-network/
    │   ├── dmz-network/
    │   └── isolated-networks/
    ├── environments/              # Environment-specific configs
    │   ├── production/
    │   ├── development/
    │   └── shared-resources/
    └── scenarios/                 # Scenario-specific deployments
        ├── red-vs-blue/
        ├── incident-response/
        └── penetration-testing/
    ```

### Variable Management

    Use consistent variable patterns across all modules:

    ```hcl
    # Standard variables for all modules
    variable "exercise_id" {
      description = "Unique identifier for the exercise instance"
      type        = string
    }

    variable "team_id" {
      description = "Team identifier for resource isolation"
      type        = string
    }

    variable "team_name" {
      description = "Human-readable team name"
      type        = string
    }

    variable "environment" {
      description = "Environment designation (dev/staging/prod)"
      type        = string
      default     = "dev"
    }

    variable "resource_tags" {
      description = "Common tags applied to all resources"
      type        = map(string)
      default = {
        "ManagedBy" = "Crucible"
        "Platform"  = "Caster"
      }
    }
    ```

### Standardize VM Modules

    Create reusable VM modules that include common configurations:

    ```hcl
    # modules/vm-standard/main.tf
    resource "vsphere_virtual_machine" "vm" {
      name             = "${var.exercise_id}-${var.team_name}-${var.vm_name}"
      resource_pool_id = data.vsphere_resource_pool.pool.id
      datastore_id     = data.vsphere_datastore.datastore.id
      folder           = var.vm_folder

      num_cpus                   = var.cpu_count
      memory                     = var.memory_mb
      guest_id                   = var.guest_id
      wait_for_guest_net_timeout = var.network_timeout

      # Network configuration
      dynamic "network_interface" {
        for_each = var.networks
        content {
          network_id     = network_interface.value.network_id
          adapter_type   = network_interface.value.adapter_type
          mac_address    = network_interface.value.mac_address
        }
      }

      # Disk configuration
      dynamic "disk" {
        for_each = var.disks
        content {
          label            = disk.value.label
          size             = disk.value.size
          unit_number      = disk.value.unit_number
          thin_provisioned = disk.value.thin_provisioned
        }
      }

      # Template cloning
      clone {
        template_uuid = data.vsphere_virtual_machine.template.id

        customize {
          linux_options {
            host_name = "${var.team_name}-${var.vm_name}"
            domain    = var.domain_name
          }

          network_interface {
            ipv4_address = var.ip_address
            ipv4_netmask = var.netmask
          }

          ipv4_gateway    = var.gateway
          dns_server_list = var.dns_servers
        }
      }

      # Extra configuration for team/exercise context
      extra_config = {
        "guestinfo.teamId"     = var.team_id
        "guestinfo.exerciseId" = var.exercise_id
        "guestinfo.teamName"   = var.team_name
        "guestinfo.vmRole"     = var.vm_role
      }

      tags = values(data.vsphere_tag.tags)
    }
    ```

### Create Specific Modules for Common VM Roles

    (Domain Controller Module)

    ```hcl
    # modules/vm-domain-controller/main.tf
    module "dc_vm" {
      source = "../vm-standard"

      vm_name     = "domain-controller"
      vm_role     = "domain-controller"
      cpu_count   = 2
      memory_mb   = 4096
      guest_id    = "windows2019srv_64Guest"

      disks = [
        {
          label            = "system"
          size             = 60
          unit_number      = 0
          thin_provisioned = true
        },
        {
          label            = "data"
          size             = 40
          unit_number      = 1
          thin_provisioned = true
        }
      ]

      # Domain controller specific variables
      exercise_id = var.exercise_id
      team_id     = var.team_id
      team_name   = var.team_name
    }

    # Additional configuration for domain setup
    resource "vsphere_virtual_machine_snapshot" "dc_configured" {
      virtual_machine_uuid = module.dc_vm.uuid
      snapshot_name        = "Domain Configured"
      description          = "Snapshot after domain controller setup"
      memory               = false
      quiesce              = true

      depends_on = [null_resource.domain_setup]
    }

    resource "null_resource" "domain_setup" {
      provisioner "local-exec" {
        command = "ansible-playbook -i ${module.dc_vm.default_ip_address}, domain-setup.yml"
      }

      depends_on = [module.dc_vm]
    }
    ```

## Summary of Best Practices

### Configuration Management

- Use consistent naming conventions across all resources
- Implement proper variable validation and defaults
- Create modular, reusable components
- Document all custom modules and configurations

### Resource Management

- Plan for resource constraints and scaling
- Implement proper cleanup and destruction procedures
- Use tags consistently for organization and automation
- Monitor resource usage and costs

### Security Considerations

- Implement network segmentation from the start
- Use least-privilege access patterns
- Encrypt sensitive data in configurations
- Plan for credential rotation and management

### Testing and Validation

- Test configurations in development environments
- Validate resource allocation before deployment
- Implement automated testing for common scenarios
- Document troubleshooting procedures
