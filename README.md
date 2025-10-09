# Crucible Framework

Developed by Carnegie Mellon University's Software Engineering Institute (SEI), Crucible is a modular framework for creating, deploying, and managing virtual environments to support training, education, and exercises. Within the Crucible framework are the following applications and plugins.

## Applications

### Alloy

Crucible's [**Alloy**](https://cmu-sei.github.io/crucible/alloy/){ target=_blank } application enables users to launch an on-demand event or join an instance of an already-running simulation. Following the event, reports can provide a summary of knowledge and performance assessments.

- [Alloy API Repository](https://github.com/cmu-sei/Alloy.Api){ target=_blank }
- [Alloy UI Repository](https://github.com/cmu-sei/Alloy.ui){ target=_blank }

### Caster

Crucible's [**Caster**](https://cmu-sei.github.io/crucible/caster/){ target=_blank } application enables the "coded" design and deployment of a cyber topology. Using Caster Designs, a novice content developer can avoid scripting OpenTofu code by simply defining variables within pre-configured OpenTofu modules. Caster supports the design and deployment of virtual environments to three types of hypervisors:

- [Caster API Repository](https://github.com/cmu-sei/Caster.Api){ target=_blank }
- [Caster UI Repository](https://github.com/cmu-sei/Caster.Ui){ target=_blank }

### Player

Crucible's [**Player**](https://cmu-sei.github.io/crucible/player/){ target=_blank } application is the user's window into the virtual environment. Player enables assignment of team membership as well as customization of a responsive, browser-based user-interfaces using various integrated applications. A Crucible system administrator can shape how scenario information, assessments, and virtual environments are presented through the use of integrated applications.

- [Player API Repository](https://github.com/cmu-sei/Player.Api){ target=_blank }
- [Player Console UI Repository](https://github.com/cmu-sei/Console.Ui){ target=_blank }
- [Player UI Repository](https://github.com/cmu-sei/Player.Ui){ target=_blank }
- [Player VM API Repository](https://github.com/cmu-sei/Vm.Api){ target=_blank }
- [Player VM UI Repository](https://github.com/cmu-sei/Vm.Ui){ target=_blank }

### Steamfitter

Crucible's [**Steamfitter**](https://cmu-sei.github.io/crucible/steamfitter/){ target=_blank } application enables the organization and execution of scenario tasks on virtual machines.

- [Steamfitter API Repository](https://github.com/cmu-sei/Steamfitter.Api){ target=_blank }
- [Steamfitter UI Repository](https://github.com/cmu-sei/Steamfitter.Ui){ target=_blank }

Steamfitter relies upon [StackStorm](https://stackstorm.com/){ target=_blank }, an open source event-driven platform used to automate workflows, to execute commands.

### TopoMojo

Crucible's [**TopoMojo**](https://cmu-sei.github.io/crucible/topomojo/about/){ target=_blank } application enables designing simple labs and challenges using form based configurations. Select and configure virtual machines, define networks, and write a guide.

- [TopoMojo API Repository](https://github.com/cmu-sei/TopoMojo){ target=_blank }
- [TopoMojo UI Repository](https://github.com/cmu-sei/topomojo-ui){ target=_blank }

### Gameboard

Crucible's [**Gameboard**](https://cmu-sei.github.io/crucible/Gameboard/){ target=_blank } application provides game design capabilities and a competition-ready user interface for running your own cybersecurity game.

- [Gameboard API Repository](https://github.com/cmu-sei/Gameboard){ target=_blank }
- [Gameboard UI Repository](https://github.com/cmu-sei/Gameboard-ui){ target=_blank }

### Blueprint

Crucible's [**Blueprint**](https://cmu-sei.github.io/crucible/blueprint/){ target=_blank } application enables the collaborative creation and visualization of a master scenario event list (MSEL) for an exercise. Scenario events are mapped to specific simulation objectives.

- [Blueprint API Repository](https://github.com/cmu-sei/Blueprint.Api){ target=_blank }
- [Blueprint UI Repository](https://github.com/cmu-sei/Blueprint.Ui){ target=_blank }

### Gallery

Crucible's [**Gallery**](https://cmu-sei.github.io/crucible/gallery/){ target=_blank } application enables participants to review cyber incident data by source type. Source type examples include: intelligence, reporting, orders, news, social media, telephone, and email. Information is grouped by critical infrastructure sector or other organizational categories.

- [Gallery API Repository](https://github.com/cmu-sei/Gallery.Api){ target=_blank }
- [Gallery UI Repository](https://github.com/cmu-sei/Gallery.Ui){ target=_blank }

### CITE

Crucible's [**Collaborative Incident Threat Evaluator (CITE)**](https://cmu-sei.github.io/crucible/cite/){ target=_blank } application enables participants from different organizations to evaluate, score, and comment on cyber incidents. CITE also provides a situational awareness dashboard that allows teams to track their internal actions and roles.

- [CITE API Repository](https://github.com/cmu-sei/CITE.Api){ target=_blank }
- [CITE UI Repository](https://github.com/cmu-sei/CITE.Ui){ target=_blank }


## Crucible Appliance

The Crucible appliance is an environment that includes everything needed to install and configure the core applications of the Crucible framework. The appliance application stack consists of a single-node Docker swarm utilizing a Traefik reverse proxy. They are assembled using Docker Compose files on an Ubuntu 20.04 operating system. To get started with the Crucible appliance, see:

- [Crucible Appliance Repository](https://github.com/cmu-sei/Crucible.Appliance){ target=_blank }

## Plugins

### Crucible Common Modules

Crucible common modules are a set of Angular modules that are common between Crucible apps. For more information, see:

- [Crucible Common Modules Repository](https://github.com/cmu-sei/Crucible.Common.Ui){ target=_blank }

### Crucible Plugin for Moodle

The Crucible plugin for Moodle is an activity plugin that allows Crucible labs and exercises to be accessed from the Moodle open-source learning management system. For more information, see:

- [Crucible plugin for Moodle Repository](https://github.com/cmu-sei/moodle-mod_crucible){ target=_blank }

### osTicket

[osTicket](https://osticket.com/) is a widely-used open source support ticket system that can be configured and deployed for an exercise. To get started with the Crucible plugin for osTicket, see:

- [osTicket Repository](https://github.com/cmu-sei/osticket-crucible){ target=_blank }

### Terraform Provider for Crucible

This is the [Terraform](https://www.terraform.io/) Provider for Crucible which is used to create many Crucible resource types (e.g., Player Virtual Machines, Views, Applications, and others). For more information, see:

- [Terraform Provider Crucible Repository](https://github.com/cmu-sei/terraform-provider-crucible){ target=_blank }

### Terraform Provider for Identity

This is the [Terraform](https://www.terraform.io/) Provider for [Identity](https://github.com/cmu-sei/Identity){ target=_blank } that creates and manages user accounts and other resources  using the Identity API. For additional information, see:

- [Terraform Provider Identity Repository](https://github.com/cmu-sei/terraform-provider-identity){ target=_blank }

## Documentation

You can find documentation on Crucible and all of its components [here](https://cmu-sei.github.io/crucible/){ target=_blank }.

## Reporting Bugs and Requesting Features

:bug: Think you found a bug? Please report all Crucible bugs - including bugs for the individual Crucible apps - in the [cmu-sei/Crucible issue tracker](https://github.com/cmu-sei/crucible/issues){ target=_blank }.

Include as much detail as possible including steps to reproduce, specific app involved, and any error messages you may have received.

Have a good idea for a new feature? Submit all new feature requests through the [cmu-sei/Crucible issue tracker](https://github.com/cmu-sei/crucible/issues){ target=_blank }.

Include the reasons why you're requesting the new feature and how it might benefit other Crucible users.

## License

Copyright 2024 Carnegie Mellon University. See the [LICENSE.md](https://github.com/cmu-sei/crucible/blob/main/LICENSE.md){ target=_blank } file for details.
