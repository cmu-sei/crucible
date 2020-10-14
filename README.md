# Crucible Framework

Developed by Carnegie Mellon University's Software Engineering Institute (SEI), Crucible is a modular framework for creating, deploying, and managing virtual environments to support training, education, and exercises. Within the Crucible framework are the following applications:

### Alloy

Alloy joins the other independent Crucible apps together to provide a complete Crucible experience (i.e. labs, on-demand exercises, exercises, etc.). To get started with Alloy, see: 
- [Alloy API Repository](https://github.com/cmu-sei/Alloy.Api)
- [Alloy UI Repository](https://github.com/cmu-sei/Alloy.ui)

### Caster

Caster is the primary deployment component of the Crucible framework. Caster is built upon [Terraform](https://www.terraform.io/), an open source "Infrastructure as Code" tool. Caster provides a web interface that gives exercise developers a way to create, share, and manage topology configurations. To get started with Caster, see:
- [Caster API Repository](https://github.com/cmu-sei/Caster.Api)
- [Caster UI Repository](https://github.com/cmu-sei/Caster.Ui)

### osTicket

osTicket (https://osticket.com/) is a widely-used open source support ticket system that can be configured and deployed for an exercise. To get started with the Crucible plugin for osTicket, see:
- [osTicket Repository](https://github.com/cmu-sei/osticket-crucible)

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

### Welder

Welder is a simple application that can be added to an exercise; Welder allows users to dynamically load a VM workstation. To get started with Welder, see:
- [Welder Repository](https://github.com/cmu-sei/Welder)

## License
Copyright 2020 Carnegie Mellon University. See the [LICENSE.md](https://github.com/cmu-sei/crucible/blob/master/license.md) file for details.
