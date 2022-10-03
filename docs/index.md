# Crucible Framework

Developed by Carnegie Mellon University's [Software Engineering Institute](https://www.sei.cmu.edu/) (SEI), Crucible is a modular framework for creating, deploying, and managing virtual environments to support training, education, and exercises.

Within the Crucible simulation framework are the following applications:

**Alloy:** Alloy joins the other independent Crucible apps together to provide a complete Crucible experience. Alloy enables a variety of events from on-demand labs to large exercises and simulations.

**Caster:** Caster is the primary deployment component of the Crucible framework. It is used to populate a Player view with virtual machines. Caster is built upon [Terraform](https://www.terraform.io/), an open source "Infrastructure as Code" tool. Caster provides a web interface that gives exercise developers a way to create, share, and manage topology configurations. It further leverages GitLab, an open-source code repository, to easily store and share reusable modules. 

**Player:** Player is the centralized interface where participants, teams, and administrators go to engage in a cyber event. In Player, participants view teams, applications, virtual environments, and third-party applications. The event experience is highly customizable by content developers.

**Steamfitter:** Steamfitter gives content developers the ability to create scenarios consisting of a series of scheduled tasks, manual tasks, and injects that run against virtual machines in an event. These scenarios enable the content developer to automate assessments and configurations.

**Welder:** Welder is a simple application that can be added to an exercise; Welder allows users to dynamically load a VM workstation.

## License

Copyright 2020 Carnegie Mellon University. See the [LICENSE](https://github.com/cmu-sei/crucible/blob/main/license.md) file for details.
