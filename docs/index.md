# Crucible Framework

Developed by Carnegie Mellon University's Software Engineering Institute (SEI), Crucible is a modular framework for creating, deploying, and managing virtual environments to support training, education, and exercises. Within the Crucible framework are the following applications and plugins.

## Core Applications

### Alloy

Alloy joins the other independent Crucible apps together to provide a complete Crucible experience (i.e. labs, on-demand exercises, exercises, etc.). To get started with Alloy, see: 
- [Alloy API Repository](https://github.com/cmu-sei/Alloy.Api)
- [Alloy UI Repository](https://github.com/cmu-sei/Alloy.ui)

### Caster

Caster is the primary deployment component of the Crucible framework. Caster is built upon [Terraform](https://www.terraform.io/), an open source "Infrastructure as Code" tool. Caster provides a web interface that gives exercise developers a way to create, share, and manage topology configurations. To get started with Caster, see:
- [Caster API Repository](https://github.com/cmu-sei/Caster.Api)
- [Caster UI Repository](https://github.com/cmu-sei/Caster.Ui)

### Player

Player is the centralized interface where users, teams, and administrators go to participate in the cyber exercise. To get started with the various components of Player, see: 

- [Player API Repository](https://github.com/cmu-sei/Player.Api)
- [Player Console UI Repository](https://github.com/cmu-sei/Console.Ui)
- [Player UI Repository](https://github.com/cmu-sei/Player.Ui)
- [Player VM API Repository](https://github.com/cmu-sei/Vm.Api)
- [Player VM UI Repository](https://github.com/cmu-sei/Vm.Ui)

### Steamfitter

Steamfitter gives exercise developers the ability to create scenarios consisting of a series of scheduled tasks, manual tasks, and injects which run against virtual machines in an exercise. To get started with Steamfitter, see: 

- [Steamfitter API Repository](https://github.com/cmu-sei/Steamfitter.Api)
- [Steamfitter UI Repository](https://github.com/cmu-sei/Steamfitter.Ui)

Steamfitter relies upon [StackStorm](https://stackstorm.com/), an open source event-driven platform used to automate workflows, to execute commands.

## Crucible Appliance

The Crucible appliance is an environment that includes everything needed to install and configure the core applications of the Crucible framework. The appliance application stack consists of a single-node Docker swarm utilizing a Traefik reverse proxy. They are assembled using Docker Compose files on an Ubuntu 20.04 operating system. To get started with the Crucible appliance, see:

- [Crucible Appliance Repository](https://github.com/cmu-sei/Crucible.Appliance)

## Plugins

### Crucible Common Modules

Crucible common modules are a set of Angular modules that are common between Crucible apps. For more information, see:
- [Crucible Common Modules Repository](https://github.com/cmu-sei/Crucible.Common.Ui)

### Crucible plugin for Moodle

The Crucible plugin for Moodle is an activity plugin that allows Crucible labs and exercises to be accessed from the Moodle open-source learning management system. For more information, see:

- [Crucible plugin for Moodle Repository](https://github.com/cmu-sei/moodle-mod_crucible)

### osTicket

osTicket (https://osticket.com/) is a widely-used open source support ticket system that can be configured and deployed for an exercise. To get started with the Crucible plugin for osTicket, see:
- [osTicket Repository](https://github.com/cmu-sei/osticket-crucible)

### Terraform Provider for Crucible

This is the [Terraform](https://www.terraform.io/) Provider for Crucible which is used to create many Crucible resource types (e.g., Player Virtual Machines, Views, Applications, and others). For more information, see: 

- [Terraform Provider Crucible Repository](https://github.com/cmu-sei/terraform-provider-crucible)

### Terraform Provider for Identity

This is the [Terraform](https://www.terraform.io/) Provider for [Identity](https://github.com/cmu-sei/Identity) that creates and manages user accounts and other resources  using the Identity API. For additional information, see: 

- [Terraform Provider Identity Repository](https://github.com/cmu-sei/terraform-provider-identity)

### Welder

Welder is a simple application that can be added to an exercise; Welder allows users to dynamically load a VM workstation. To get started with Welder, see:
- [Welder Repository](https://github.com/cmu-sei/Welder)

## Documentation

You can find documentation on Crucible and all of its components [here](https://cmu-sei.github.io/crucible/).

## Reporting bugs and requesting features

Think you found a bug? Please report all Crucible bugs - including bugs for the individual Crucible apps - in the [cmu-sei/crucible issue tracker](https://github.com/cmu-sei/crucible/issues). 

Include as much detail as possible including steps to reproduce, specific app involved, and any error messages you may have received.

Have a good idea for a new feature? Submit all new feature requests through the [cmu-sei/crucible issue tracker](https://github.com/cmu-sei/crucible/issues). 

Include the reasons why you're requesting the new feature and how it might benefit other Crucible users.

## License

Copyright 2021 Carnegie Mellon University. See the [LICENSE.md](https://github.com/cmu-sei/crucible/blob/master/license.md) file for details.