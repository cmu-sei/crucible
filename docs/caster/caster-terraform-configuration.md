# Caster Terraform Configuration

This topic is for anyone who manages a Crucible instance who wants to configure their Terraform provider installation for Caster. Terraform can be configured to only allow certain providers to be downloaded from the Internet and used from a local Filestore. 

Documentation describing this can be found in **HashiCorp's Terraform** documentation: **CLI Configuration File** > [Provider Installation](https://www.terraform.io/docs/cli/config/config-file.html#provider-installation).

For your reference, below is the `.terraformrc` file currently implemented in the SEI's CyberForce instance of Caster.

In the SEI's instance, we want to be able to use any plugins in the `sei` or `mastercard` namespace that have been downloaded locally.  In addition, any of the `hashicorp` namespace providers in the `direct` section can be downloaded directly from the Internet without any operator intervention.  

These plugins are then all cached in the `plugin_cache_dir` section, to save from downloading the providers during every Terraform `plan` and `apply`.

## Sample Caster Terraform Configuration

```
plugin_cache_dir = "/terraform/plugin-cache"
provider_installation {
	filesystem_mirror {        
		path = "/terraform/plugins/linux_amd64"
        include = [            
        	"registry.terraform.local/sei/*",            	
        	"registry.terraform.local/mastercard/*"        
        ]    
     }    
     direct {        
     	include = [
        "hashicorp/vsphere",
        "hashicorp/aws",            
        "hashicorp/azurerm",            
        "hashicorp/azuread",            
        "hashicorp/null",            
        "hashicorp/random",            
        "hashicorp/template",            
        "hashicorp/cloudinit"        
       ]    
      } 
     }
```

