# Crucible

Developed by Carnegie Mellon University's Software Engineering Institute (SEI), Crucible is a modular framework for creating, deploying, and managing virtual environments to support training, education, and exercises.

## Crucible Framework

Within the Crucible framework are the following applications:

#### Alloy
Alloy joins the other independent Crucible apps together to provide a complete Crucible experience (i.e. labs, on-demand exercises, exercises, etc.). To get started with Alloy, see: 
- [Alloy API Readme](https://github.com/cmu-sei/Alloy.Api/blob/development/README.md)
- [Alloy API Repo](https://github.com/cmu-sei/Alloy.Api)
- [Alloy UI Readme](https://github.com/cmu-sei/Alloy.Ui/blob/development/README.md)
- [Alloy UI Repo](https://github.com/cmu-sei/Alloy.ui)

- **Caster:** Caster is the primary deployment component of the Crucible framework. Caster is  built upon [Terraform](https://www.terraform.io/), an open source "Infrastructure as Code" tool. Caster provides a web interface that gives exercise developers a way to create, share, and manage topology configurations. To get started with Caster, see the [Caster API repository](https://github.com/cmu-sei/Caster.Api) in cmu-sei GitHub and the [Caster UI Readme](https://github.com/cmu-sei/Caster.Ui/blob/development/README.md).
- **Steamfitter:** Steamfitter gives exercise developers the ability to create scenarios consisting of a series of scheduled tasks, manual tasks, and injects which run against virtual machines in an exercise. To get started with Steamfitter, see: [Steamfitter API Readme](https://github.com/cmu-sei/Steamfitter.Api/blob/development/README.md) and [Steamfitter UI Readme](https://github.com/cmu-sei/Steamfitter.Ui/blob/development/README.md). Steamfitter relies upon [StackStorm](https://stackstorm.com/), an open source event-driven platform used to automate workflows, to execute commands. 
- **Player:** Player is the centralized interface where users, teams, and administrators go to participate in the cyber exercise. To get started with Player, see: [Player API Readme](https://github.com/cmu-sei/Player.Api/blob/development/README.md) and [Player UI Readme](https://github.com/cmu-sei/Player.Ui/blob/development/README.md).
- **Welder:** Welder is a simple application that can be added to an exercise; Welder allows users to dynamically load a VM workstation. See the [Welder repository](https://github.com/cmu-sei/Welder) in cmu-sei GitHub.

## Installation
<!--- I would like to get a high-level outline of installation steps and add them here. Perhaps link to more detailed How-To's in [SEI GitHub wiki](https://github.com/cmu-sei/crucible/wiki). --->

## Authors
<!--- What do you think of something like this: The Crucible Stack was built by the development team within the SEI's Mod/Sim and Exercises (MSE) Initiative. Contact them at: [crucible-devs@sei.cmu.edu](mailto:crucible-devs@sei.cmu.edu). I made that alias up. --->

## License
Copyright 2020 Carnegie Mellon University. See the [LICENSE.md](https://github.com/cmu-sei/crucible/blob/master/license.md) file for details.
