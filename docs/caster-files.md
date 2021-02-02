# Caster Files

*Files* represent text files that will eventually be put onto a file system and used with the Terraform command line tool. Files can be named and edited through Caster, but file extensions are important and have specific meaning to Terraform. 

- `.tf` A configuration file that defines resources, variables, etc., in a Terraform configuration.
- `.auto.tfvars` Contains the values to be used for variables defined in `.tf` files.

>**Note:** When working with Files in Caster **CTRL+L** locks/unlocks a file to prevent others from editing that file simultaneously. When locked, the file icon appears as a dashed line. When unlocked, the file icon appears solid. Files can also be locked by an administrator. A file is *administratively locked* to prevent anyone from changing that file. A lock icon in the top right corner of the file edit screen denotes that the file is administratively locked. **CTRL+S** saves a file.

See the official [Terraform Documentation](https://www.terraform.io/docs/index.html) for more details on supported file types and extensions. In the future, Caster may provide more guidance on what types of files can be created and what their contents are expected to be.