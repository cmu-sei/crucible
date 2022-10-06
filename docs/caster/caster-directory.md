# Caster Directory

The top-level construct within a project is called a *directory*. A project can contain many directories. Directories contain files that make up a particular Terraform configuration, workspaces that represent a specific instance of that configuration, and sub-directories. Directories are meant to be used primarily for organization and shared resources.

## Directory Hierarchies

Directories can contain sub-directories to create a _hierarchy_ of directories and the configuration files contained within them. When a run is created in a workspace, the files in the workspace, the workspace's directory, ***and all parent directories*** are merged together and passed to Terraform as a single configuration. This eliminates redundancy when developing many environments that are largely the same, or sharing a set of common variables or data across many configurations. 

> For example, a large deployment might have a top-level directory that defines global variables like `vlan ids` and `team ids` in use, and then sub-directories that define resources that use those variables.

Users can add, rename, delete, or export a directory from the navigation panel on a project's main Caster page. 

Peer directories (directories that fall outside a parent directory) are not included in a run.
