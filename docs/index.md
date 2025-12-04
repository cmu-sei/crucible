# ![An icon representing the Crucible application suite icon](assets/cruciblelogo.svg "Crucible Application Suite Icon"){: style="height:75px;width:75px"} Introducing Crucible

**Crucible** is an open-source application framework for creating and managing virtual environments and events.

Since 2018, Crucible has enabled large-scale United States (US) Department of Defense (DoD) cyber exercises, the President's Cybersecurity Challenge Competition, and partner nation information sharing and training initiatives.

## Key Features

- Open-source application framework built on Angular and .NET Core software frameworks
- Modular design - extensive application programming interfaces
- Customizable, immersive, browser-based user interface
- Flexible integration of powerful, third-party, open-source tools
- Scenario-based cyber experimentation, exercises, and challenges
- Model topologies, simulate user activity, script scenario events
- Efficiency through automation
- Interoperability through open standards
- Options for building cyber terrain:
  - "Infrastructure-as-code" for scalability, iteration, and reuse
  - Form-based configuration for simple and quick

## Addressing Persistent Challenges

Crucible confronts challenges faced by platform administrators and content developers:

- manual configurations lead to high-labor costs and excessive human error—limiting scalability and automation
- proprietary range software leads to vendor lock-in and higher costs

## INDIVIDUAL TRAINING

Crucible can be a platform for individual practice or competition. These platforms tend to feature the following two applications:

### Going Simple: Labs/Challenges

[![TopoMojo Logo](assets/topomojo-logo.svg){: style="height:75px;width:75px"}](topomojo/index.md)Crucible's [**TopoMojo**](topomojo/index.md) application enables design of simple labs and challenges using form-based configurations. Select and configure virtual machines, define networks, and write a guide.

Novice Crucible content developers can easily get productive by using TopoMojo. Choose this app when the benefits of more advanced "infrastructure as code" automation are not needed. TopoMojo supports the configuration and deployment of small virtual environments to two types of hypervisors: VMware vSphere ESXi and Proxmox Virtual Environment KVM (open source).

### Crafting a Challenge Competition

[![Gameboard Logo](assets/gameboard-logo.svg){: style="height:75px;width:75px"}](gameboard/index.md) Crucible's [**Gameboard**](gameboard/index.md) application provides game design capabilities and a competition-ready user interface for running your own cybersecurity game.

A Crucible content developer can create, clone, manage, and delete games and challenges—for competition or practice.

## Team Exercising

Crucible can also support more advanced needs commonly found within concept experimentation and team exercising (table-top, functional, and full)—using some of the following ten applications:

### Designing User Experiences

[![Player Logo](assets/player-logo.svg){: style="height:75px;width:75px"}](player/index.md) Crucible's [**Player**](player/index.md) application is the exerciser's window into the virtual environment. Player enables assignment of team membership as well as customization of a responsive, browser-based user interfaces using various integrated applications. A Crucible content developer can shape how integrated applications present scenario information, assessments, and virtual environments.

#### Open-Source Integrations

- **osTicket**, a support ticket system, manages cyber range service requests.
- **Mattermost, Rocket.Chat, Nextcloud Talk** chat services.
- **Stalwart, Roundcube**, web-based email service.

### Coding a Topology

[![Caster Logo](assets/caster-logo.svg){: style="height:75px;width:75px"}](caster/index.md) Crucible's [**Caster**](caster/index.md) application enables coding design and deployment of a cyber topology. With Caster Designs, an intermediate content developer can avoid scripting Terraform code and simply define variables within pre-configured Terraform modules.

Caster supports the design and deployment of virtual environments to a variety of hypervisors. Supported platforms include VMware vSphere ESXi, Microsoft Azure HyperV (public-cloud), Amazon Web Services Xen/Nitro (public-cloud), and Proxmox Virtual Environment KVM (open source).

#### Open-Source Integrations

- **Terraform/OpenTofu**, an "infrastructure-as-code" tool, enables scripted deployment of cyber infrastructure.
- **GitLab**, a version control system and code-repository, stores Terraform/OpenTofu modules.

### Crafting a Scenario

[![Blueprint Logo](assets/blueprint-logo.png)](blueprint/index.md) Crucible's [**Blueprint**](blueprint/index.md) application enables the collaborative creation and visualization of a master scenario event list (MSEL) for an exercise. Blueprint maps scenario events to simulation objectives.

[![Steamfitter Logo](assets/steamfitter-logo.svg){: style="height:75px;width:75px"}](steamfitter/index.md) Crucible's [**Steamfitter**](steamfitter/index.md) application enables the organization and execution of tasks on virtual machines.

#### Open-Source Integrations

- **StackStorm**, an event-driven automation platform, scripts scenario events and senses the virtual environment.
- **Ansible**, a software provisioning, configuration management and application deployment tool, enables post-deployment provisioning of services to infrastructure.

### Animating Activity

[![GHOSTS Logo](assets/ghosts_new.png){: style="height:75px;width:75px"}](https://cmu-sei.github.io/GHOSTS/) Crucible's [**GHOSTS**](https://cmu-sei.github.io/GHOSTS/) Non-Player Character (NPC) automation and orchestration framework deploys and shapes the activities of NPCs using GenAI.

#### Open-Source Integrations

- **Ollama**, a platform designed to run LLaMA, Mistral, and other open source large language models locally.

### Evaluating Threats

[![CITE Logo](assets/cite-logo.png)](cite/index.md) Crucible's [**Collaborative Incident Threat Evaluator (CITE)**](cite/index.md) application enables participants from different organizations to evaluate, score, and comment on cyber incidents. CITE's situational awareness dashboard allows teams to track internal actions and roles.

### Displaying Incident Information

[![Gallery Logo](assets/gallery-logo.png)](gallery/index.md) Crucible's [**Gallery**](gallery/index.md) application enables participants to review cyber incident information by source type (intelligence, reporting, orders, news, social media, telephone, email). Gallery groups that information by critical infrastructure sector or any other organization.

### Assessing Performance

![SEER Logo](assets/seer-logo.svg){: style="height:75px;width:75px"} Crucible's **SEER** application enables assessment of team performance. During events, participants tackle mission-essential tasks and individual qualification requirements. Map performance assessments to training objectives to scenario events.

### Launching an On-Demand Exercise

[![Alloy Logo](assets/alloy-logo.svg){: style="height:75px;width:75px"}](alloy/index.md) Crucible's [**Alloy**](alloy/index.md) application enables users to launch an on-demand event or join an instance of an already-running exercise. Following an event, Alloy can also provide a summary of knowledge and performance assessments.

### Operational Deployment

Crucible applications implement the OpenID Connect authentication protocol and integrate with **Keycloak**, an open-source identity authentication service.

Crucible teams deploy applications as **Docker** containers, which employ operating system level virtualization to isolate containers from each other. They rely on **Kubernetes**, a popular container-orchestration system, to provide container deployment, scaling, and management services. Teams use **Argo CD**, a popular open-source GitOps tool set, to perform Kubernetes workflow and cluster management.

A pre-configured Crucible Appliance virtual machine is available for download.

The SEI owns and operates an on-premises instance of Crucible that can deploy virtual environments to VMware, Proxmox, or to a cloud provider:

#### Fortress

[fortress.sei.cmu.edu](https://fortress.sei.cmu.edu)

![Fortress Logo](assets/fortress-app.svg){: style="height:75px;width:75px"}
