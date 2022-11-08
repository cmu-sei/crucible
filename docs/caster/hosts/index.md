# Caster Hosts

A *host* consists of a name, datastore, and maximum number of virtual machines that it can support. Hosts are created and managed through the API. After a host is created, it can be assigned to an exercise. An exercise can have many hosts. 

Workspaces have an additional property, `DynamicHost`, which is usually set to `false`. When Alloy creates a workspace, this is set to `true`, and changes the behavior of a run. When `DynamicHost` is `true`, Caster examines all of the hosts assigned to the current exercise and chooses the one with the least usage (the number of machines to deploy/maximum machines) to assign to the workspace. 

Along with all of the files normally added to the run, Caster will create a `generated_host_values.auto.tfvars` containing two variable values: `vsphere_host_name` and `vsphere_datastore`, which will be set to the name and datastore of the selected host. When the run is applied, Caster tracks how many virtual machines are deployed to the host and uses it for future calculations. 

When the workspace is deleted after an on-demand exercise (ODX) is finished, the host's resources will be released. If a run attempts to deploy more virtual machines than there is capacity for in the available hosts, the run will fail.

## On-Demand Exercise functionality

Caster is called by Alloy in order to deploy resources for lab or ODX-style functionality. Caster itself does not differ in its main functionality of deploying workspaces and lets Alloy handle most of the ODX functionality. 

However, in order to support this functionality Caster dynamically selects a host to deploy to.

Normally, the cluster or host to deploy to is embedded in the configuration - either directly or as a variable - and Caster doesn't concern itself with this. For ODX's, Caster *does* need to concern itself with:

- ensuring that resources are deployed evenly to the available hosts, and 
- more ODX's than the hardware can deploy are not deployed. 

To address these concerns the concept of a *host* was added to Caster.