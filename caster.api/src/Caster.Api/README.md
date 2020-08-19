# Caster API

## Development `appsettings`

.NET Core supports multiple `appsettings` files that are read based on the environment. Currently only the `Development`
environment is used. This means that you can make a file named `appsettings.Development.json` in the same directory as
`appsettings.json` and any settings in `appsettings.Development.json` will override the settings given in
`appsettings.json`. Most importantly, it means you won't have any issues if the settings file changes and you want the
latest code.

## Database

Currently, only PostgreSQL is supported. The simplest way to create one is to run a PostgreSQL Docker container locally,
but instructions for that are outside the scope of this document.

Once you have your database configured, you'll need to add the connection string from `appsettings.json` into your
`appsettings.Development.json`. You'll need to add the Section and the key/value you want to use:

    "ConnectionStrings": {
      "PostgreSQL": "Server=localhost;Port=5432;Database=caster_api;Username=;Password=;"
    }

You'll just need to add the Username and Password for your database into this line.

## Authentication and Authorization

You will also need to run an identity server. Configuring one is far outside the scope of this document. On your
identity server, you will need to add a client named `caster-api` and a scope also named `caster-api`.

However, once you have your identity server up and running, you'll need to add some additional lines in
`appsettings.Development.json`. Find the following lines

    "Authorization": {
      "Authority": "https://localhost:5000",
      "AuthorizationUrl": "https://localhost:5000/connect/authorize",
    }

and update the domains/ports in these lines to point to your identity server.

## Terraform

Caster uses Terraform (https://www.terraform.io/) to perform it's deployment functions. There are some required settings for configuring this.

BinaryPath - The path to a directory where terraform binaries will be stored. To make a terraform version available to be used in Caster, create a sub-directory named for the version number, containing the terraform binary within it. For example, to make 0.12.28 and 0.12.29 available, BinaryPath should point to a directory containing 2 sub-directories, named 0.12.28 and 0.12.29, each containing the corresponding terraform version's binary.

DefaultVersion - The version of terraform that will be used if no specific version is set on a workspace

PluginDirectory - The path to a directory of any necessary terraform plugins. For use in offline environments or where you do not want additional plugins to be downloaded. The vsphere plugin is a common requirement. If this is left blank, plugins will attempt to be downloaded on the fly by terraform.

Note: terraform 0.13 significantly changes the directory structure it expects plugins to be found in. Please read the terraform documentation carefully when moving to 0.13+.

RootWorkingDirectory - The path to a directory in which Caster can create sub-directories for use in it's terraform integration.

OutputSaveInterval - The interval (in milliseconds) in which an ongoing terraform process will write it's output to the database. The default is 5000, so during a `terraform apply`, for example, the output will be updated every 5 seconds until the command has completed.

## Example `appsettings.Development.json`

    {
      "ConnectionStrings": {
        "PostgreSQL": "Server=localhost;Port=5432;Database=caster_api;Username=user;Password=password;"
      },
      "Authorization": {
        "Authority": "https://id.my.local.server",
        "AuthorizationUrl": "https://id.my.local.server/connect/authorize",
      },
      "Terraform": {
        "BinaryPath": "/terraform/binaries",
        "DefaultVersion": "0.12.29",
        "PluginDirectory": "/terraform/terraform.d/plugins/linux_amd64",
        "RootWorkingDirectory": "/terraform/root"
      }
    }

## Change Port

By default, Caster API listens on port 4309. In case you want to change this, open the file
`Properties/launchSettings.json` and find the line

    "applicationUrl": "http://localhost:4309",

and change the port in this line.
