# Crucible Framework

Developed by Carnegie Mellon University's Software Engineering Institute (SEI), Crucible is a modular framework for creating, deploying, and managing virtual environments to support training, education, and exercises. Within the Crucible framework are the following applications and plugins.

## Applications

### Alloy

Crucible's [**Alloy**](https://cmu-sei.github.io/crucible/alloy/) application enables users to launch an on-demand event or join an instance of an already-running simulation. Following the event, reports can provide a summary of knowledge and performance assessments.

- [Alloy API Repository](https://github.com/cmu-sei/Alloy.Api)
- [Alloy UI Repository](https://github.com/cmu-sei/Alloy.ui)

### Caster

Crucible's [**Caster**](https://cmu-sei.github.io/crucible/caster/) application enables the "coded" design and deployment of a cyber topology. Using Caster Designs, a novice content developer can avoid scripting OpenTofu code by simply defining variables within pre-configured OpenTofu modules. Caster supports the design and deployment of virtual environments to three types of hypervisors:

- [Caster API Repository](https://github.com/cmu-sei/Caster.Api)
- [Caster UI Repository](https://github.com/cmu-sei/Caster.Ui)

### Player

Crucible's [**Player**](https://cmu-sei.github.io/crucible/player/) application is the user's window into the virtual environment. Player enables assignment of team membership as well as customization of a responsive, browser-based user-interfaces using various integrated applications. A Crucible system administrator can shape how scenario information, assessments, and virtual environments are presented through the use of integrated applications.

- [Player API Repository](https://github.com/cmu-sei/Player.Api)
- [Player Console UI Repository](https://github.com/cmu-sei/Console.Ui)
- [Player UI Repository](https://github.com/cmu-sei/Player.Ui)
- [Player VM API Repository](https://github.com/cmu-sei/Vm.Api)
- [Player VM UI Repository](https://github.com/cmu-sei/Vm.Ui)

### Steamfitter

Crucible's [**Steamfitter**](https://cmu-sei.github.io/crucible/steamfitter/) application enables the organization and execution of scenario tasks on virtual machines.

- [Steamfitter API Repository](https://github.com/cmu-sei/Steamfitter.Api)
- [Steamfitter UI Repository](https://github.com/cmu-sei/Steamfitter.Ui)

Steamfitter relies upon [StackStorm](https://stackstorm.com/), an open source event-driven platform used to automate workflows, to execute commands.

### TopoMojo

Crucible's [**TopoMojo**](https://cmu-sei.github.io/crucible/topomojo/about/) application enables designing simple labs and challenges using form based configurations. Select and configure virtual machines, define networks, and write a guide.

- [TopoMojo API Repository](https://github.com/cmu-sei/TopoMojo)
- [TopoMojo UI Repository](https://github.com/cmu-sei/topomojo-ui)

### Gameboard

Crucible's [**Gameboard**](https://cmu-sei.github.io/crucible/Gameboard/) application provides game design capabilities and a competition-ready user interface for running your own cybersecurity game.

- [Gameboard API Repository](https://github.com/cmu-sei/Gameboard)
- [Gameboard UI Repository](https://github.com/cmu-sei/Gameboard-ui)

### Blueprint

Crucible's [**Blueprint**](https://cmu-sei.github.io/crucible/blueprint/) application enables the collaborative creation and visualization of a master scenario event list (MSEL) for an exercise. Scenario events are mapped to specific simulation objectives.

- [Blueprint API Repository](https://github.com/cmu-sei/Blueprint.Api)
- [Blueprint UI Repository](https://github.com/cmu-sei/Blueprint.Ui)

### Gallery

Crucible's [**Gallery**](https://cmu-sei.github.io/crucible/gallery/) application enables participants to review cyber incident data by source type. Source type examples include: intelligence, reporting, orders, news, social media, telephone, and email. Information is grouped by critical infrastructure sector or other organizational categories.

- [Gallery API Repository](https://github.com/cmu-sei/Gallery.Api)
- [Gallery UI Repository](https://github.com/cmu-sei/Gallery.Ui)

### CITE

Crucible's [**Collaborative Incident Threat Evaluator (CITE)**](https://cmu-sei.github.io/crucible/cite/) application enables participants from different organizations to evaluate, score, and comment on cyber incidents. CITE also provides a situational awareness dashboard that allows teams to track their internal actions and roles.

- [CITE API Repository](https://github.com/cmu-sei/CITE.Api)
- [CITE UI Repository](https://github.com/cmu-sei/CITE.Ui)


## Crucible Appliance

The Crucible appliance is an environment that includes everything needed to install and configure the core applications of the Crucible framework. The appliance application stack consists of a single-node Docker swarm utilizing a Traefik reverse proxy. They are assembled using Docker Compose files on an Ubuntu 20.04 operating system. To get started with the Crucible appliance, see:

- [Crucible Appliance Repository](https://github.com/cmu-sei/Crucible.Appliance)

## Plugins

### Crucible Common Modules

Crucible common modules are a set of Angular modules that are common between Crucible apps. For more information, see:

- [Crucible Common Modules Repository](https://github.com/cmu-sei/Crucible.Common.Ui)

### Crucible Plugin for Moodle

The Crucible plugin for Moodle is an activity plugin that allows Crucible labs and exercises to be accessed from the Moodle open-source learning management system. For more information, see:

- [Crucible plugin for Moodle Repository](https://github.com/cmu-sei/moodle-mod_crucible)

### osTicket

[osTicket](https://osticket.com/) is a widely-used open source support ticket system that can be configured and deployed for an exercise. To get started with the Crucible plugin for osTicket, see:

- [osTicket Repository](https://github.com/cmu-sei/osticket-crucible)

### Terraform Provider for Crucible

This is the [Terraform](https://www.terraform.io/) Provider for Crucible which is used to create many Crucible resource types (e.g., Player Virtual Machines, Views, Applications, and others). For more information, see:

- [Terraform Provider Crucible Repository](https://github.com/cmu-sei/terraform-provider-crucible)

### Terraform Provider for Identity

This is the [Terraform](https://www.terraform.io/) Provider for [Identity](https://github.com/cmu-sei/Identity) that creates and manages user accounts and other resources  using the Identity API. For additional information, see:

- [Terraform Provider Identity Repository](https://github.com/cmu-sei/terraform-provider-identity)

## Documentation

You can find documentation on Crucible and all of its components [here](https://cmu-sei.github.io/crucible/).

## Reporting Bugs and Requesting Features

:bug: Think you found a bug? Please report all Crucible bugs - including bugs for the individual Crucible apps - in the [cmu-sei/Crucible issue tracker](https://github.com/cmu-sei/crucible/issues).

Include as much detail as possible including steps to reproduce, specific app involved, and any error messages you may have received.

Have a good idea for a new feature? Submit all new feature requests through the [cmu-sei/Crucible issue tracker](https://github.com/cmu-sei/crucible/issues).

Include the reasons why you're requesting the new feature and how it might benefit other Crucible users.

## License

Copyright 2024 Carnegie Mellon University. See the [LICENSE.md](https://github.com/cmu-sei/crucible/blob/main/LICENSE.md) file for details.
