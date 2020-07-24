## Crucible Framework

Developed by Carnegie Mellon University's [Software Engineering Institute](https://www.sei.cmu.edu/) (SEI), Crucible is a modular framework for creating, deploying, and managing virtual environments to support training, education, and exercises.

Within the Crucible simulation framework are the following applications:

- **Alloy:** Alloy joins the other independent Crucible apps together to provide a complete Crucible experience. Alloy enables a variety of events from on-demand labs to large exercises and simulations. To get started with Alloy, see: [Alloy API Readme](https://github.com/cmu-sei/crucible/blob/master/alloy.api/README.md) and [Alloy UI Readme](https://github.com/cmu-sei/crucible/blob/master/alloy.ui/README.md) in SEI GitHub.
- **Caster:** Caster is the primary deployment component of the Crucible framework. Caster is built upon [Terraform](https://www.terraform.io/), an open source "Infrastructure as Code" tool. Caster provides a web interface that gives exercise developers a way to create, share, and manage topology configurations. It further leverages GitLab, an open-source code repository, to easily store and share reusable modules. To get started with Caster, see: Caster API Readme and [Caster UI Readme](https://github.com/cmu-sei/crucible/blob/master/caster.ui/README.md).
- **Steamfitter:** Steamfitter gives content developers the ability to create scenarios consisting of a series of scheduled tasks, manual tasks, and injects which run against virtual machines in an event. These scenarios enable the content developer to automate assessments and configurations. To get started with Steamfitter, see: [Steamfitter API Readme](https://github.com/cmu-sei/crucible/blob/master/steamfitter.api/README.md) and [Steamfitter UI Readme](https://github.com/cmu-sei/crucible/blob/master/steamfitter.ui/README.md).
- **Player** is the centralized interface where participants, teams, and administrators go to engage in a cyber event. In Player, participants view teams, applications, virtual environments, and third-party applications. The event experience is highly customizable by content developers.

## License

Copyright 2020 Carnegie Mellon University. See the [LICENSE.md](https://github.com/cmu-sei/crucible/blob/master/alloy.api/license.txt) file for details.

Copyright 2020 Carnegie Mellon University. See the <a href="./license">License file</a> for details.
