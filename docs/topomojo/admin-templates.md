# Admin Templates

The **Templates** tab is where you can view all of the templates that exist in TopoMojo.

**Search:** Search for templates by workspace. Notice that you can apply filters here to further narrow down your search. In the screen print below, the filter is for linked VMs with a parent template of a VM called `kali-201901`.

![templates filter](img/templates-filter.png)

You can filter for specific workspaces here too. Clicking the *name* of the of the workspace takes you directly to the workspace.

!!! note "Linked and unlinked templates"

    The chain link icon next to a template name indicates the VM is *linked*. Use linked VMs when the prebuilt, stock templates included with TopoMojo meet your needs. Linked VMs save resources when VMs don't require custom configurations when deployed. Changes can't be saved to linked VMs when deployed. Changes can only be saved to *unlinked* VMs. 

## Template Properties

**Name:** The VM name can't contain spaces; Topo will replace spaces in a name with a `-`.

**Description:** Not visible to users; use the *Description* in a way that meets your needs. For example, VM credentials could be included here.

**Networks:** A space delimited list of network names. When the VM is deployed, it will have *one* network interface for each of the named networks. Networks are created on the hypervisor by TopoMojo at VM-deploy time if they don't already exist. 

TopoMojo appends the isolation tag of the workspace/gamespace to network names to ensure network isolation. 

TopoMojo does not append the isolation tag to persistent/shared networks listed here; the VM will be connected to the existing shared/persistent network.

For more information on *isolation tags*, see "Isolation tags" in [TopoMojo concepts](index.md).

**Guest Settings:** List key value pairs in the form of `key=value` to pass data into deployed VMs via VMware guestinfo Variables. The **Guest Settings** field uses VMware Guest Info Variables to inject content into virtual machines. Key/value pairs are placed here. The *key* is the name of the guest variable you want to define, and the *value* is value, information, setting, of the variable.  For example, `var1=test` is a guest setting named “var1” with a value of “test”.

**Replicas:** *Replicas* indicates how many copies of the VM get deployed in a gamespace. This will vary according to your needs. You may need two copies of the VM per gamespace or you may need 10. E.g.: two users are working a Topo lab together; we want to set Replicas to `2` to ensure that each user has their own VM to work with. If set to `1`, then the two users could encroach on each other's work on the single VM.

When deciding how many replicas are needed, keep resources in mind. If, as in our example above, we only need two copies of the VM at any given time don't set Replicas to `5`. Topo will deploy five, two will get used, and the three extra won't get used everytime the gamespace is deployed.

`-1`: Setting Replicas to `-1` means Topo will deploy one VM template copy per user. If there are two users, then two copies are deployed; if there are 10 users, then 10 copies. No extra VMs are deployed. This setting is used in conjunction with the Gameboard app, where Gameboard informs TopoMojo on how many copies to make based upon the Gameboard team size.

The value set in **Replicas** only applies to the template you are editing; not every template in the workspace. So, if you want the same number of copies deployed in a gamespace for each template, you'll have to edit each template individually.