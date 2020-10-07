# Crucible

Developed by Carnegie Mellon University's Software Engineering Institute (SEI), Crucible is a modular framework for creating, deploying, and managing virtual environments to support training, education, and exercises.

## Crucible Stack

Within the Crucible stack are the following applications:

- **Alloy:** Alloy joins the other independent Crucible apps together to provide a complete Crucible experience (i.e. labs, on-demand exercises, exercises, etc.). To get started with Alloy, see: [Alloy API Readme](https://github.com/cmu-sei/crucible/blob/master/alloy.api/README.md) and [Alloy UI Readme](https://github.com/cmu-sei/crucible/blob/master/alloy.ui/README.md) in SEI GitHub.
- **Caster:** Caster is the primary deployment component of the Crucible framework. Caster is  built upon [Terraform](https://www.terraform.io/), an open source "Infrastructure as Code" tool. Caster provides a web interface that gives exercise developers a way to create, share, and manage topology configurations. To get started with Caster, see: Caster API Readme and [Caster UI Readme](https://github.com/cmu-sei/crucible/blob/master/caster.ui/README.md).
- **Steamfitter:** Steamfitter gives exercise developers the ability to create scenarios consisting of a series of scheduled tasks, manual tasks, and injects which run against virtual machines in an exercise. To get started with Steamfitter, see: [Steamfitter API Readme](https://github.com/cmu-sei/crucible/blob/master/steamfitter.api/README.md) and [Steamfitter UI Readme](https://github.com/cmu-sei/crucible/blob/master/steamfitter.ui/README.md).
- **Player:** Player is the centralized interface where users, teams, and administrators go to participate in the cyber exercise.
- **Welder:** Welder is a simple application that can be added to an exercise; Welder allows users to dynamically load a VM workstation.

## Installation
<!--- I would like to get a high-level outline of installation steps and add them here. Perhaps link to more detailed How-To's in [SEI GitHub wiki](https://github.com/cmu-sei/crucible/wiki). --->

## Authors
<!--- What do you think of something like this: The Crucible Stack was built by the development team within the SEI's Mod/Sim and Exercises (MSE) Initiative. Contact them at: [crucible-devs@sei.cmu.edu](mailto:crucible-devs@sei.cmu.edu). I made that alias up. --->


## License
Copyright 2020 Carnegie Mellon University. See the [LICENSE.md](https://github.com/cmu-sei/crucible/blob/master/license.md) file for details.
