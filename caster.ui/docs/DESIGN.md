### Caster
Caster aims to be a design tool for project deployment. Caster will initially support
terraform deployments.

#### Initial release
The initial release of Caster will focus on the following core features:
  - a way to import terraform config files `.tf` and variable files `.tfvars`
    - Drag and drop or multiple file import.
    - Ignore state files `.tfstate, .tfstate.backup`
    - Directory structure import
  - organize those files in a folder structure
    - Recreate directory structure
    - Add a folder
    - Add a file
  - edit imported files in an online editor
  - initialize a terraform config `terraform init`
  - A button to apply a terraform config
  - A way to review terraform changes / output (websockets)
